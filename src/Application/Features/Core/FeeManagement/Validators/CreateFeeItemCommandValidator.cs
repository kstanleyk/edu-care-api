using EduCare.Application.Features.Core.FeeManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.FeeManagement.Validators;

public class CreateFeeItemCommandValidator : AbstractValidator<CreateFeeItemCommand>
{
    public CreateFeeItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee item name is required")
            .MaximumLength(100).WithMessage("Fee item name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Fee item description is required")
            .MaximumLength(500).WithMessage("Fee item description cannot exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Fee item category is required")
            .MaximumLength(50).WithMessage("Fee item category cannot exceed 50 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Fee item code is required")
            .MaximumLength(20).WithMessage("Fee item code cannot exceed 20 characters")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Fee item code can only contain uppercase letters, numbers, hyphens, and underscores");
    }
}