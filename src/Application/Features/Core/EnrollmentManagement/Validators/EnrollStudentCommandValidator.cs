using EduCare.Application.Features.Core.EnrollmentManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Validators;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required")
            .NotEqual(Guid.Empty).WithMessage("Student ID cannot be empty");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Class ID is required")
            .NotEqual(Guid.Empty).WithMessage("Class ID cannot be empty");

        RuleFor(x => x.AcademicYearId)
            .NotEmpty().WithMessage("Academic year ID is required")
            .NotEqual(Guid.Empty).WithMessage("Academic year ID cannot be empty");

        RuleFor(x => x.FeeStructureId)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee structure ID cannot be empty");

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty().WithMessage("Enrollment date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Enrollment date cannot be in the future");
    }
}