namespace EduCare.Domain.Entity.Auth;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public required DateTime CreatedOn { get; set; }

    public string PermissionId { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}