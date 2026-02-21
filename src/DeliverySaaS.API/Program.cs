using System.Security.Claims;
using System.Text;
using DeliverySaaS.API.Authorization;
using DeliverySaaS.API.Extensions;
using DeliverySaaS.Application.Accounting;
using DeliverySaaS.API.Security;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Identity.Enums;
using DeliverySaaS.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<RequestContext>();
builder.Services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderProblemService, OrderProblemService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
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

    options.AddPolicy("BranchScope", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("branch_id", _ => true) &&
        !ctx.User.IsInRole("CompanyAdmin") &&
        !ctx.User.IsInRole("SaaSAdmin")));

    options.AddPolicy("CompanyScope", p => p.RequireRole("CompanyAdmin"));
    options.AddPolicy("SaaSScope", p => p.RequireRole("SaaSAdmin"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseTenantBranchExtraction();
app.UseAuthorization();
app.MapControllers();

app.Run();
