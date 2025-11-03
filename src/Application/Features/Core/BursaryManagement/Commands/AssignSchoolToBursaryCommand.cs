using EduCare.Application.Features.Core.BursaryManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.BursaryManagement.Commands;

public record AssignSchoolToBursaryCommand(Guid BursaryId, Guid SchoolId) : IRequest<Result<BursaryDto>>;

public class AssignSchoolToBursaryCommandHandler(
    IBursaryRepository bursaryRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<AssignSchoolToBursaryCommand, Result<BursaryDto>>
{
    public async Task<Result<BursaryDto>> Handle(AssignSchoolToBursaryCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new AssignSchoolToBursaryCommandValidator();
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
            var bursary = await bursaryRepository.GetByIdWithSchoolsAsync(command.BursaryId);
            if (bursary is null)
            {
                return Result<BursaryDto>.Failed(
                    Error.NotFound(
                        "Bursary.NotFound",
                        $"Bursary with ID '{command.BursaryId}' was not found"
                    )
                );
            }

            // Validate school exists
            var school = await schoolRepository.GetByIdAsync(command.SchoolId);
            if (school is null)
            {
                return Result<BursaryDto>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID '{command.SchoolId}' was not found"
                    )
                );
            }

            // Check if school is already assigned to this bursary
            if (bursary.Schools.Any(s => s.Id == command.SchoolId))
            {
                return Result<BursaryDto>.Failed(
                    Error.Validation(
                        "Bursary.SchoolAlreadyAssigned",
                        $"School with ID '{command.SchoolId}' is already assigned to this bursary"
                    ),
                    "School is already assigned to this bursary"
                );
            }

            // Create parameters object
            var parameters = new AssignSchoolToBursaryParameters(command.BursaryId, command.SchoolId);

            // Call repository with transaction support
            var repositoryResult = await bursaryRepository.AssignSchoolAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Updated)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.NotFound => Result<BursaryDto>.Failed(
                        Error.NotFound(
                            "Bursary.NotFound",
                            $"Bursary with ID '{command.BursaryId}' was not found"
                        ),
                        "Bursary not found"
                    ),
                    RepositoryActionStatus.Invalid => Result<BursaryDto>.Failed(
                        Error.Validation(
                            "Bursary.SchoolAlreadyAssigned",
                            $"School is already assigned to this bursary"
                        ),
                        "School is already assigned to this bursary"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.ConcurrencyConflict",
                            "A concurrency conflict occurred while assigning the school to bursary"
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
                            "Bursary.AssignmentFailed",
                            $"An unexpected error occurred while assigning the school to bursary. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while assigning the school to bursary"
                    )
                };
            }

            // Manually map to DTO
            var bursaryDto = MapToBursaryDto(repositoryResult.Entity!);

            return Result<BursaryDto>.Succeeded(
                bursaryDto,
                "School successfully assigned to bursary"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error assigning school {SchoolId} to bursary {BursaryId}", command.SchoolId, command.BursaryId);

            return Result<BursaryDto>.Failed(
                Error.Failure(
                    "Bursary.AssignmentFailed",
                    $"An error occurred while assigning the school to bursary: {ex.Message}"
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

public record AssignSchoolToBursaryRequestDto(Guid BursaryId, Guid SchoolId);

public record BursaryDto(
    Guid Id,
    string Name,
    string Code,
    Address? Address,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    IReadOnlyCollection<SchoolSummaryDto> Schools,
    IReadOnlyCollection<PaymentSummaryDto> Payments);

public record SchoolSummaryDto(
    Guid Id,
    string Name,
    string Code,
    SchoolType Type,
    SchoolMode Mode);

public record PaymentSummaryDto(
    Guid Id,
    Money Amount,
    DateTime PaymentDate,
    string PaymentMethod,
    string ReferenceNumber);

public record AssignSchoolToBursaryParameters(Guid BursaryId, Guid SchoolId);