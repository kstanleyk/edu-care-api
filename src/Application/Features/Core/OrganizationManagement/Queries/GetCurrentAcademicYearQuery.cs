using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

public record GetCurrentAcademicYearQuery(Guid SchoolId) : IRequest<Result<AcademicYearDto?>>;

public class GetCurrentAcademicYearQueryHandler(
    IAcademicYearRepository academicYearRepository,
    ISchoolRepository schoolRepository)
    : IRequestHandler<GetCurrentAcademicYearQuery, Result<AcademicYearDto?>>
{
    public async Task<Result<AcademicYearDto?>> Handle(GetCurrentAcademicYearQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Validate school exists
            var school = await schoolRepository.GetAsync(query.SchoolId);
            if (school is null)
            {
                return Result<AcademicYearDto?>.Failed(
                    Error.NotFound(
                        "School.NotFound",
                        $"School with ID {query.SchoolId} was not found"
                    )
                );
            }

            // Get current academic year for the school
            var currentAcademicYear = await academicYearRepository.GetCurrentBySchoolIdAsync(query.SchoolId);

            if (currentAcademicYear is null)
            {
                // Return success with null value - no current academic year found
                return Result<AcademicYearDto?>.Succeeded(null);
            }

            // Manually map AcademicYear to AcademicYearDto without AutoMapper
            var academicYearDto = MapToAcademicYearDto(currentAcademicYear, school.Name);

            return Result<AcademicYearDto?>.Succeeded(academicYearDto);
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, you'd inject ILogger)
            // _logger.LogError(ex, "Error getting current academic year for school {SchoolId}", query.SchoolId);

            return Result<AcademicYearDto?>.Failed(
                Error.Failure(
                    "CurrentAcademicYearQuery.Failed",
                    $"An error occurred while retrieving the current academic year: {ex.Message}"
                )
            );
        }
    }

    private static AcademicYearDto MapToAcademicYearDto(AcademicYear academicYear, string schoolName)
    {
        return new AcademicYearDto(
            Id: academicYear.Id,
            Name: academicYear.Name,
            Code: academicYear.Code,
            StartDate: academicYear.StartDate,
            EndDate: academicYear.EndDate,
            IsCurrent: academicYear.IsCurrent,
            SchoolId: academicYear.SchoolId,
            //SchoolName: schoolName,
            CreatedOn: academicYear.CreatedOn,
            ModifiedOn: academicYear.ModifiedOn
        );
    }
}