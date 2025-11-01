using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Auth;
using EduCare.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Auth;

public class UserPermissionRepository(EduCareContext context) :Disposable, IUserPermissionRepository
{
    public async Task<HashSet<string>> GetPermissionsForUserAsync(string userId)
    {
        var usrId = await context.UserSet
            .Where(u => u.IdentityId == userId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var permissions = await context.UserRoleSet
            .Where(ur => ur.UserId == usrId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Id)
            .Distinct()
            .ToListAsync();

        return permissions.ToHashSet();
    }

    //protected override void DisposeCore()
    //{
    //    base.DisposeCore();
    //}
}
