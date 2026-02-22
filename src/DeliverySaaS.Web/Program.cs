using DeliverySaaS.Web.Middleware;
using DeliverySaaS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(cfg["Api:BaseUrl"] ?? "http://localhost:8080");
}).AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<TokenContextMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
