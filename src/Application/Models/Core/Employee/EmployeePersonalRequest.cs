namespace EduCare.Application.Models.Core.Employee;

public class EmployeePersonalRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string PlaceOfBirth { get; set; } = null!;
    public string BirthCertificateNumber { get; set; } = null!;
    public string GenderId { get; set; } = null!;
    public string FatherName { get; set; } = null!;
    public string MotherName { get; set; } = null!;
}