using EduCare.Application.Features.Core.FeeManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record GetActiveFeeStructureQuery(Guid ClassId, DateTime? asOf = null) : IRequest<FeeStructureDto?>;