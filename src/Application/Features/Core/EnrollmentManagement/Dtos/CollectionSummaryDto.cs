using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record CollectionSummaryDto
{
    public Guid BursaryId { get; init; }
    public string BursaryName { get; init; } = null!;
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public Money TotalCollections { get; init; } = null!;
    public int TotalPayments { get; init; }
    public Money AveragePayment { get; init; } = null!;
    public Money HighestPayment { get; init; } = null!;
    public Money LowestPayment { get; init; } = null!;
    public Dictionary<string, Money> CollectionsByPaymentMethod { get; init; } = new();
    public Dictionary<string, int> PaymentCountByMethod { get; init; } = new();
    public List<DailyCollectionDto> DailyCollections { get; init; } = new();
}