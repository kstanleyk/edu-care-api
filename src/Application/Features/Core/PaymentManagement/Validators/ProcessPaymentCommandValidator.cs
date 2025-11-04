using EduCare.Application.Features.Core.PaymentManagement.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Application.Features.Core.PaymentManagement.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Enrollment ID cannot be empty");

        RuleFor(x => x.BursaryId)
            .NotEmpty().WithMessage("Bursary ID is required")
            .NotEqual(Guid.Empty).WithMessage("Bursary ID cannot be empty");

        RuleFor(x => x.Amount)
            .NotNull().WithMessage("Payment amount is required")
            .Must(amount => amount.Amount > 0).WithMessage("Payment amount must be greater than zero");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters");

        RuleFor(x => x.ReferenceNumber)
            .NotEmpty().WithMessage("Reference number is required")
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => x.Notes != null);
    }
}