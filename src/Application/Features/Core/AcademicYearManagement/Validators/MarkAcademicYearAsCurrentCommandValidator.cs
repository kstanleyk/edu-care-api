using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Validators;

public class MarkAcademicYearAsCurrentCommandValidator : AbstractValidator<MarkAcademicYearAsCurrentCommand>
{
    public MarkAcademicYearAsCurrentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Academic year ID is required")
            .NotEqual(Guid.Empty).WithMessage("Academic year ID cannot be empty");
    }
}