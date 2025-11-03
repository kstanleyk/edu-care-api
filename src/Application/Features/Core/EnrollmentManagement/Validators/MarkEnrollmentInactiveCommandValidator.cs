using EduCare.Application.Features.Core.EnrollmentManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Validators;

public class MarkEnrollmentInactiveCommandValidator : AbstractValidator<MarkEnrollmentInactiveCommand>
{
    public MarkEnrollmentInactiveCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Enrollment ID cannot be empty");
    }
}