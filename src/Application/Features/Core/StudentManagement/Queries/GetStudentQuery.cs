using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetStudentQuery(Guid Id) : IRequest<Result<StudentDto>>;

public class GetStudentQueryHandler(
    IStudentRepository studentRepository)
    : IRequestHandler<GetStudentQuery, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(GetStudentQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Get student by ID with all related details
            var student = await studentRepository.GetByIdWithDetailsAsync(query.Id);
            if (student is null)
            {
                return Result<StudentDto>.Failed(
                    Error.NotFound(
                        "Student.NotFound",
                        $"Student with ID {query.Id} was not found"
                    )
                );
            }

            // Manually map Student to StudentDto without AutoMapper
            var studentDto = MapToStudentDto(student);

            return Result<StudentDto>.Succeeded(studentDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting student {StudentId}", query.Id);

            return Result<StudentDto>.Failed(
                Error.Failure(
                    "StudentQuery.Failed",
                    $"An error occurred while retrieving the student: {ex.Message}"
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