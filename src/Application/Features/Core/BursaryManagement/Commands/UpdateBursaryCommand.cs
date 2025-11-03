using EduCare.Application.Features.Core.BursaryManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.BursaryManagement.Commands;

public record UpdateBursaryCommand(Guid Id, string Name, Address? Address) : IRequest<Result<BursaryDto>>;

public class UpdateBursaryCommandHandler(
    IBursaryRepository bursaryRepository)
    : IRequestHandler<UpdateBursaryCommand, Result<BursaryDto>>
{
    public async Task<Result<BursaryDto>> Handle(UpdateBursaryCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new UpdateBursaryCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                return Result<BursaryDto>.Failed(
                    Error.Validation("Bursary.ValidationError", string.Join(", ", validationErrors))
                );
            }

            // Validate bursary exists
            var bursary = await bursaryRepository.GetByIdWithSchoolsAsync(command.Id);
            if (bursary is null)
            {
                return Result<BursaryDto>.Failed(
                    Error.NotFound(
                        "Bursary.NotFound",
                        $"Bursary with ID '{command.Id}' was not found"
                    )
                );
            }

            // Create parameters object
            var parameters = new UpdateBursaryParameters(command.Id, command.Name, command.Address);

            // Call repository with transaction support
            var repositoryResult = await bursaryRepository.UpdateBursaryAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<BursaryDto>.Failed(
                        Error.NotFound(
                            "Bursary.NotFound",
                            $"Bursary with ID '{command.Id}' was not found"
                        ),
                        "Bursary not found"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.ConcurrencyConflict",
                            "A concurrency conflict occurred while updating the bursary"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.NotUpdated",
                            "Bursary was not updated"
                        ),
                        "No changes were made to the bursary"
                    ),
                    _ => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.UpdateFailed",
                            $"An unexpected error occurred while updating the bursary. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while updating the bursary"
                    )
                };
            }

            // Manually map to DTO
            var bursaryDto = MapToBursaryDto(repositoryResult.Entity!);

            return Result<BursaryDto>.Succeeded(
                bursaryDto,
                "Bursary updated successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error updating bursary {BursaryId}", command.Id);

            return Result<BursaryDto>.Failed(
                Error.Failure(
                    "Bursary.UpdateFailed",
                    $"An error occurred while updating the bursary: {ex.Message}"
                ),
                "An unexpected error occurred while processing your request"
            );
        }
    }

    private static BursaryDto MapToBursaryDto(Bursary bursary)
    {
        return new BursaryDto(
            Id: bursary.Id,
            Name: bursary.Name,
            Code: bursary.Code,
            Address: bursary.Address,
            CreatedOn: bursary.CreatedOn,
            ModifiedOn: bursary.ModifiedOn,
            Schools: bursary.Schools.Select(MapToSchoolSummaryDto).ToList(),
            Payments: bursary.Payments.Select(MapToPaymentSummaryDto).ToList()
        );
    }

    private static SchoolSummaryDto MapToSchoolSummaryDto(School school)
    {
        return new SchoolSummaryDto(
            Id: school.Id,
            Name: school.Name,
            Code: school.Code,
            Type: school.Type,
            Mode: school.Mode
        );
    }

    private static PaymentSummaryDto MapToPaymentSummaryDto(Payment payment)
    {
        return new PaymentSummaryDto(
            Id: payment.Id,
            Amount: payment.Amount,
            PaymentDate: payment.PaymentDate,
            PaymentMethod: payment.PaymentMethod,
            ReferenceNumber: payment.ReferenceNumber
        );
    }
}

public record UpdateBursaryRequestDto(string Name, Address? Address);

public record UpdateBursaryParameters(Guid Id, string Name, Address? Address);