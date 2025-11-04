using EduCare.Application.Features.Core.OrganizationManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.OrganizationManagement.Validators;

public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Organization ID is required")
            .NotEqual(Guid.Empty).WithMessage("Organization ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required")
            .MaximumLength(100).WithMessage("Organization name cannot exceed 100 characters");
    }
}