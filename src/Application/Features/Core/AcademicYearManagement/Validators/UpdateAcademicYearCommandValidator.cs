using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Validators;

public class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Academic year ID is required")
            .NotEqual(Guid.Empty).WithMessage("Academic year ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Academic year name is required")
            .MaximumLength(100).WithMessage("Academic year name cannot exceed 100 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}