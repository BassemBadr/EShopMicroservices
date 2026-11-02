using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Authentication;

/// <summary>
/// Reads the caller identity from <see cref="HttpContext.User"/>.
/// </summary>
/// <remarks>
/// Services call <c>MapInboundClaims = false</c>, so the JWT claim types stay raw
/// (<c>sub</c>, <c>role</c>, <c>name</c>). The <see cref="ClaimTypes"/> aliases are accepted
/// as well so cookie principals (which use the default inbound mapping) resolve the same way.
/// </remarks>
public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private const string SubjectClaim = "sub";
    private const string ClientIdClaim = "client_id";
    private const string RoleClaim = "role";
    private const string NameClaim = "name";

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? SubjectId => Find(SubjectClaim, ClaimTypes.NameIdentifier);

    public Guid? UserId => Guid.TryParse(SubjectId, out var userId) ? userId : null;

    public string? ClientId => Find(ClientIdClaim);

    public string? UserKey =>
        SubjectId ?? (ClientId is { Length: > 0 } clientId ? $"client:{clientId}" : null);

    public string UserName => Find(NameClaim, ClaimTypes.Name) ?? UserKey ?? "anonymous";

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var principal = Principal;

            if (principal is null) return [];

            return principal.FindAll(RoleClaim)
                .Concat(principal.FindAll(ClaimTypes.Role))
                .Select(claim => claim.Value)
                .Distinct()
                .ToArray();
        }
    }

    public bool IsInRole(string role) => Roles.Contains(role);

    private string? Find(params string[] claimTypes)
    {
        var principal = Principal;

        if (principal is null) return null;

        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;

            if (!string.IsNullOrEmpty(value)) return value;
        }

        return null;
    }
}
