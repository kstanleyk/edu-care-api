using EduCare.Application.Features.Core.BursaryManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IBursaryRepository : IRepository<Bursary, Guid>
{
    Task<Bursary?> GetByIdWithSchoolsAsync(Guid id);
    Task<RepositoryActionResult<Bursary>> AssignSchoolAsync(AssignSchoolToBursaryParameters parameters);
    Task<RepositoryActionResult<Bursary>> CreateBursaryAsync(CreateBursaryParameters parameters);
    Task<Bursary?> GetByCodeAsync(string code);
    Task<RepositoryActionResult<Bursary>> UpdateBursaryAsync(UpdateBursaryParameters parameters);
}