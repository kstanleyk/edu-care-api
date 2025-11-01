namespace EduCare.Application.Models.Core.Employee;

public class EmployeeEmploymentVm
{
    public string Id { get; set; } = null!;
    public DateTime? EmploymentDate { get; set; } = null!;
    public DateTime? RetirementDate { get; set; } = null!;
    public string EmploymentPlace { get; set; } = null!;
    public string EmploymentAuthority { get; set; } = null!;
    public string EmploymentStatusId { get; set; } = null!;
}