using System.Linq.Expressions;
using EduCare.Application.Helpers;
using EduCare.Domain.Abstractions;

namespace EduCare.Application.Interfaces;

public interface IRepository { }

public interface IRepository<TEntity, TId> : IDisposable, IRepository
    where TEntity : Entity<TId>
{
    Task<TEntity?> GetAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> where);
    Task<TEntity[]> GetAllAsync();
    Task<int> GetCountAsync();
    Task<RepositoryActionResult<IEnumerable<TEntity>>> LoadAsync(IEnumerable<TEntity> entities);
    Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> where);
    Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, string>> orderBy);
    Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, string>> orderBy,
        Expression<Func<TEntity, string>> thenBy);
    Task<RepositoryActionResult<TEntity>> AddAsync(TEntity employee);
    Task<RepositoryActionResult<IEnumerable<TEntity>>> AddManyAsync(IEnumerable<TEntity> entities);
    Task<RepositoryActionResult<TEntity>> EditAsync(TEntity entity);
    Task<RepositoryActionResult<TEntity>> DeleteAsync(TEntity entity);
    Task<RepositoryActionResult<TEntity>> DeleteManyAsync(Expression<Func<TEntity, bool>> where);
    Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> where);
    Task<TId[]> GetIdsAsync();
    Task<TEntity?> GetFirstOrDefaultAsync();
    Task<RepositoryActionResult<TEntity[]>> EditManyAsync(IEnumerable<TEntity> entities);
    Task<string> GenerateReferenceAsync(Expression<Func<TEntity, string>> columnSelector,
        DateTime transactionDate, int serialLength = 4);
    Task<RepositoryActionResult<TEntity>> PatchAsync(
        TEntity incomingEntity,
        params Expression<Func<TEntity, object>>[] updatedProperties);
    Task<TId[]> GetAllIdsAsync();
}