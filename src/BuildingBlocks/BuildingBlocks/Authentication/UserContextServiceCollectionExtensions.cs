using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Authentication;

public static class UserContextServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IUserContext"/> (plus the <see cref="IHttpContextAccessor"/> it needs).
    /// Call it after <c>AddIdentityServerAuthentication</c> in any service that stores data per user.
    /// </summary>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();

        return services;
    }
}
