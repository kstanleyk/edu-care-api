namespace EduCare.Application.Models.Core.EducationLevel;

public class EducationLevelVm
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}