using EduCare.Application.Features.Core.OrganizationManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.OrganizationManagement.Validators;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required")
            .MaximumLength(100).WithMessage("Organization name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Organization code is required")
            .MaximumLength(20).WithMessage("Organization code cannot exceed 20 characters")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Organization code can only contain uppercase letters, numbers, hyphens, and underscores");
    }
}