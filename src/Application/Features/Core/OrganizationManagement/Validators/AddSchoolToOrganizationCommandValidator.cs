using EduCare.Application.Features.Core.OrganizationManagement.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Application.Features.Core.OrganizationManagement.Validators;

public class AddSchoolToOrganizationCommandValidator : AbstractValidator<AddSchoolToOrganizationCommand>
{
    public AddSchoolToOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required")
            .NotEqual(Guid.Empty).WithMessage("Organization ID cannot be empty");

        RuleFor(x => x.SchoolId)
            .NotEmpty().WithMessage("School ID is required")
            .NotEqual(Guid.Empty).WithMessage("School ID cannot be empty");
    }
}