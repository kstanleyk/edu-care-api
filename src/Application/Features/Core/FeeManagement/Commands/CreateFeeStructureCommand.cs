using EduCare.Application.Features.Core.FeeManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record CreateFeeStructureCommand(
    string Name,
    string Description,
    Guid ClassId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo) : IRequest<Result<FeeStructureDto>>;

public class CreateFeeStructureCommandHandler(
    IFeeStructureRepository feeStructureRepository,
    IClassRepository classRepository)
    : IRequestHandler<CreateFeeStructureCommand, Result<FeeStructureDto>>
{
    public async Task<Result<FeeStructureDto>> Handle(CreateFeeStructureCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new CreateFeeStructureCommandValidator();
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

            // Validate class exists
            var classEntity = await classRepository.GetByIdAsync(command.ClassId);
            if (classEntity is null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.NotFound(
                        "Class.NotFound",
                        $"Class with ID '{command.ClassId}' was not found"
                    )
                );
            }

            // Check for overlapping fee structures
            var overlappingFeeStructure = await feeStructureRepository.GetActiveByClassIdAsync(
                command.ClassId, command.EffectiveFrom);

            if (overlappingFeeStructure is not null)
            {
                return Result<FeeStructureDto>.Failed(
                    Error.Validation(
                        "FeeStructure.Overlapping",
                        $"An active fee structure already exists for this class during the specified period"
                    ),
                    "An active fee structure already exists for this class during the specified period"
                );
            }

            // Create parameters object
            var parameters = new CreateFeeStructureParameters(
                command.Name,
                command.Description,
                command.ClassId,
                command.EffectiveFrom,
                command.EffectiveTo);

            // Call repository with transaction support
            var repositoryResult = await feeStructureRepository.CreateFeeStructureAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Conflict => Result<FeeStructureDto>.Failed(
                        Error.Conflict(
                            "FeeStructure.Conflict",
                            "Fee structure creation failed due to conflict"
                        ),
                        "Fee structure creation failed due to conflict"
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
                            "A concurrency conflict occurred while creating the fee structure"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.NotCreated",
                            "Fee structure was not created"
                        ),
                        "No changes were made to create the fee structure"
                    ),
                    _ => Result<FeeStructureDto>.Failed(
                        Error.Failure(
                            "FeeStructure.CreationFailed",
                            $"An unexpected error occurred while creating the fee structure. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while creating the fee structure"
                    )
                };
            }

            // Manually map to DTO
            var feeStructureDto = MapToFeeStructureDto(repositoryResult.Entity!, classEntity);

            return Result<FeeStructureDto>.Succeeded(
                feeStructureDto,
                "Fee structure created successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error creating fee structure for class {ClassId}", command.ClassId);

            return Result<FeeStructureDto>.Failed(
                Error.Failure(
                    "FeeStructure.CreationFailed",
                    $"An error occurred while creating the fee structure: {ex.Message}"
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

public record CreateFeeStructureRequestDto(
    string Name,
    string Description,
    Guid ClassId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo);

public record CreateFeeStructureParameters(
    string Name,
    string Description,
    Guid ClassId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo);