using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetBursaryCollectionSummaryQuery(Guid BursaryId, DateOnly FromDate, DateOnly ToDate)
    : IRequest<Result<CollectionSummaryDto>>;

public class GetBursaryCollectionSummaryQueryHandler(
    IBursaryRepository bursaryRepository,
    IPaymentRepository paymentRepository)
    : IRequestHandler<GetBursaryCollectionSummaryQuery, Result<CollectionSummaryDto>>
{
    public async Task<Result<CollectionSummaryDto>> Handle(GetBursaryCollectionSummaryQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate bursary exists
            var bursary = await bursaryRepository.GetByIdAsync(query.BursaryId);
            if (bursary is null)
            {
                return Result<CollectionSummaryDto>.Failed(
                    Error.NotFound(
                        "Bursary.NotFound",
                        $"Bursary with ID {query.BursaryId} was not found"
                    )
                );
            }

            // Validate date range
            if (query.FromDate > query.ToDate)
            {
                return Result<CollectionSummaryDto>.Failed(
                    Error.Validation(
                        "DateRange.Invalid",
                        "From date must be before or equal to to date"
                    )
                );
            }

            // Get payments for the bursary within the date range
            var payments = await paymentRepository.GetByBursaryAndDateRangeAsync(
                query.BursaryId, query.FromDate, query.ToDate);

            // Calculate collection summary
            var collectionSummary = CalculateCollectionSummary(payments, query.FromDate, query.ToDate, bursary.Name);

            return Result<CollectionSummaryDto>.Succeeded(collectionSummary);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting bursary collection summary for bursary {BursaryId}", query.BursaryId);

            return Result<CollectionSummaryDto>.Failed(
                Error.Failure(
                    "BursaryCollectionSummaryQuery.Failed",
                    $"An error occurred while retrieving the bursary collection summary: {ex.Message}"
                )
            );
        }
    }

    private static CollectionSummaryDto CalculateCollectionSummary(
        List<Payment> payments, DateOnly fromDate, DateOnly toDate, string bursaryName)
    {
        if (!payments.Any())
        {
            return new CollectionSummaryDto(
                BursaryId: Guid.Empty,
                BursaryName: bursaryName,
                FromDate: fromDate,
                ToDate: toDate,
                TotalCollections: new Money(0),
                TotalPayments: 0,
                AveragePayment: new Money(0),
                HighestPayment: new Money(0),
                LowestPayment: new Money(0),
                CollectionsByPaymentMethod: new Dictionary<string, Money>(),
                PaymentCountByMethod: new Dictionary<string, int>(),
                DailyCollections: CalculateDailyCollections(payments, fromDate, toDate)
            );
        }

        var totalCollections = payments.Sum(p => p.Amount.Amount);
        var totalPayments = payments.Count;
        var averagePayment = totalCollections / totalPayments;
        var highestPayment = payments.Max(p => p.Amount.Amount);
        var lowestPayment = payments.Min(p => p.Amount.Amount);

        var collectionsByPaymentMethod = payments
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key,
                g => new Money(g.Sum(p => p.Amount.Amount))
            );

        var paymentCountByMethod = payments
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        var dailyCollections = CalculateDailyCollections(payments, fromDate, toDate);

        return new CollectionSummaryDto(
            BursaryId: payments.First().BursaryId,
            BursaryName: bursaryName,
            FromDate: fromDate,
            ToDate: toDate,
            TotalCollections: new Money(totalCollections),
            TotalPayments: totalPayments,
            AveragePayment: new Money(averagePayment),
            HighestPayment: new Money(highestPayment),
            LowestPayment: new Money(lowestPayment),
            CollectionsByPaymentMethod: collectionsByPaymentMethod,
            PaymentCountByMethod: paymentCountByMethod,
            DailyCollections: dailyCollections
        );
    }

    private static List<DailyCollectionDto> CalculateDailyCollections(List<Payment> payments, DateOnly fromDate, DateOnly toDate)
    {
        var dailyCollections = new List<DailyCollectionDto>();
        var currentDate = fromDate;

        while (currentDate <= toDate)
        {
            var datePayments = payments.Where(p => DateOnly.FromDateTime(p.PaymentDate) == currentDate).ToList();
            var dailyAmount = datePayments.Sum(p => p.Amount.Amount);
            var paymentCount = datePayments.Count;

            dailyCollections.Add(new DailyCollectionDto(
                Date: currentDate,
                Amount: new Money(dailyAmount),
                PaymentCount: paymentCount
            ));

            currentDate = currentDate.AddDays(1);
        }

        return dailyCollections;
    }
}