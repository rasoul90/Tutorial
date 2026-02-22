using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using DeliverySaaS.API.Authorization;
using DeliverySaaS.API.Extensions;
using DeliverySaaS.API.Middleware;
using DeliverySaaS.API.Security;
using DeliverySaaS.API.Validation;
using DeliverySaaS.Application.Accounting;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Integration;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Application.Payments;
using DeliverySaaS.Domain.Identity.Enums;
using DeliverySaaS.Infrastructure.DependencyInjection;
using DeliverySaaS.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSettlementRequestValidator>();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("MobilePolicy", p =>
    {
        p.Window = TimeSpan.FromMinutes(1);
        p.PermitLimit = 60;
        p.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("IntegrationPolicy", p =>
    {
        p.Window = TimeSpan.FromMinutes(1);
        p.PermitLimit = 120;
        p.QueueLimit = 0;
    });
});

builder.Services.AddScoped<RequestContext>();
builder.Services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderProblemService, OrderProblemService>();
builder.Services.AddScoped<IOrderMobileService, OrderMobileService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IMerchantPaymentsService, MerchantPaymentsService>();
builder.Services.AddSingleton<IClaimsTransformation, RolePermissionClaimsTransformation>();

builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.CanViewOrders,
        p => p.RequireClaim("permission", AuthorizationPolicies.PermissionValue(Permission.OrdersView)));

    options.AddPolicy(AuthorizationPolicies.CanTransitionOrders,
        p => p.RequireClaim("permission", AuthorizationPolicies.PermissionValue(Permission.OrdersTransition)));

    options.AddPolicy(AuthorizationPolicies.CanManageOrderProblems,
        p => p.RequireClaim("permission", AuthorizationPolicies.PermissionValue(Permission.OrderProblemsManage)));

    options.AddPolicy(AuthorizationPolicies.CanViewFinancialReports,
        p => p.RequireAssertion(ctx =>
            ctx.User.HasClaim("permission", AuthorizationPolicies.PermissionValue(Permission.FinReportsView)) ||
            ctx.User.HasClaim("permission", AuthorizationPolicies.PermissionValue(Permission.BranchReportsView))));

    options.AddPolicy(AuthorizationPolicies.CanViewCompanyReports,
        p => p.RequireClaim("permission", AuthorizationPolicies.PermissionValue(Permission.TenantReportsView)));

    options.AddPolicy(AuthorizationPolicies.CanManageMerchantPayments,
        p => p.RequireClaim("permission", AuthorizationPolicies.PermissionValue(Permission.MerchantPaymentsManage)));

    options.AddPolicy("BranchScope", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("branch_id", _ => true) &&
        !ctx.User.IsInRole("CompanyAdmin") &&
        !ctx.User.IsInRole("SaaSAdmin")));

    options.AddPolicy("CompanyScope", p => p.RequireRole("CompanyAdmin"));
    options.AddPolicy("SaaSScope", p => p.RequireRole("SaaSAdmin"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigration");
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var retries = 10;

    while (retries-- > 0)
    {
        try
        {
            dbContext.Database.Migrate();
            break;
        }
        catch (Exception ex) when (retries > 0)
        {
            logger.LogWarning(ex, "Database migration failed. Retrying... Remaining retries: {Retries}", retries);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseTenantBranchExtraction();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
