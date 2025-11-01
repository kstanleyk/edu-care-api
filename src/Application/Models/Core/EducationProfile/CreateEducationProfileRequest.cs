namespace EduCare.Application.Models.Core.EducationProfile;

public class CreateEducationProfileRequest
{
    public string EmployeeId { get; set; } = null!;
    public string EducationLevelId { get; set; } = null!;
    public string SchoolAttended { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public string CertificateObtained { get; set; } = null!;
    public string SchoolLocation { get; set; } = null!;
}