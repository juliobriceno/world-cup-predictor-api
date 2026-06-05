using System.Security.Claims;

namespace Goal2026API.Common;

public static class ClaimsPrincipalExtensions
{
    public static string GetFirebaseUid(this ClaimsPrincipal user)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(uid))
        {
            throw new UnauthorizedAccessException("Firebase UID claim was not found.");
        }

        return uid;
    }
}