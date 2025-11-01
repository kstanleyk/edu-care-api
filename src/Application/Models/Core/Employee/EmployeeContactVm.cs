namespace EduCare.Application.Models.Core.Employee;

public class EmployeeContactVm
{
    public string Id { get; set; } = null!;
    public PhoneNumberVm Phone1 { get; set; } = null!;
    public PhoneNumberVm? Phone2 { get; set; } = null!;
    public string WorkEmail { get; set; } = null!;
    public string PersonalEmail { get; set; } = null!;
    public string EmergencyContactName { get; set; } = null!;
    public PhoneNumberVm EmergencyContactNumber { get; set; } = null!;
}

public class PhoneNumberVm
{
    public string CountryCode { get; set; } = null!;
    public string Number { get; set; } = null!;
}