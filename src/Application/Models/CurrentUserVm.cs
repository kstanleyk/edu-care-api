namespace EduCare.Application.Models;

public class CurrentUserVm
{
    public string Tenant { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string Branch { get; set; }
    public string Subject { get; set; }
    public string ImageUrl { get; set; }
    public string UserAgent { get; set; }
}