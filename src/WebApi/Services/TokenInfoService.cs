using System.Security.Claims;

namespace EduCare.CoreApi.Services;

public class TokenInfoService(IHttpContextAccessor httpContextAccessor)
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? GetClientId() => User?.FindFirst("client_id")?.Value;
    public string? GetSubject() => User?.FindFirst("sub")?.Value;
    public string? GetOiPrst() => User?.FindFirst("oi_prst")?.Value;
    public string? GetTokenId() => User?.FindFirst("oi_tkn_id")?.Value;
}