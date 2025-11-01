namespace EduCare.Application.Models.Core.Employee;

public class EmployeeIdentityVm
{
    public string Id { get; set; } = null!;
    public string Number { get; set; } = null!;
    public DateTime? IssueDate { get; set; } = null!;
    public string PlaceOfIssue { get; set; } = null!;
}