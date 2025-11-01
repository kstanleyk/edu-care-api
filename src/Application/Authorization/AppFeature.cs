namespace EduCare.Application.Authorization;

public static class AppFeature
{
    //IdentityInfo
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string UserRoles = nameof(UserRoles);
    public const string RoleClaims = nameof(RoleClaims);

    //Core
    public const string Client = nameof(Client);
    public const string Wallet = nameof(Wallet);
}
