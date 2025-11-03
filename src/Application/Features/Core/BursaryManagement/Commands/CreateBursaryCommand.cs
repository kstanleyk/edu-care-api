using EduCare.Application.Features.Core.BursaryManagement.Validators;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.BursaryManagement.Commands;

public record CreateBursaryCommand(string Name, string Code, Address? Address) : IRequest<Result<BursaryDto>>;

public class CreateBursaryCommandHandler(
    IBursaryRepository bursaryRepository)
    : IRequestHandler<CreateBursaryCommand, Result<BursaryDto>>
{
    public async Task<Result<BursaryDto>> Handle(CreateBursaryCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate command using FluentValidation
            var validator = new CreateBursaryCommandValidator();
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

            // Check for duplicate bursary code
            var existingBursary = await bursaryRepository.GetByCodeAsync(command.Code);
            if (existingBursary is not null)
            {
                return Result<BursaryDto>.Failed(
                    Error.Validation(
                        "Bursary.DuplicateCode",
                        $"Bursary with code '{command.Code}' already exists"
                    )
                );
            }

            // Create parameters object
            var parameters = new CreateBursaryParameters(command.Name, command.Code, command.Address);

            // Call repository with transaction support
            var repositoryResult = await bursaryRepository.CreateBursaryAsync(parameters);

            if (repositoryResult.Status != RepositoryActionStatus.Created)
            {
                return repositoryResult.Status switch
                {
                    RepositoryActionStatus.Invalid => Result<BursaryDto>.Failed(
                        Error.Validation(
                            "Bursary.DuplicateCode",
                            $"Bursary with code '{command.Code}' already exists"
                        ),
                        "Bursary creation failed due to duplicate code"
                    ),
                    RepositoryActionStatus.ConcurrencyConflict => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.ConcurrencyConflict",
                            "A concurrency conflict occurred while creating the bursary"
                        ),
                        "Please try again as another operation conflicted with this request"
                    ),
                    RepositoryActionStatus.NothingModified => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.NotCreated",
                            "Bursary was not created"
                        ),
                        "No changes were made to create the bursary"
                    ),
                    _ => Result<BursaryDto>.Failed(
                        Error.Failure(
                            "Bursary.CreationFailed",
                            $"An unexpected error occurred while creating the bursary. Status: {repositoryResult.Status}"
                        ),
                        "An unexpected error occurred while creating the bursary"
                    )
                };
            }

            // Manually map to DTO
            var bursaryDto = MapToBursaryDto(repositoryResult.Entity!);

            return Result<BursaryDto>.Succeeded(
                bursaryDto,
                "Bursary created successfully"
            );
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error creating bursary with code {BursaryCode}", command.Code);

            return Result<BursaryDto>.Failed(
                Error.Failure(
                    "Bursary.CreationFailed",
                    $"An error occurred while creating the bursary: {ex.Message}"
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

public record CreateBursaryParameters(string Name, string Code, Address? Address);
