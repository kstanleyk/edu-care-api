using EduCare.Application.Features.Core.BursaryManagement.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Application.Features.Core.BursaryManagement.Validators;

public class UpdateBursaryCommandValidator : AbstractValidator<UpdateBursaryCommand>
{
    public UpdateBursaryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Bursary ID is required")
            .NotEqual(Guid.Empty).WithMessage("Bursary ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Bursary name is required")
            .MaximumLength(100).WithMessage("Bursary name cannot exceed 100 characters");

        // Address validation can be added if needed
        // RuleFor(x => x.Address).SetValidator(new AddressValidator());
    }
}