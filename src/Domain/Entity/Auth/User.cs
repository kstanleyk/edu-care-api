namespace EduCare.Domain.Entity.Auth;

public class User 
{
    public required Guid Id { get; set; }
    public required string IdentityId { get; set; }
    public required string Email { get; set; }
    public string? FullName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public required DateTime CreatedOn { get; set; }

    public static User Create(Guid id, string identityId, string email, string? fullName, string? profileImageUrl)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileImageUrl);

        return new User
        {
            Id = id,
            IdentityId = identityId,
            Email = email,
            FullName = fullName,
            ProfileImageUrl = profileImageUrl,
            CreatedOn = DateTime.UtcNow
        };
    }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}