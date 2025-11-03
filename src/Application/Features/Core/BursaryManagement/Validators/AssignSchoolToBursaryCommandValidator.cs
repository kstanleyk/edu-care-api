using EduCare.Application.Features.Core.BursaryManagement.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Application.Features.Core.BursaryManagement.Validators;

public class AssignSchoolToBursaryCommandValidator : AbstractValidator<AssignSchoolToBursaryCommand>
{
    public AssignSchoolToBursaryCommandValidator()
    {
        RuleFor(x => x.BursaryId)
            .NotEmpty().WithMessage("Bursary ID is required")
            .NotEqual(Guid.Empty).WithMessage("Bursary ID cannot be empty");

        RuleFor(x => x.SchoolId)
            .NotEmpty().WithMessage("School ID is required")
            .NotEqual(Guid.Empty).WithMessage("School ID cannot be empty");
    }
}