using Microsoft.AspNetCore.Authorization;

namespace EduCare.CoreApi.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; set; } = permission;
}