using EduCare.Application.Features.Core.FeeManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record GetClassFeeStructuresQuery(Guid ClassId) : IRequest<List<FeeStructureDto>>;