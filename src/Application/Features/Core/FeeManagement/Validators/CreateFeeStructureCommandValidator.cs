using EduCare.Application.Features.Core.FeeManagement.Commands;
using FluentValidation;

namespace EduCare.Application.Features.Core.FeeManagement.Validators;

public class CreateFeeStructureCommandValidator : AbstractValidator<CreateFeeStructureCommand>
{
    public CreateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee structure name is required")
            .MaximumLength(100).WithMessage("Fee structure name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Fee structure description is required")
            .MaximumLength(500).WithMessage("Fee structure description cannot exceed 500 characters");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Class ID is required")
            .NotEqual(Guid.Empty).WithMessage("Class ID cannot be empty");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective from date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Effective from date cannot be in the past");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to date must be after effective from date");
    }
}