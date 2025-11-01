namespace EduCare.Application.Models.Core.Employee;

public class EmployeeNationalityRequest
{
    public Guid Id { get; set; }
    public string Village { get; set; } = null!;
    public string SubDivisionId { get; set; } = null!;
    public string CountryId { get; set; } = null!;
}