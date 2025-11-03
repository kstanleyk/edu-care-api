using EduCare.Application.Features.Core.FeeManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

// Command
public record UpdateFeeItemAmountCommand(Guid FeeStructureId, Guid FeeItemId, Money NewAmount)
    : IRequest<Result<FeeStructureDto>>;


public class UpdateFeeItemAmountCommandHandler(
    IFeeStructureRepository feeStructureRepository,
    IClassRepository classRepository)
    : IRequestHandler<UpdateFeeItemAmountCommand, Result<FeeStructureDto>>
{
    public async Task<Result<FeeStructureDto>> Handle(UpdateFeeItemAmountCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new UpdateFeeItemAmountCommandValidator();
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

            // Check if fee item exists in the fee structure
            var feeStructureItem = feeStructure.FeeItems.FirstOrDefault(fi => fi.FeeItemId == command.FeeItemId);
            if (feeStructureItem is null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.NotFound(
                        "FeeStructureItem.NotFound",
                        $"Fee item with ID '{command.FeeItemId}' was not found in the fee structure"
                    )
                );
            }

            // Check if the amount is actually changing
            if (feeStructureItem.Amount.Equals(command.NewAmount))
            {
                return Result<FeeStructureDto>.Failed(
                    Error.Validation(
                        "FeeStructure.AmountUnchanged",
                        "New amount is the same as the current amount"
                    ),
                    "Amount is already set to the specified value"
                );
            }

            // Create parameters object
            var parameters = new UpdateFeeItemAmountParameters(
                command.FeeStructureId,
                command.FeeItemId,
                command.NewAmount);

            // Call repository with transaction support
            var repositoryResult = await feeStructureRepository.UpdateFeeItemAmountAsync(parameters);

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
                            "FeeStructureItem.NotFound",
                            $"Fee item is not part of this fee structure"
                        ),
                        "Fee item is not part of this fee structure"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.ConcurrencyConflict",
                            "A concurrency conflict occurred while updating the fee item amount"
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
                            $"An unexpected error occurred while updating the fee item amount. Status: {repositoryResult.Status}"
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
                "Fee item amount updated successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error updating fee item amount for fee item {FeeItemId} in fee structure {FeeStructureId}", command.FeeItemId, command.FeeStructureId);

            return Result<FeeStructureDto>.Failed(
                Error.Failure(
                    "FeeStructure.UpdateFailed",
                    $"An error occurred while updating the fee item amount: {ex.Message}"
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

// Request DTO (if needed for API)
public record UpdateFeeItemAmountRequestDto(Guid FeeStructureId, Guid FeeItemId, Money NewAmount);

// Parameters for repository
public record UpdateFeeItemAmountParameters(Guid FeeStructureId, Guid FeeItemId, Money NewAmount);