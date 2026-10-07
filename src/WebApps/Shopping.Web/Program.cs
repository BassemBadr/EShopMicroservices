using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Shopping.Web.HttpHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddRefitClient<ICatalogService>()
     .ConfigureHttpClient(c =>
     {
         c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
     })
     .AddHttpMessageHandler<UserAccessTokenHandler>();
builder.Services.AddRefitClient<IBasketService>()
     .ConfigureHttpClient(c =>
     {
         c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
     })
     .AddHttpMessageHandler<UserAccessTokenHandler>();
builder.Services.AddRefitClient<IOrderingService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<UserAccessTokenHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["IdentityServer:Authority"];
    options.ClientId = "shopping-web";
    // shopping-web is a PUBLIC client (RequireClientSecret=false, PKCE):
    // do NOT set ClientSecret here — IdentityServer rejects secrets on public clients.
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;                       // keep access_token for the handler
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("catalog-api");
    options.Scope.Add("basket-api");
    options.Scope.Add("ordering-api");
    options.Scope.Add("offline_access");             // refresh token (client has AllowOfflineAccess)
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<UserAccessTokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
