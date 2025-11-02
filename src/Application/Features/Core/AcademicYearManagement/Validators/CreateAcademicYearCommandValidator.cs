using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Validators;

public class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Academic year name is required")
            .MaximumLength(100).WithMessage("Academic year name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Academic year code is required")
            .MaximumLength(20).WithMessage("Academic year code cannot exceed 20 characters")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Academic year code can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.SchoolId)
            .NotEmpty().WithMessage("School ID is required")
            .NotEqual(Guid.Empty).WithMessage("School ID cannot be empty");
    }
}