using EduCare.Application.Features.Core.BursaryManagement.Dtos;
using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record GetEnrollmentFeeBreakdownQuery(Guid EnrollmentId) : IRequest<Result<FeeBreakdownDto>>;

public class GetEnrollmentFeeBreakdownQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    IStudentRepository studentRepository,
    IClassRepository classRepository)
    : IRequestHandler<GetEnrollmentFeeBreakdownQuery, Result<FeeBreakdownDto>>
{
    public async Task<Result<FeeBreakdownDto>> Handle(GetEnrollmentFeeBreakdownQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate enrollment exists
            var enrollment = await enrollmentRepository.GetByIdWithDetailsAsync(query.EnrollmentId);
            if (enrollment is null)
            {
                return Result<FeeBreakdownDto>.Failed(
                    Error.NotFound("Enrollment.NotFound", $"Enrollment with ID {query.EnrollmentId} was not found")
                );
            }

            // Validate enrollment is active
            if (!enrollment.IsActive)
            {
                return Result<FeeBreakdownDto>.Failed(
                    Error.Validation("Enrollment.Inactive", $"Enrollment with ID {query.EnrollmentId} is not active")
                );
            }

            // Get additional context for validation
            var student = await studentRepository.GetAsync(enrollment.StudentId);
            var classEntity = await classRepository.GetAsync(enrollment.ClassId);

            if (student is null || classEntity is null)
            {
                return Result<FeeBreakdownDto>.Failed(
                    Error.NotFound(
                        "RelatedEntity.NotFound",
                        "Related student or class information not found"
                    )
                );
            }

            // Calculate all financial details
            var totalMandatoryFees = enrollment.FeeStructure.CalculateTotalFees();
            var totalOptionalFees = CalculateTotalOptionalFees(enrollment.FeeStructure);
            var totalSelectedOptionalFees = CalculateTotalSelectedOptionalFees(enrollment);
            var scholarshipDiscount = CalculateScholarshipDiscount(enrollment);
            var totalNetFees = CalculateTotalNetFees(totalMandatoryFees, totalSelectedOptionalFees, scholarshipDiscount);
            var totalPaid = enrollment.CalculateTotalPaid();
            var balance = enrollment.CalculateBalance();

            // Map fee items to detail DTOs
            var feeItems = MapFeeItemsToDetailDtos(enrollment);

            var feeBreakdownDto = new FeeBreakdownDto(
                EnrollmentId: enrollment.Id,
                TotalMandatoryFees: totalMandatoryFees,
                TotalOptionalFees: totalOptionalFees,
                TotalSelectedOptionalFees: totalSelectedOptionalFees,
                ScholarshipDiscount: scholarshipDiscount,
                TotalNetFees: totalNetFees,
                TotalPaid: totalPaid,
                Balance: balance,
                FeeItems: feeItems
            );

            return Result<FeeBreakdownDto>.Succeeded(feeBreakdownDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting fee breakdown for enrollment {EnrollmentId}", query.EnrollmentId);

            return Result<FeeBreakdownDto>.Failed(
                Error.Failure(
                    "FeeBreakdownQuery.Failed",
                    $"An error occurred while retrieving the fee breakdown: {ex.Message}"
                )
            );
        }
    }

    private static Money CalculateTotalOptionalFees(FeeStructure feeStructure)
    {
        var optionalFeesTotal = feeStructure.FeeItems
            .Where(fi => fi.IsOptional)
            .Sum(fi => fi.Amount.Amount);

        return new Money(optionalFeesTotal);
    }

    private static Money CalculateTotalSelectedOptionalFees(Enrollment enrollment)
    {
        var selectedOptionalFeesTotal = enrollment.SelectedOptionalFees.Sum(fi => fi.Amount.Amount);
        return new Money(selectedOptionalFeesTotal);
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

    private static Money CalculateTotalNetFees(Money totalMandatoryFees, Money totalSelectedOptionalFees, Money scholarshipDiscount)
    {
        var netFees = totalMandatoryFees.Amount + totalSelectedOptionalFees.Amount - scholarshipDiscount.Amount;
        return new Money(Math.Max(0, netFees));
    }

    private static List<FeeItemDetailDto> MapFeeItemsToDetailDtos(Enrollment enrollment)
    {
        var feeItems = new List<FeeItemDetailDto>();

        // Add mandatory fee items
        var mandatoryItems = enrollment.FeeStructure.FeeItems
            .Where(fi => !fi.IsOptional)
            .OrderBy(fi => fi.DisplayOrder);

        foreach (var item in mandatoryItems)
        {
            feeItems.Add(new FeeItemDetailDto(
                FeeItemId: item.FeeItemId,
                Name: item.FeeItem.Name,
                Category: item.FeeItem.Category,
                Description: item.FeeItem.Description,
                Amount: item.Amount,
                IsOptional: false,
                IsSelected: true, // Mandatory fees are always selected
                DisplayOrder: item.DisplayOrder
            ));
        }

        // Add optional fee items
        var optionalItems = enrollment.FeeStructure.FeeItems
            .Where(fi => fi.IsOptional)
            .OrderBy(fi => fi.DisplayOrder);

        foreach (var item in optionalItems)
        {
            var isSelected = enrollment.SelectedOptionalFees.Any(sof => sof.FeeItemId == item.FeeItemId);

            feeItems.Add(new FeeItemDetailDto(
                FeeItemId: item.FeeItemId,
                Name: item.FeeItem.Name,
                Category: item.FeeItem.Category,
                Description: item.FeeItem.Description,
                Amount: item.Amount,
                IsOptional: true,
                IsSelected: isSelected,
                DisplayOrder: item.DisplayOrder
            ));
        }

        return feeItems;
    }
}