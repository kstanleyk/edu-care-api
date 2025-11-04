using EduCare.Application.Features.Core.FeeManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.FeeManagement.Validators;

public class AddFeeItemToStructureCommandValidator : AbstractValidator<AddFeeItemToStructureCommand>
{
    public AddFeeItemToStructureCommandValidator()
    {
        RuleFor(x => x.FeeStructureId)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee structure ID cannot be empty");

        RuleFor(x => x.FeeItemId)
            .NotEmpty().WithMessage("Fee item ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee item ID cannot be empty");

        RuleFor(x => x.Amount)
            .NotNull().WithMessage("Amount is required")
            .Must(amount => amount.Amount > 0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");
    }
}