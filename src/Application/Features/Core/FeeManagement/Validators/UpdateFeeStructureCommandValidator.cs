using EduCare.Application.Features.Core.FeeManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.FeeManagement.Validators;

public class UpdateFeeStructureCommandValidator : AbstractValidator<UpdateFeeStructureCommand>
{
    public UpdateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .NotEqual(Guid.Empty).WithMessage("Fee structure ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee structure name is required")
            .MaximumLength(100).WithMessage("Fee structure name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Fee structure description is required")
            .MaximumLength(500).WithMessage("Fee structure description cannot exceed 500 characters");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective from date is required");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to date must be after effective from date");
    }
}