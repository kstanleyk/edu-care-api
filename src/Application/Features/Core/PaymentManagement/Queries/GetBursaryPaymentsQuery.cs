using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetBursaryPaymentsQuery(Guid BursaryId, DateOnly FromDate, DateOnly ToDate)
    : IRequest<Result<List<PaymentDto>>>;

public class GetBursaryPaymentsQueryHandler(
    IBursaryRepository bursaryRepository,
    IPaymentRepository paymentRepository)
    : IRequestHandler<GetBursaryPaymentsQuery, Result<List<PaymentDto>>>
{
    public async Task<Result<List<PaymentDto>>> Handle(GetBursaryPaymentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate bursary exists
            var bursary = await bursaryRepository.GetByIdAsync(query.BursaryId);
            if (bursary is null)
            {
                return Result<List<PaymentDto>>.Failed(
                    Error.NotFound(
                        "Bursary.NotFound",
                        $"Bursary with ID {query.BursaryId} was not found"
                    )
                );
            }

            // Validate date range
            if (query.FromDate > query.ToDate)
            {
                return Result<List<PaymentDto>>.Failed(
                    Error.Validation(
                        "DateRange.Invalid",
                        "From date must be before or equal to to date"
                    )
                );
            }

            // Get payments for the bursary within the date range
            var payments = await paymentRepository.GetByBursaryAndDateRangeAsync(
                query.BursaryId, query.FromDate, query.ToDate);

            // Manually map Payment entities to PaymentDto list without AutoMapper
            var paymentDtos = payments.Select(MapToPaymentDto).ToList();

            return Result<List<PaymentDto>>.Succeeded(paymentDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting bursary payments for bursary {BursaryId}", query.BursaryId);

            return Result<List<PaymentDto>>.Failed(
                Error.Failure(
                    "BursaryPaymentsQuery.Failed",
                    $"An error occurred while retrieving the bursary payments: {ex.Message}"
                )
            );
        }
    }

    private static PaymentDto MapToPaymentDto(Payment payment)
    {
        //return new PaymentDto(
        //    Id: payment.Id,
        //    EnrollmentId: payment.EnrollmentId,
        //    BursaryId: payment.BursaryId,
        //    Amount: payment.Amount,
        //    PaymentDate: payment.PaymentDate,
        //    PaymentMethod: payment.PaymentMethod,
        //    ReferenceNumber: payment.ReferenceNumber,
        //    Notes: payment.Notes,
        //    CreatedOn: payment.CreatedOn,
        //    ModifiedOn: payment.ModifiedOn,
        //    StudentName: payment.Enrollment.Student.Name.FullName,
        //    StudentCode: payment.Enrollment.Student.StudentId,
        //    ClassName: payment.Enrollment.Class.Name,
        //    BursaryName: payment.Bursary.Name
        //);

        return new PaymentDto(
            Id: payment.Id,
            Amount: payment.Amount,
            PaymentDate: payment.PaymentDate,
            PaymentMethod: payment.PaymentMethod,
            ReferenceNumber: payment.ReferenceNumber,
            Notes: payment.Notes,
            BursaryId: payment.BursaryId,
            BursaryName: payment.Bursary?.Name ?? string.Empty,
            CreatedOn: payment.CreatedOn,
            ModifiedOn: payment.ModifiedOn
        );
    }
}