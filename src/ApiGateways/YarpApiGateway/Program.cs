using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilderContext =>
    {
        transformBuilderContext.AddRequestTransform(async transformContext =>
        {
            var user = transformContext.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var sub = user.FindFirst("sub")?.Value;
                var roles = user.FindAll("role").Select(c => c.Value).ToList();

                if (!string.IsNullOrEmpty(sub))
                    transformContext.ProxyRequest.Headers.Add("X-User-Id", sub);
                if (roles.Any())
                    transformContext.ProxyRequest.Headers.Add("X-User-Roles", string.Join(",", roles));
            }
        });
    });

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed-window-policy", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 5;
    });
});

// ---- 2. Token validation
var identityServerAuthority = builder.Configuration["IdentityServer:Authority"]
    ?? "http://identityserver:8080";

// EmitStaticAudienceClaim=true  =>  JWT aud is ALWAYS {authority}/resources
var tokenAudience = $"{identityServerAuthority}/resources";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityServerAuthority;
        options.RequireHttpsMetadata = false;   // dev only — http inside docker
        options.Audience = tokenAudience;       // NOT "yarp-gateway"
        options.MapInboundClaims = false;       // keep sub/role claim names intact
        options.TokenValidationParameters.RoleClaimType = "role";
        options.TokenValidationParameters.NameClaimType = "sub";
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRateLimiter();

app.UseAuthentication();    // NEW — parse the Bearer token
app.UseAuthorization();     // NEW — enforce route AuthorizationPolicy

app.MapReverseProxy();

app.Run();
