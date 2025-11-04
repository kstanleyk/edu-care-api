using EduCare.Application.Features.Core.FeeManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record AddFeeItemToStructureCommand(
    Guid FeeStructureId,
    Guid FeeItemId,
    Money Amount,
    bool IsOptional = false,
    int DisplayOrder = 0) : IRequest<Result<FeeStructureDto>>;

public class AddFeeItemToStructureCommandHandler(
    IFeeStructureRepository feeStructureRepository,
    IFeeItemRepository feeItemRepository,
    IClassRepository classRepository)
    : IRequestHandler<AddFeeItemToStructureCommand, Result<FeeStructureDto>>
{
    public async Task<Result<FeeStructureDto>> Handle(AddFeeItemToStructureCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new AddFeeItemToStructureCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<FeeStructureDto>.Failed(
                    Error.Validation("FeeStructure.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate fee structure exists
            var feeStructure = await feeStructureRepository.GetByIdWithFeeItemsAsync(command.FeeStructureId);
            if (feeStructure is null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.NotFound(
                        "FeeStructure.NotFound",
                        $"Fee structure with ID '{command.FeeStructureId}' was not found"
                    )
                );
            }

            // Validate fee item exists
            var feeItem = await feeItemRepository.GetByIdAsync(command.FeeItemId);
            if (feeItem is null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.NotFound(
                        "FeeItem.NotFound",
                        $"Fee item with ID '{command.FeeItemId}' was not found"
                    )
                );
            }

            // Check if fee item is already added to this fee structure
            var existingFeeItem = feeStructure.FeeItems.FirstOrDefault(fi => fi.FeeItemId == command.FeeItemId);
            if (existingFeeItem is not null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.Validation(
                        "FeeStructure.DuplicateFeeItem",
                        $"Fee item '{feeItem.Name}' is already added to this fee structure"
                    ),
                    "Fee item is already added to this fee structure"
                );
            }

            // Check if fee item is active
            if (!feeItem.IsActive)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.Validation(
                        "FeeItem.Inactive",
                        $"Cannot add inactive fee item '{feeItem.Name}' to fee structure"
                    ),
                    "Cannot add inactive fee item to fee structure"
                );
            }

            // Create parameters object
            var parameters = new AddFeeItemToStructureParameters(
                command.FeeStructureId,
                command.FeeItemId,
                command.Amount,
                command.IsOptional,
                command.DisplayOrder);

            // Call repository with transaction support
            var repositoryResult = await feeStructureRepository.AddFeeItemToStructureAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<FeeStructureDto>.Failed(
                        Error.NotFound(
                            "FeeStructure.NotFound",
                            $"Fee structure or fee item was not found"
                        ),
                        "Fee structure or fee item not found"
                    ),
                    RepositoryActionStatus.Invalid => Result<FeeStructureDto>.Failed(
                        Error.Validation(
                            "FeeStructure.DuplicateFeeItem",
                            $"Fee item is already added to this fee structure"
                        ),
                        "Fee item is already added to this fee structure"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.ConcurrencyConflict",
                            "A concurrency conflict occurred while adding the fee item to the fee structure"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.NotUpdated",
                            "Fee structure was not updated"
                        ),
                        "No changes were made to the fee structure"
                    ),
                    _ => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.UpdateFailed",
                            $"An unexpected error occurred while adding the fee item to the fee structure. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the fee structure"
                    )
                };
            }

            // Get related entities for DTO mapping
            var classEntity = await classRepository.GetByIdAsync(feeStructure.ClassId);

            // Manually map to DTO
            var feeStructureDto = MapToFeeStructureDto(repositoryResult.Entity!, classEntity!);

            return Result<FeeStructureDto>.Succeeded(
                feeStructureDto,
                "Fee item added to fee structure successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error adding fee item {FeeItemId} to fee structure {FeeStructureId}", command.FeeItemId, command.FeeStructureId);

            return Result<FeeStructureDto>.Failed(
                Error.Failure(
                    "FeeStructure.UpdateFailed",
                    $"An error occurred while adding the fee item to the fee structure: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    private static FeeStructureDto MapToFeeStructureDto(FeeStructure feeStructure, Class classEntity)
    {
        return new FeeStructureDto(
            Id: feeStructure.Id,
            Name: feeStructure.Name,
            Description: feeStructure.Description,
            ClassId: feeStructure.ClassId,
            ClassName: classEntity.Name,
            ClassCode: classEntity.Code,
            IsActive: feeStructure.IsActive,
            EffectiveFrom: feeStructure.EffectiveFrom,
            EffectiveTo: feeStructure.EffectiveTo,
            CreatedOn: feeStructure.CreatedOn,
            ModifiedOn: feeStructure.ModifiedOn,
            FeeItems: feeStructure.FeeItems.Select(MapToFeeStructureItemDto).ToList()
        );
    }

    private static FeeStructureItemDto MapToFeeStructureItemDto(FeeStructureItem feeStructureItem)
    {
        return new FeeStructureItemDto(
            Id: feeStructureItem.Id,
            FeeItemId: feeStructureItem.FeeItemId,
            FeeItemName: feeStructureItem.FeeItem.Name,
            FeeItemCode: feeStructureItem.FeeItem.Code,
            FeeItemCategory: feeStructureItem.FeeItem.Category,
            Amount: feeStructureItem.Amount,
            IsOptional: feeStructureItem.IsOptional,
            DisplayOrder: feeStructureItem.DisplayOrder,
            CreatedOn: feeStructureItem.CreatedOn,
            ModifiedOn: feeStructureItem.ModifiedOn
        );
    }
}

public record AddFeeItemToStructureParameters(
    Guid FeeStructureId,
    Guid FeeItemId,
    Money Amount,
    bool IsOptional,
    int DisplayOrder);

public record AddFeeItemToStructureRequestDto(
    Guid FeeStructureId,
    Guid FeeItemId,
    Money Amount,
    bool IsOptional = false,
    int DisplayOrder = 0);

public record FeeStructureDto(
    Guid Id,
    string Name,
    string Description,
    Guid ClassId,
    string ClassName,
    string ClassCode,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    IReadOnlyCollection<FeeStructureItemDto> FeeItems);

public record FeeStructureItemDto(
    Guid Id,
    Guid FeeItemId,
    string FeeItemName,
    string FeeItemCode,
    string FeeItemCategory,
    Money Amount,
    bool IsOptional,
    int DisplayOrder,
    DateTime CreatedOn,
    DateTime? ModifiedOn);

public record FeeItemDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    string Code,
    bool IsActive,
    DateTime CreatedOn,
    DateTime? ModifiedOn);