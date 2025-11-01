namespace EduCare.Application.Models.Core.Employee;

public class EmployeeMaritalRequest
{
    public Guid Id { get; set; }
    public string MaritalStatusId { get; set; } = null!;
    public string SpouseName { get; set; } = null!;
    public string MarriageCertificateNumber { get; set; } = null!;
    public DateTime? MarriageDate { get; set; } = null!;
}