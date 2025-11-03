using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record GetClassFeeStructuresQuery(Guid ClassId) : IRequest<Result<List<FeeStructureDto>>>;

public class GetClassFeeStructuresQueryHandler(
    IFeeStructureRepository feeStructureRepository,
    IClassRepository classRepository)
    : IRequestHandler<GetClassFeeStructuresQuery, Result<List<FeeStructureDto>>>
{
    public async Task<Result<List<FeeStructureDto>>> Handle(GetClassFeeStructuresQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate class exists
            var classEntity = await classRepository.GetByIdAsync(query.ClassId);
            if (classEntity is null)
            {
                return Result<List<FeeStructureDto>>.Failed(
                    Error.NotFound(
                        "Class.NotFound",
                        $"Class with ID {query.ClassId} was not found"
                    )
                );
            }

            // Get all fee structures for the class
            var feeStructures = await feeStructureRepository.GetByClassIdAsync(query.ClassId);

            // Manually map FeeStructure entities to FeeStructureDto list without AutoMapper
            var feeStructureDtos = feeStructures.Select(MapToFeeStructureDto).ToList();

            return Result<List<FeeStructureDto>>.Succeeded(feeStructureDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting fee structures for class {ClassId}", query.ClassId);

            return Result<List<FeeStructureDto>>.Failed(
                Error.Failure(
                    "FeeStructuresQuery.Failed",
                    $"An error occurred while retrieving the fee structures: {ex.Message}"
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
            ClassName: feeStructure.Class.Name,
            ClassCode: feeStructure.Class.Code,
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
            FeeItemName: feeStructureItem.FeeItem.Name,
            FeeItemDescription: feeStructureItem.FeeItem.Description,
            FeeItemCategory: feeStructureItem.FeeItem.Category,
            FeeItemCode: feeStructureItem.FeeItem.Code,
            Amount: feeStructureItem.Amount,
            IsOptional: feeStructureItem.IsOptional,
            DisplayOrder: feeStructureItem.DisplayOrder
        );
    }
}