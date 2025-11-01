using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record GetActiveFeeStructureQuery(Guid ClassId, DateTime? asOf = null) : IRequest<Result<FeeStructureDto?>>;

public class GetActiveFeeStructureQueryHandler(
    IFeeStructureRepository feeStructureRepository,
    IClassRepository classRepository)
    : IRequestHandler<GetActiveFeeStructureQuery, Result<FeeStructureDto?>>
{
    public async Task<Result<FeeStructureDto?>> Handle(GetActiveFeeStructureQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate class exists
            var classEntity = await classRepository.GetAsync(query.ClassId);
            if (classEntity is null)
            {
                return Result<FeeStructureDto?>.Failed(
                    Error.NotFound(
                        "Class.NotFound",
                        $"Class with ID {query.ClassId} was not found"
                    )
                );
            }

            // Determine the effective date
            var effectiveDate = query.asOf ?? DateTime.UtcNow;

            // Get active fee structure for the class
            var feeStructure = await feeStructureRepository.GetActiveByClassIdAsync(query.ClassId, effectiveDate);

            if (feeStructure is null)
            {
                // Return success with null value - no active fee structure found
                return Result<FeeStructureDto?>.Succeeded(null);
            }

            // Manually map FeeStructure to FeeStructureDto without AutoMapper
            var feeStructureDto = MapToFeeStructureDto(feeStructure);

            return Result<FeeStructureDto?>.Succeeded(feeStructureDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting active fee structure for class {ClassId}", query.ClassId);

            return Result<FeeStructureDto?>.Failed(
                Error.Failure(
                    "FeeStructureQuery.Failed",
                    $"An error occurred while retrieving the fee structure: {ex.Message}"
                )
            );
        }
    }

    private static FeeStructureDto MapToFeeStructureDto(FeeStructure feeStructure)
    {
        return new FeeStructureDto(
            Id: feeStructure.Id,
            Name: feeStructure.Name,
            Description: feeStructure.Description,
            ClassId: feeStructure.ClassId,
            ClassName: feeStructure.Class?.Name ?? string.Empty,
            ClassCode: feeStructure.Class?.Code ?? string.Empty,
            IsActive: feeStructure.IsActive,
            EffectiveFrom: feeStructure.EffectiveFrom,
            EffectiveTo: feeStructure.EffectiveTo,
            CreatedOn: feeStructure.CreatedOn,
            ModifiedOn: feeStructure.ModifiedOn,
            TotalMandatoryFees: feeStructure.CalculateTotalFees(),
            TotalWithOptionalFees: feeStructure.CalculateTotalWithOptionalFees(),
            FeeItems: feeStructure.FeeItems.Select(MapToFeeStructureItemDto).ToList()
        );
    }

    private static FeeStructureItemDto MapToFeeStructureItemDto(FeeStructureItem feeStructureItem)
    {
        return new FeeStructureItemDto(
            Id: feeStructureItem.Id,
            FeeItemId: feeStructureItem.FeeItemId,
            FeeItemName: feeStructureItem.FeeItem?.Name ?? string.Empty,
            FeeItemDescription: feeStructureItem.FeeItem?.Description ?? string.Empty,
            FeeItemCategory: feeStructureItem.FeeItem?.Category ?? string.Empty,
            FeeItemCode: feeStructureItem.FeeItem?.Code ?? string.Empty,
            Amount: feeStructureItem.Amount,
            IsOptional: feeStructureItem.IsOptional,
            DisplayOrder: feeStructureItem.DisplayOrder
        );
    }
}