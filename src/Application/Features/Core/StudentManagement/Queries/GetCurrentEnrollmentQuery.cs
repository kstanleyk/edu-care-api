using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetCurrentEnrollmentQuery(Guid StudentId) : IRequest<EnrollmentDto?>;