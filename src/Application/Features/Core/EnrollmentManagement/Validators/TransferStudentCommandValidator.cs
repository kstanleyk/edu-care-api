using EduCare.Application.Features.Core.EnrollmentManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Validators;

public class TransferStudentCommandValidator : AbstractValidator<TransferStudentCommand>
{
    public TransferStudentCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Current enrollment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Current enrollment ID cannot be empty");

        RuleFor(x => x.NewClassId)
            .NotEmpty().WithMessage("New class ID is required")
            .NotEqual(Guid.Empty).WithMessage("New class ID cannot be empty");

        RuleFor(x => x.NewFeeStructureId)
            .NotEmpty().WithMessage("New fee structure ID is required")
            .NotEqual(Guid.Empty).WithMessage("New fee structure ID cannot be empty");
    }
}