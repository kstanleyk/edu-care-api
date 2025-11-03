using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetEnrollmentPaymentsQuery(Guid EnrollmentId) : IRequest<Result<List<PaymentDto>>>;

public class GetEnrollmentPaymentsQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    IPaymentRepository paymentRepository)
    : IRequestHandler<GetEnrollmentPaymentsQuery, Result<List<PaymentDto>>>
{
    public async Task<Result<List<PaymentDto>>> Handle(GetEnrollmentPaymentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate enrollment exists
            var enrollment = await enrollmentRepository.GetByIdAsync(query.EnrollmentId);
            if (enrollment is null)
            {
                return Result<List<PaymentDto>>.Failed(
                    Error.NotFound(
                        "Enrollment.NotFound",
                        $"Enrollment with ID {query.EnrollmentId} was not found"
                    )
                );
            }

            // Get all payments for the enrollment
            var payments = await paymentRepository.GetByEnrollmentAsync(query.EnrollmentId);

            // Manually map Payment entities to PaymentDto list without AutoMapper
            var paymentDtos = payments.Select(MapToPaymentDto).ToList();

            return Result<List<PaymentDto>>.Succeeded(paymentDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting payments for enrollment {EnrollmentId}", query.EnrollmentId);

            return Result<List<PaymentDto>>.Failed(
                Error.Failure(
                    "EnrollmentPaymentsQuery.Failed",
                    $"An error occurred while retrieving the enrollment payments: {ex.Message}"
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
            //StudentName: payment.Enrollment.Student.Name.FullName,
            //StudentCode: payment.Enrollment?.Student?.StudentId ?? string.Empty,
            //ClassName: payment.Enrollment?.Class?.Name ?? string.Empty,
            BursaryName: payment.Bursary?.Name ?? string.Empty
        );
    }
}