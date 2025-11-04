using EduCare.Application.Features.Core.FeeManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

// Command
public record UpdateFeeStructureCommand(
    Guid Id,
    string Name,
    string Description,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive) : IRequest<Result<FeeStructureDto>>;

public class UpdateFeeStructureCommandHandler(
    IFeeStructureRepository feeStructureRepository,
    IClassRepository classRepository)
    : IRequestHandler<UpdateFeeStructureCommand, Result<FeeStructureDto>>
{
    public async Task<Result<FeeStructureDto>> Handle(UpdateFeeStructureCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new UpdateFeeStructureCommandValidator();
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
            var feeStructure = await feeStructureRepository.GetByIdWithFeeItemsAsync(command.Id);
            if (feeStructure is null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.NotFound(
                        "FeeStructure.NotFound",
                        $"Fee structure with ID '{command.Id}' was not found"
                    )
                );
            }

            // Check for overlapping fee structures (excluding current one)
            var hasOverlap = await feeStructureRepository.HasOverlappingFeeStructureAsync(
                feeStructure.ClassId,
                command.EffectiveFrom,
                command.EffectiveTo,
                command.Id); // Exclude current fee structure

            if (hasOverlap)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.Validation(
                        "FeeStructure.Overlapping",
                        "An active fee structure already exists for this class during the specified period"
                    ),
                    "An active fee structure already exists for this class during the specified period"
                );
            }

            // Check if there are any actual changes
            if (!HasChanges(feeStructure, command))
            {
                return Result<FeeStructureDto>.Failed(
                    Error.Validation(
                        "FeeStructure.NoChanges",
                        "No changes were made to the fee structure"
                    ),
                    "No changes were made to the fee structure"
                );
            }

            // Create parameters object
            var parameters = new UpdateFeeStructureParameters(
                command.Id,
                command.Name,
                command.Description,
                command.EffectiveFrom,
                command.EffectiveTo,
                command.IsActive);

            // Call repository with transaction support
            var repositoryResult = await feeStructureRepository.UpdateFeeStructureAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<FeeStructureDto>.Failed(
                        Error.NotFound(
                            "FeeStructure.NotFound",
                            $"Fee structure with ID '{command.Id}' was not found"
                        ),
                        "Fee structure not found"
                    ),
                    RepositoryActionStatus.Invalid => Result<FeeStructureDto>.Failed(
                        Error.Validation(
                            "FeeStructure.Overlapping",
                            "An overlapping fee structure exists for this class"
                        ),
                        "An overlapping fee structure exists for this class"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.ConcurrencyConflict",
                            "A concurrency conflict occurred while updating the fee structure"
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
                            $"An unexpected error occurred while updating the fee structure. Status: {repositoryResult.Status}"
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
                "Fee structure updated successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error updating fee structure {FeeStructureId}", command.Id);

            return Result<FeeStructureDto>.Failed(
                Error.Failure(
                    "FeeStructure.UpdateFailed",
                    $"An error occurred while updating the fee structure: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    /// <summary>
    /// Checks if there are any actual changes between the current fee structure and the command
    /// </summary>
    private static bool HasChanges(FeeStructure feeStructure, UpdateFeeStructureCommand command)
    {
        return feeStructure.Name != command.Name ||
               feeStructure.Description != command.Description ||
               feeStructure.EffectiveFrom != command.EffectiveFrom ||
               feeStructure.EffectiveTo != command.EffectiveTo ||
               feeStructure.IsActive != command.IsActive;
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
public record UpdateFeeStructureRequestDto(
    Guid Id,
    string Name,
    string Description,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive);

// Parameters for repository
public record UpdateFeeStructureParameters(
    Guid Id,
    string Name,
    string Description,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive);