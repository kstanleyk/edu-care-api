using EduCare.Application.Features.Core.FeeManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.FeeManagement.Validators;

public class UpdateFeeItemCommandValidator : AbstractValidator<UpdateFeeItemCommand>
{
    public UpdateFeeItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fee item ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee item ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee item name is required")
            .MaximumLength(100).WithMessage("Fee item name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Fee item description is required")
            .MaximumLength(500).WithMessage("Fee item description cannot exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Fee item category is required")
            .MaximumLength(50).WithMessage("Fee item category cannot exceed 50 characters");
    }
}