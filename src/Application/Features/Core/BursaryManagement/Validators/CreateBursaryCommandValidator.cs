using EduCare.Application.Features.Core.BursaryManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.BursaryManagement.Validators;

public class CreateBursaryCommandValidator : AbstractValidator<CreateBursaryCommand>
{
    public CreateBursaryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Bursary name is required")
            .MaximumLength(100).WithMessage("Bursary name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Bursary code is required")
            .MaximumLength(20).WithMessage("Bursary code cannot exceed 20 characters")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Bursary code can only contain letters, numbers, hyphens, and underscores");

        // Address validation can be added if needed
        // RuleFor(x => x.Address).SetValidator(new AddressValidator());
    }
}