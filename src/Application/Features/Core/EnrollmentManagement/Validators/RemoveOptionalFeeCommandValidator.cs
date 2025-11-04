using EduCare.Application.Features.Core.EnrollmentManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Validators;

public class RemoveOptionalFeeCommandValidator : AbstractValidator<RemoveOptionalFeeCommand>
{
    public RemoveOptionalFeeCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Enrollment ID cannot be empty");

        RuleFor(x => x.FeeItemId)
            .NotEmpty().WithMessage("Fee item ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee item ID cannot be empty");
    }
}