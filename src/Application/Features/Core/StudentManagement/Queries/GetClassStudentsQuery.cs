using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetClassStudentsQuery(Guid ClassId) : IRequest<Result<List<StudentDto>>>;

public class GetClassStudentsQueryHandler(
    IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<GetClassStudentsQuery, Result<List<StudentDto>>>
{
    public async Task<Result<List<StudentDto>>> Handle(GetClassStudentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (query.ClassId == Guid.Empty)
            {
                return Result<List<StudentDto>>.Failed(
                    Error.Validation(
                        "ClassId.Empty",
                        "Class ID cannot be empty"
                    )
                );
            }

            // Get all active enrollments for the class with student details
            var enrollments = await enrollmentRepository.GetActiveEnrollmentsByClassIdAsync(query.ClassId);

            if (!enrollments.Any())
            {
                return Result<List<StudentDto>>.Succeeded([]);
            }

            // Manually map each enrollment's student to StudentDto without AutoMapper
            var studentDtos = enrollments
                .Select(e => MapToStudentDto(e.Student!))
                .ToList();

            return Result<List<StudentDto>>.Succeeded(studentDtos);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting students for class {ClassId}", query.ClassId);

            return Result<List<StudentDto>>.Failed(
                Error.Failure(
                    "ClassStudentsQuery.Failed",
                    $"An error occurred while retrieving students for the class: {ex.Message}"
                )
            );
        }
    }

    private static StudentDto MapToStudentDto(Student student)
    {
        return new StudentDto(
            Id: student.Id,
            StudentId: student.StudentId,
            FirstName: student.Name.FirstName,
            LastName: student.Name.LastName,
            MiddleName: student.Name.MiddleName,
            FullName: student.Name.FullName,
            DateOfBirth: student.DateOfBirth,
            Gender: student.Gender,
            CreatedOn: student.CreatedOn,
            ModifiedOn: student.ModifiedOn,
            Parents: student.Parents.Select(MapToParentDto).ToList(),
            CurrentEnrollment: GetCurrentEnrollmentDto(student),
            EnrollmentHistory: GetEnrollmentHistoryDtos(student)
        );
    }

    private static ParentDto MapToParentDto(Parent parent)
    {
        return new ParentDto(
            Id: parent.Id,
            FirstName: parent.Name.FirstName,
            LastName: parent.Name.LastName,
            MiddleName: parent.Name.MiddleName,
            FullName: parent.Name.FullName,
            Email: parent.Email,
            Phone: parent.Phone,
            Relationship: parent.Relationship,
            IsPrimaryContact: parent.IsPrimaryContact,
            Address: parent.Address,
            CreatedOn: parent.CreatedOn
        );
    }

    private static EnrollmentSummaryDto? GetCurrentEnrollmentDto(Student student)
    {
        var currentEnrollment = student.Enrollments.FirstOrDefault(e => e.IsActive);
        if (currentEnrollment is null)
            return null;

        return new EnrollmentSummaryDto(
            Id: currentEnrollment.Id,
            ClassName: currentEnrollment.Class?.Name ?? string.Empty,
            ClassCode: currentEnrollment.Class?.Code ?? string.Empty,
            AcademicYearName: currentEnrollment.AcademicYear?.Name ?? string.Empty,
            EnrollmentDate: currentEnrollment.EnrollmentDate,
            IsActive: currentEnrollment.IsActive
        );
    }

    private static List<EnrollmentSummaryDto> GetEnrollmentHistoryDtos(Student student)
    {
        return student.Enrollments
            .Where(e => !e.IsActive) // Exclude current enrollment
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e => new EnrollmentSummaryDto(
                Id: e.Id,
                ClassName: e.Class?.Name ?? string.Empty,
                ClassCode: e.Class?.Code ?? string.Empty,
                AcademicYearName: e.AcademicYear?.Name ?? string.Empty,
                EnrollmentDate: e.EnrollmentDate,
                IsActive: e.IsActive
            ))
            .ToList();
    }
}