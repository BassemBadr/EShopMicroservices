using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Authentication;

public static class IdentityServerAuthenticationExtensions
{
    public static IServiceCollection AddIdentityServerAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["IdentityServer:Authority"]
            ?? throw new InvalidOperationException("IdentityServer:Authority not configured");

        // Static audience claim (EmitStaticAudienceClaim=true) => aud is always {authority}/resources
        var audience = $"{authority}/resources";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = false;   // dev only — http inside docker
                options.Audience = audience;            // NOT a per-API name
                options.MapInboundClaims = false;       // keep sub/role names
                options.TokenValidationParameters.RoleClaimType = "role";
                options.TokenValidationParameters.NameClaimType = "sub";
            });

        services.AddAuthorization(options =>
        {
            // Secure-by-default: every endpoint requires a valid token
            // (Carter endpoints AND gRPC methods), unless AllowAnonymous.
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
