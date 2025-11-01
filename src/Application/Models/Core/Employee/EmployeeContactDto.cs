namespace EduCare.Application.Models.Core.Employee;

public class EmployeeContactRequest
{
    public Guid Id { get; set; }
    public PhoneNumberDto Phone1 { get; set; } = null!;
    public PhoneNumberDto? Phone2 { get; set; } = null!;
    public string WorkEmail { get; set; } = null!;
    public string? PersonalEmail { get; set; } = null!;
    public string EmergencyContactName { get; set; } = null!;
    public PhoneNumberDto EmergencyContactNumber { get; set; } = null!;
}

public class PhoneNumberDto
{
    public string CountryCode { get; set; } = null!;
    public string Number { get; set; } = null!;
}