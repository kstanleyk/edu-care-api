using System.Collections.ObjectModel;

namespace EduCare.Application.Authorization;

public class AppPermission(string feature, string action, string group, string description, bool isBasic = false)
{
    public string Feature { get; } = feature;
    public string Action { get; } = action;
    public string Group { get; } = group;
    public string Description { get; } = description;
    public bool IsBasic { get; } = isBasic;

    public string Name => NameFor(Feature, Action);

    public static string NameFor(string feature, string action) => $"Permission.{feature}.{action}";
}

public class AppPermissions
{
    private static readonly AppPermission[] All =
    [
        new AppPermission(AppFeature.Users, AppAction.Create, AppRoleGroup.SystemAccess, "Create Users"),
        new AppPermission(AppFeature.Users, AppAction.Update, AppRoleGroup.SystemAccess, "Update Users"),
        new AppPermission(AppFeature.Users, AppAction.Read, AppRoleGroup.SystemAccess, "Read Users"),
        new AppPermission(AppFeature.Users, AppAction.Delete, AppRoleGroup.SystemAccess, "Delete Users"),

        new AppPermission(AppFeature.UserRoles, AppAction.Read, AppRoleGroup.SystemAccess, "Read User Roles"),
        new AppPermission(AppFeature.UserRoles, AppAction.Update, AppRoleGroup.SystemAccess, "Update User Roles"),

        new AppPermission(AppFeature.Roles, AppAction.Read, AppRoleGroup.SystemAccess, "Read Roles"),
        new AppPermission(AppFeature.Roles, AppAction.Create, AppRoleGroup.SystemAccess, "Create Roles"),
        new AppPermission(AppFeature.Roles, AppAction.Update, AppRoleGroup.SystemAccess, "Update Roles"),
        new AppPermission(AppFeature.Roles, AppAction.Delete, AppRoleGroup.SystemAccess, "Delete Roles"),

        new AppPermission(AppFeature.RoleClaims, AppAction.Read, AppRoleGroup.SystemAccess, "Read Role Claims/Permissions"),
        new AppPermission(AppFeature.RoleClaims, AppAction.Update, AppRoleGroup.SystemAccess, "Update Role Claims/Permissions"),

        new AppPermission(AppFeature.Client, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Client", true),
        new AppPermission(AppFeature.Client, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Client"),
        new AppPermission(AppFeature.Client, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Client"),

        new AppPermission(AppFeature.Wallet, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Wallet", true),
        new AppPermission(AppFeature.Wallet, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Wallet"),
        new AppPermission(AppFeature.Wallet, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Wallet"),
        new AppPermission(AppFeature.Wallet, AppAction.Deposit, AppRoleGroup.ManagementHierarchy, "RequestDeposit funds into wallet"),
        new AppPermission(AppFeature.Wallet, AppAction.Withdraw, AppRoleGroup.ManagementHierarchy, "RequestWithdrawal funds from wallet"),
        new AppPermission(AppFeature.Wallet, AppAction.Approve, AppRoleGroup.ManagementHierarchy, "Approve funds deposit"),
    ];

    public static IReadOnlyList<AppPermission> AdminPermissions { get; } =
        new ReadOnlyCollection<AppPermission>(All.Where(p => !p.IsBasic).ToArray());

    public static IReadOnlyList<AppPermission> BasicPermissions { get; } =
        new ReadOnlyCollection<AppPermission>(All.Where(p => p.IsBasic).ToArray());

    public static IReadOnlyList<AppPermission> AllPermissions { get; } =
        new ReadOnlyCollection<AppPermission>(All);

    public static IReadOnlyList<FeatureWithAction> AppFeatures { get; } =
        AppPermissions.AllPermissions
            .GroupBy(p => p.Feature)
            .Select(g => new FeatureWithAction
            {
                Feature = g.Key,
                Actions = g.Select(p => new ActionDetail
                {
                    Action = p.Action,
                    Description = p.Description,
                    IsBasic = p.IsBasic
                }).ToList()
            })
            .ToList();

}

public class ActionDetail
{
    public required string Action { get; set; }
    public required string Description { get; set; }
    public bool IsBasic { get; set; }
}

public class FeatureWithAction
{
    public required string Feature { get; set; }
    public required List<ActionDetail> Actions { get; set; }
}