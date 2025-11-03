using EduCare.Application.Features.Core.FeeManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

// Command
public record UpdateFeeItemCommand(Guid Id, string Name, string Description, string Category, bool IsActive)
    : IRequest<Result<FeeItemDto>>;

public class UpdateFeeItemCommandHandler(
    IFeeItemRepository feeItemRepository)
    : IRequestHandler<UpdateFeeItemCommand, Result<FeeItemDto>>
{
    public async Task<Result<FeeItemDto>> Handle(UpdateFeeItemCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new UpdateFeeItemCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<FeeItemDto>.Failed(
                    Error.Validation("FeeItem.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate fee item exists
            var feeItem = await feeItemRepository.GetByIdAsync(command.Id);
            if (feeItem is null)
            {
                return Result<FeeItemDto>.Failed(
                    Error.NotFound(
                        "FeeItem.NotFound",
                        $"Fee item with ID '{command.Id}' was not found"
                    )
                );
            }

            // Check if there are any actual changes
            if (!HasChanges(feeItem, command))
            {
                return Result<FeeItemDto>.Failed(
                    Error.Validation(
                        "FeeItem.NoChanges",
                        "No changes were made to the fee item"
                    ),
                    "No changes were made to the fee item"
                );
            }

            // Create parameters object
            var parameters = new UpdateFeeItemParameters(
                command.Id,
                command.Name,
                command.Description,
                command.Category,
                command.IsActive);

            // Call repository with transaction support
            var repositoryResult = await feeItemRepository.UpdateFeeItemAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<FeeItemDto>.Failed(
                        Error.NotFound(
                            "FeeItem.NotFound",
                            $"Fee item with ID '{command.Id}' was not found"
                        ),
                        "Fee item not found"
                    ),
                    RepositoryActionStatus.Conflict => Result<FeeItemDto>.Failed(
                        Error.Conflict(
                            "FeeItem.Conflict",
                            "Fee item update failed due to conflict"
                        ),
                        "Fee item update failed due to conflict"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<FeeItemDto>.Failed(
                        Error.Failure(
                            "FeeItem.ConcurrencyConflict",
                            "A concurrency conflict occurred while updating the fee item"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<FeeItemDto>.Failed(
                        Error.Failure(
                            "FeeItem.NotUpdated",
                            "Fee item was not updated"
                        ),
                        "No changes were made to the fee item"
                    ),
                    _ => Result<FeeItemDto>.Failed(
                        Error.Failure(
                            "FeeItem.UpdateFailed",
                            $"An unexpected error occurred while updating the fee item. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the fee item"
                    )
                };
            }

            // Manually map to DTO
            var feeItemDto = MapToFeeItemDto(repositoryResult.Entity!);

            return Result<FeeItemDto>.Succeeded(
                feeItemDto,
                "Fee item updated successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error updating fee item {FeeItemId}", command.Id);

            return Result<FeeItemDto>.Failed(
                Error.Failure(
                    "FeeItem.UpdateFailed",
                    $"An error occurred while updating the fee item: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    /// <summary>
    /// Checks if there are any actual changes between the current fee item and the command
    /// </summary>
    private static bool HasChanges(FeeItem feeItem, UpdateFeeItemCommand command)
    {
        return feeItem.Name != command.Name ||
               feeItem.Description != command.Description ||
               feeItem.Category != command.Category ||
               feeItem.IsActive != command.IsActive;
    }

    private static FeeItemDto MapToFeeItemDto(FeeItem feeItem)
    {
        return new FeeItemDto(
            Id: feeItem.Id,
            Name: feeItem.Name,
            Description: feeItem.Description,
            Category: feeItem.Category,
            Code: feeItem.Code,
            IsActive: feeItem.IsActive,
            CreatedOn: feeItem.CreatedOn,
            ModifiedOn: feeItem.ModifiedOn
        );
    }
}

// Request DTO (if needed for API)
public record UpdateFeeItemRequestDto(Guid Id, string Name, string Description, string Category, bool IsActive);

// Parameters for repository
public record UpdateFeeItemParameters(Guid Id, string Name, string Description, string Category, bool IsActive);