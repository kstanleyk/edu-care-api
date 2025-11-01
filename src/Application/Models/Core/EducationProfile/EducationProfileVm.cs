namespace EduCare.Application.Models.Core.EducationProfile;

public class EducationProfileVm
{
    public string Id { get; set; } = null!;
    public Guid? PublicId { get; set; }
    public string EmployeeId { get; set; } = null!;
    public string EducationLevelId { get; set; } = null!;
    public string SchoolAttended { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public string CertificateObtained { get; set; } = null!;
    public string SchoolLocation { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}

public class EducationProfileCodes
{
    public IEnumerable<string> EducationLevelIds { get; set; } = [];
    public IEnumerable<string> EmployeeIds { get; set; } = [];
}