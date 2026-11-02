using BuildingBlocks.Exceptions;

namespace BuildingBlocks.Authentication;

public static class UserContextExtensions
{
    /// <summary>
    /// The caller key, or a 400 when the token carries neither <c>sub</c> nor <c>client_id</c>.
    /// </summary>
    public static string RequireUserKey(this IUserContext userContext) =>
        userContext.UserKey
        ?? throw new BadRequestException(
            "The access token carries no user or client identity (no 'sub' or 'client_id' claim).");

    /// <summary>
    /// The caller's <c>sub</c> as a GUID (the Ordering customer id).
    /// Only user tokens have one — a machine token cannot check out a basket.
    /// </summary>
    public static Guid RequireUserId(this IUserContext userContext) =>
        userContext.UserId
        ?? throw new BadRequestException(
            "This operation requires a user token with a GUID 'sub' claim; client_credentials tokens have none.");
}
