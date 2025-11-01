namespace EduCare.Application.Models.Core.Country;

public class CreateCountryRequest
{
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}