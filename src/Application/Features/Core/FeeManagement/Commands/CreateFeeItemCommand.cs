using EduCare.Application.Features.Core.FeeManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record CreateFeeItemCommand(string Name, string Description, string Category, string Code)
    : IRequest<Result<FeeItemDto>>;

public class CreateFeeItemCommandHandler(
    IFeeItemRepository feeItemRepository)
    : IRequestHandler<CreateFeeItemCommand, Result<FeeItemDto>>
{
    public async Task<Result<FeeItemDto>> Handle(CreateFeeItemCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new CreateFeeItemCommandValidator();
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

            // Check if fee item with same code already exists
            var existingFeeItem = await feeItemRepository.GetByCodeAsync(command.Code);
            if (existingFeeItem is not null)
            {
                return Result<FeeItemDto>.Failed(
                    Error.Validation(
                        "FeeItem.DuplicateCode",
                        $"Fee item with code '{command.Code}' already exists"
                    ),
                    "A fee item with this code already exists"
                );
            }

            // Create parameters object
            var parameters = new CreateFeeItemParameters(
                command.Name,
                command.Description,
                command.Category,
                command.Code);

            // Call repository with transaction support
            var repositoryResult = await feeItemRepository.CreateFeeItemAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Conflict => Result<FeeItemDto>.Failed(
                        Error.Conflict(
                            "FeeItem.DuplicateCode",
                            $"Fee item with code '{command.Code}' already exists"
                        ),
                        "A fee item with this code already exists"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<FeeItemDto>.Failed(
                        Error.Failure(
                            "FeeItem.ConcurrencyConflict",
                            "A concurrency conflict occurred while creating the fee item"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<FeeItemDto>.Failed(
                        Error.Failure(
                            "FeeItem.NotCreated",
                            "Fee item was not created"
                        ),
                        "No changes were made to create the fee item"
                    ),
                    _ => Result<FeeItemDto>.Failed(
                        Error.Failure(
                            "FeeItem.CreationFailed",
                            $"An unexpected error occurred while creating the fee item. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while creating the fee item"
                    )
                };
            }

            // Manually map to DTO
            var feeItemDto = MapToFeeItemDto(repositoryResult.Entity!);

            return Result<FeeItemDto>.Succeeded(
                feeItemDto,
                "Fee item created successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error creating fee item with code {Code}", command.Code);

            return Result<FeeItemDto>.Failed(
                Error.Failure(
                    "FeeItem.CreationFailed",
                    $"An error occurred while creating the fee item: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
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

public record CreateFeeItemRequestDto(string Name, string Description, string Category, string Code);

public record CreateFeeItemParameters(string Name, string Description, string Category, string Code);
