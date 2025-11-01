namespace EduCare.Application.Models.Core.Employee;

public class EmployeeEmploymentRequest
{
    public Guid Id { get; set; } 
    public DateTime EmploymentDate { get; set; } 
    public DateTime? RetirementDate { get; set; } = null!;
    public string EmploymentPlace { get; set; } = null!;
    public string EmploymentAuthority { get; set; } = null!;
    public string EmploymentStatusId { get; set; } = null!;
}