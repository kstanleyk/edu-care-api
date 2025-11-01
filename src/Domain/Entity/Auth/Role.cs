namespace EduCare.Domain.Entity.Auth;

public class Role
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime CreatedOn { get; set; }

    public static Role Create(string? name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedOn = DateTime.UtcNow
        };
    }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}