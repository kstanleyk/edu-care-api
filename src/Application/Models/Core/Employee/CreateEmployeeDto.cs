namespace EduCare.Application.Models.Core.Employee;

public class CreateEmployeeDto
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PlaceOfBirth { get; set; } = null!;
    public string BirthCertificateNumber { get; set; } = null!;
    public string GenderId { get; set; } = null!;
    public string FatherName { get; set; } = null!;
    public string MotherName { get; set; } = null!;
    public string CountryId { get; set; } = null!;
    public string NicNumber { get; set; } = null!;
    public DateTime NicIssuedOn { get; set; }
    public string NicIssuedAt { get; set; } = null!;
    public string Phone1 { get; set; } = null!;
    public string? Phone2 { get; set; }
    public string WorkEmail { get; set; } = null!;
    public string? PersonalEmail { get; set; }
}