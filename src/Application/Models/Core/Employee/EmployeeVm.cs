namespace EduCare.Application.Models.Core.Employee;

public class EmployeeVm
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? FacultyId { get; set; }
    public DateTime CreatedOn { get; set; }
}