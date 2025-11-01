using System.Security.Claims;

namespace EduCare.CoreApi.Services;

public static class ClaimsPrincipalExtensions
{
    public static string? GetSubject(this ClaimsPrincipal user) =>
        user.FindFirst("sub")?.Value;

    public static string? GetClientId(this ClaimsPrincipal user) =>
        user.FindFirst("client_id")?.Value;

    public static string? GetOiPrst(this ClaimsPrincipal user) =>
        user.FindFirst("oi_prst")?.Value;

    public static string? GetTokenId(this ClaimsPrincipal user) =>
        user.FindFirst("oi_tkn_id")?.Value;
}