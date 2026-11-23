using System.Security.Claims;

namespace CloudKitchen_API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? throw new UnauthorizedAccessException("User id missing from token.");
        return Guid.Parse(id);
    }
}