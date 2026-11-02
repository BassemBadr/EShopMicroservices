namespace BuildingBlocks.Authentication;

/// <summary>
/// The identity of the current caller, read from the validated access token only.
/// Never derive this from a route value, query string, request body or forwarded header.
/// </summary>
public interface IUserContext
{
    /// <summary>True when the request carried a token that passed validation.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The <c>sub</c> claim. For this IdentityServer that is the ASP.NET Identity user id
    /// (a GUID); client_credentials tokens do not have one.
    /// </summary>
    string? SubjectId { get; }

    /// <summary><see cref="SubjectId"/> parsed as a GUID — the value Ordering stores as the customer id.</summary>
    Guid? UserId { get; }

    /// <summary>The <c>client_id</c> claim, present on machine (client_credentials) tokens.</summary>
    string? ClientId { get; }

    /// <summary>
    /// Stable per-caller key used as the basket owner and the audit author:
    /// the user's <c>sub</c> for user tokens, otherwise <c>client:{client_id}</c>.
    /// Null when the request is anonymous.
    /// </summary>
    string? UserKey { get; }

    /// <summary>Display name (<c>name</c> claim), falling back to <see cref="UserKey"/>.</summary>
    string UserName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);

    /// <summary>
    /// Declares who the rest of this scope is acting for. Needed by background work: a MassTransit
    /// consumer has no <c>HttpContext</c>, so without this the audit columns would record
    /// <c>"system"</c> even though the work was triggered by a real user. Only the identity keys
    /// change; <see cref="IsAuthenticated"/> and <see cref="Roles"/> keep describing the request.
    /// </summary>
    /// <param name="userKey">The <see cref="UserKey"/> the original caller had.</param>
    /// <param name="userId">The original caller's <see cref="UserId"/>, when it is a user token.</param>
    void ActAs(string userKey, Guid? userId = null);
}
