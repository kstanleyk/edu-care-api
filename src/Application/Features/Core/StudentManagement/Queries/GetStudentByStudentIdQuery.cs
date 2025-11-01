using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetStudentByStudentIdQuery(string StudentId) : IRequest<StudentDto>;