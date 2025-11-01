using AutoMapper;
using EduCare.Application.Features.Core.BursaryManagement.Dtos;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;

namespace EduCare.Application.MappingProfile;

public class BalanceProfile : Profile
{
    public BalanceProfile()
    {
        CreateMap<Enrollment, BalanceDto>()
            .ForMember(dest => dest.TotalFees,
                opt => opt.MapFrom(src => src.CalculateTotalFees()))
            .ForMember(dest => dest.TotalPaid,
                opt => opt.MapFrom(src => src.CalculateTotalPaid()))
            .ForMember(dest => dest.ScholarshipDiscount,
                opt => opt.MapFrom(src => CalculateScholarshipDiscount(src)))
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.CalculateBalance()));
    }

    private static Money CalculateScholarshipDiscount(Enrollment enrollment)
    {
        var totalFees = enrollment.CalculateTotalFees();
        var activeScholarships = enrollment.Scholarships.Where(s => s.IsActive).ToArray();

        if (!activeScholarships.Any())
            return new Money(0);

        var totalPercentage = activeScholarships.Sum(s => s.Percentage);
        var maxPercentage = Math.Min(totalPercentage, 100);

        var discountAmount = totalFees.Amount * (maxPercentage / 100);
        return new Money(discountAmount);
    }
}