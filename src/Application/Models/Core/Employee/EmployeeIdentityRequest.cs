namespace EduCare.Application.Models.Core.Employee;

public class EmployeeIdentityRequest
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;
    public DateTime? IssueDate { get; set; } = null!;
    public string PlaceOfIssue { get; set; } = null!;
}