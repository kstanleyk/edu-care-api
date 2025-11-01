namespace EduCare.Application.Models.Core.EducationLevel;

public class CreateEducationLevelRequest
{
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}