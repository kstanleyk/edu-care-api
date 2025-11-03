using EduCare.Application.Features.Core.EnrollmentManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Validators;

public class PromoteStudentCommandValidator : AbstractValidator<PromoteStudentCommand>
{
    public PromoteStudentCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Current enrollment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Current enrollment ID cannot be empty");

        RuleFor(x => x.NextClassId)
            .NotEmpty().WithMessage("Next class ID is required")
            .NotEqual(Guid.Empty).WithMessage("Next class ID cannot be empty");

        RuleFor(x => x.NextAcademicYearId)
            .NotEmpty().WithMessage("Next academic year ID is required")
            .NotEqual(Guid.Empty).WithMessage("Next academic year ID cannot be empty");

        RuleFor(x => x.NewFeeStructureId)
            .NotEmpty().WithMessage("New fee structure ID is required")
            .NotEqual(Guid.Empty).WithMessage("New fee structure ID cannot be empty");
    }
}