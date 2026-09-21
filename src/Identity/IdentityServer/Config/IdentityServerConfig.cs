using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServer.Config;

public class IdentityServerConfig
{
    // ---- API Scopes: what the protected APIs are called in the token world
    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("catalog-api", "Catalog Service"),
            new ApiScope("basket-api", "Basket Service"),
            new ApiScope("ordering-api", "Ordering Service"),
            new ApiScope("discount-api", "Discount Service"),
        ];

    // ---- Ide
    // ntity Resources: user profile claims that come with tokens
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("roles", "User roles", ["role"]),
        ];

    // ---- Clients: the applications allowed to request tokens
    public static IEnumerable<Client> Clients =>
        [
            // Shopping.Web — interactive user login (Authorization Code + PKCE)
            new Client
            {
                ClientId = "shopping-web",
                ClientName = "Shopping Web Application",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,          // public client (SaaS/Razor web app)

                RedirectUris = { "https://localhost:5055/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5055/signout-callback-oidc" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "roles",
                    "catalog-api",
                    "basket-api",
                    "ordering-api",
                },

                AllowOfflineAccess = true,            // enables refresh tokens
                RequireConsent = false,               // skip consent screen for our own app
            },

            // YarpApiGateway — machine-to-machine (used in a later phase)
            new Client
            {
                ClientId = "yarp-gateway",
                ClientName = "YARP API Gateway",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("gateway-secret".Sha256()) },
                AllowedScopes = { "catalog-api", "basket-api", "ordering-api", "discount-api" },
            }
        ];
}
