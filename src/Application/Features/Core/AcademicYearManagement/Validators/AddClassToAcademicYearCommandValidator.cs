using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Validators;

public class AddClassToAcademicYearCommandValidator : AbstractValidator<AddClassToAcademicYearCommand>
{
    public AddClassToAcademicYearCommandValidator()
    {
        RuleFor(x => x.AcademicYearId)
            .NotEmpty().WithMessage("Academic year ID is required")
            .NotEqual(Guid.Empty).WithMessage("Academic year ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Class name is required")
            .MaximumLength(100).WithMessage("Class name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Class code is required")
            .MaximumLength(20).WithMessage("Class code cannot exceed 20 characters")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Class code can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.GradeLevel)
            .GreaterThan(0).WithMessage("Grade level must be greater than 0")
            .LessThanOrEqualTo(12).WithMessage("Grade level cannot exceed 12");
    }
}