using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class StudentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Student, Guid>(databaseFactory), IStudentRepository
{

}