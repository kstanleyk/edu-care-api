using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Enrollment : Aggregate<Guid>
{
    public Guid StudentId { get; private set; }
    public Guid ClassId { get; private set; }
    public Guid AcademicYearId { get; private set; }
    public Guid FeeStructureId { get; private set; } // Reference to the specific fee structure applied
    public DateOnly EnrollmentDate { get; private set; }
    public bool IsActive { get; private set; }

    public Student Student { get; private set; } = null!;
    public Class Class { get; private set; } = null!;
    public AcademicYear AcademicYear { get; private set; } = null!;
    public FeeStructure FeeStructure { get; private set; } = null!;

    private readonly List<Scholarship> _scholarships = [];
    public IReadOnlyCollection<Scholarship> Scholarships => _scholarships.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private readonly List<EnrollmentFeeItem> _selectedOptionalFees = [];
    public IReadOnlyCollection<EnrollmentFeeItem> SelectedOptionalFees => _selectedOptionalFees.AsReadOnly();

    protected Enrollment() { }

    /// <summary>
    /// Creates a new student enrollment with a specific fee structure
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="classId">Class ID</param>
    /// <param name="academicYearId">Academic year ID</param>
    /// <param name="feeStructureId">Fee structure ID to apply</param>
    /// <param name="enrollmentDate">Date of enrollment</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Enrollment Create(Guid studentId, Guid classId, Guid academicYearId,
        Guid feeStructureId, DateOnly enrollmentDate, DateTime? createdOn = null)
    {
        return new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = classId,
            AcademicYearId = academicYearId,
            FeeStructureId = feeStructureId,
            EnrollmentDate = enrollmentDate,
            IsActive = true,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void MarkAsInactive()
    {
        IsActive = false;
        ModifiedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Transfers the student to a different class within the same academic year
    /// </summary>
    /// <param name="newClassId">The new class ID</param>
    /// <param name="newFeeStructureId">The new fee structure ID</param>
    public void UpdateClassAndFeeStructure(Guid newClassId, Guid newFeeStructureId)
    {
        DomainGuards.AgainstDefault(newClassId, nameof(newClassId));
        DomainGuards.AgainstDefault(newFeeStructureId, nameof(newFeeStructureId));

        ClassId = newClassId;
        FeeStructureId = newFeeStructureId;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddScholarship(Scholarship scholarship)
    {
        DomainGuards.AgainstNull(scholarship, nameof(scholarship));

        // Business rule: Cannot have multiple active scholarships of the same type
        if (_scholarships.Any(s => s.Type == scholarship.Type && s.IsActive))
            throw new InvalidOperationException($"Active {scholarship.Type} scholarship already exists");

        _scholarships.Add(scholarship);
    }

    public void AddPayment(Payment payment)
    {
        DomainGuards.AgainstNull(payment, nameof(payment));
        _payments.Add(payment);
    }

    public void SelectOptionalFee(Guid feeItemId)
    {
        // Check if the fee item exists in the fee structure and is optional
        var optionalFee = FeeStructure.FeeItems.FirstOrDefault(fi =>
            fi.FeeItemId == feeItemId && fi.IsOptional);

        if (optionalFee == null)
            throw new InvalidOperationException("Fee item is not available as an optional fee in the current fee structure");

        // Check if already selected
        if (_selectedOptionalFees.Any(sf => sf.FeeItemId == feeItemId))
            throw new InvalidOperationException("Optional fee already selected");

        var enrollmentFeeItem = EnrollmentFeeItem.Create(feeItemId, optionalFee.Amount);
        _selectedOptionalFees.Add(enrollmentFeeItem);
    }

    public void RemoveOptionalFee(Guid feeItemId)
    {
        var selectedFee = _selectedOptionalFees.FirstOrDefault(sf => sf.FeeItemId == feeItemId);
        if (selectedFee != null)
        {
            _selectedOptionalFees.Remove(selectedFee);
        }
    }

    public Money CalculateTotalFees()
    {
        var baseFees = FeeStructure.CalculateTotalFees();
        var optionalFeesTotal = _selectedOptionalFees.Sum(sf => sf.Amount.Amount);

        return new Money(baseFees.Amount + optionalFeesTotal);
    }

    public Money CalculateTotalPaid()
    {
        var totalPaid = _payments.Sum(p => p.Amount.Amount);
        return new Money(totalPaid);
    }

    public Money CalculateBalance()
    {
        var totalFees = CalculateTotalFees();
        var totalPaid = CalculateTotalPaid();
        var scholarshipDiscount = CalculateScholarshipDiscount(totalFees);

        var netFees = totalFees.Amount - scholarshipDiscount;
        var balance = netFees - totalPaid.Amount;

        return new Money(Math.Max(0, balance));
    }

    private decimal CalculateScholarshipDiscount(Money totalFees)
    {
        var activeScholarships = _scholarships.Where(s => s.IsActive).ToArray();
        if (!activeScholarships.Any()) return 0;

        var totalPercentage = activeScholarships.Sum(s => s.Percentage);
        var maxPercentage = Math.Min(totalPercentage, 100); // Cap at 100%

        return totalFees.Amount * (maxPercentage / 100);
    }
}