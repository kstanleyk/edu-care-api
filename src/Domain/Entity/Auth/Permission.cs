namespace EduCare.Domain.Entity.Auth;

public class Permission
{
    public required string Id { get; set; }
    public required string Feature { get; set; }
    public required string Action { get; set; }
    public required string Group { get; set; }
    public string? Description { get; set; }
    public bool IsBasic { get; set; }
    public required DateTime CreatedOn { get; set; }

    public static Permission Create(string id, string? feature, string? action, string? group, string? description, bool isBasic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Permission
        {
            Id = id,
            Feature = feature,
            Action = action,
            Group = group,
            Description = description,
            IsBasic = isBasic,
            CreatedOn = DateTime.UtcNow
        };
    }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}