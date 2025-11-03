using EduCare.Application.Features.Core.FeeManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.FeeManagement.Validators;

public class RemoveFeeItemFromStructureCommandValidator : AbstractValidator<RemoveFeeItemFromStructureCommand>
{
    public RemoveFeeItemFromStructureCommandValidator()
    {
        RuleFor(x => x.FeeStructureId)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee structure ID cannot be empty");

        RuleFor(x => x.FeeItemId)
            .NotEmpty().WithMessage("Fee item ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee item ID cannot be empty");
    }
}