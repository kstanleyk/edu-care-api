using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetPaymentQuery(Guid PaymentId) : IRequest<Result<PaymentDto>>;

public class GetPaymentQueryHandler(
    IPaymentRepository paymentRepository)
    : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(GetPaymentQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Get payment by ID with all related details
            var payment = await paymentRepository.GetByIdWithFullDetailsAsync(query.PaymentId);
            if (payment is null)
            {
                return Result<PaymentDto>.Failed(
                    Error.NotFound(
                        "Payment.NotFound",
                        $"Payment with ID {query.PaymentId} was not found"
                    )
                );
            }

            // Manually map Payment to PaymentDto without AutoMapper
            var paymentDto = MapToPaymentDto(payment);

            return Result<PaymentDto>.Succeeded(paymentDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting payment {PaymentId}", query.PaymentId);

            return Result<PaymentDto>.Failed(
                Error.Failure(
                    "PaymentQuery.Failed",
                    $"An error occurred while retrieving the payment: {ex.Message}"
                )
            );
        }
    }

    private static PaymentDto MapToPaymentDto(Payment payment)
    {
        return new PaymentDto(
            Id: payment.Id,
            //EnrollmentId: payment.EnrollmentId,
            BursaryId: payment.BursaryId,
            Amount: payment.Amount,
            PaymentDate: payment.PaymentDate,
            PaymentMethod: payment.PaymentMethod,
            ReferenceNumber: payment.ReferenceNumber,
            Notes: payment.Notes,
            CreatedOn: payment.CreatedOn,
            ModifiedOn: payment.ModifiedOn,
            //StudentName: payment.Enrollment?.Student?.Name?.FullName ?? string.Empty,
            //StudentCode: payment.Enrollment?.Student?.StudentId ?? string.Empty,
            //ClassName: payment.Enrollment?.Class?.Name ?? string.Empty,
            //AcademicYearName: payment.Enrollment?.AcademicYear?.Name ?? string.Empty,
            BursaryName: payment.Bursary?.Name ?? string.Empty
            //BursaryCode: payment.Bursary?.Code ?? string.Empty
        );
    }
}