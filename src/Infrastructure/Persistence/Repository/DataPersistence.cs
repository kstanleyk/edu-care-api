using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces;
using EduCare.Domain.Abstractions;
using EduCare.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository;

public abstract class DataRepositoryBase<TEntity, TContext, TId>(TContext context) : Disposable, IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TContext : DbContext
    where TId : notnull
{
    protected TContext Context = context;
    protected IDbConnection Db = context.Database.GetDbConnection();
    protected string ConnectionString = context.Database.GetDbConnection().ConnectionString;

    protected DbSet<TEntity> DbSet { get; } = context.Set<TEntity>();

    public virtual async Task<RepositoryActionResult<TEntity>> AddAsync(TEntity entity)
    {
        try
        {
            DbSet.Add(entity);
            var result = await SaveChangesAsync();
            return result != 0
                ? new RepositoryActionResult<TEntity>(entity, RepositoryActionStatus.Created)
                : new RepositoryActionResult<TEntity>(entity, RepositoryActionStatus.NothingModified);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity>> AddAsync(TEntity entity, int propertyLength)
    {
        if (entity.Id == null)
            throw new ArgumentNullException(nameof(entity.Id), "Entity SequentialId cannot be null.");

        return await AddAsync(entity, x => x.Id, nameof(entity.Id), propertyLength);
    }

    public virtual async Task<RepositoryActionResult<TEntity>> AddAsync(TEntity entity,
        Expression<Func<TEntity, TId>> sortOrder, string keySelector, int propertyLength)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var lastEntity = DbSet.OrderByDescending(sortOrder).ToArray().FirstOrDefault();

            var lastCode = string.Empty;


            if (lastEntity == null)
            {
                lastCode = "1";
            }
            else
            {
                var properties = lastEntity.GetType().GetProperties().ToList();
                foreach (var property in properties)
                {
                    if (property.Name != keySelector) continue;
                    var value = property.GetValue(lastEntity, null);
                    if (value is string strValue)
                    {
                        lastCode = (strValue.ToNumValue() + 1)
                            .ToString(CultureInfo.InvariantCulture);
                    }
                    else if (value != null)
                    {
                        lastCode = (value.ToString()!.ToNumValue() + 1)
                            .ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        lastCode = "1";
                    }
                    break;
                }
            }

            var serial = lastCode.PadLeft(propertyLength,'0');
            var targetProperties = entity.GetType().GetProperties().ToList();
            foreach (var property in targetProperties.Where(property => property.Name == keySelector))
            {
                property.SetValue(entity, serial, null);
                break;
            }

            DbSet.Add(entity);

            var result = await SaveChangesAsync();
            if (result == 0)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<TEntity>(entity, RepositoryActionStatus.NothingModified);
            }

            await tx.CommitAsync();

            return new RepositoryActionResult<TEntity>(entity, RepositoryActionStatus.Created);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<IEnumerable<TEntity>>> AddManyAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            DbSet.AddRange(enumerable);
            var result = await SaveChangesAsync();
            return result != 0
                ? new RepositoryActionResult<IEnumerable<TEntity>>(enumerable, RepositoryActionStatus.Created)
                : new RepositoryActionResult<IEnumerable<TEntity>>(enumerable,
                    RepositoryActionStatus.NothingModified);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<IEnumerable<TEntity>>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity[]>> EditManyAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();

            foreach (var entity in enumerable)
            {
                var existingEntity = await ItemToGetAsync(entity);
                if (existingEntity == null)
                {
                    return new RepositoryActionResult<TEntity[]>(null, RepositoryActionStatus.NotFound);
                }

                // Attach the tracked createDepartment
                DbSet.Attach(existingEntity);

                // Copy values from input createDepartment to the tracked createDepartment
                Context.Entry(existingEntity).CurrentValues.SetValues(entity);
            }

            var result = await SaveChangesAsync();
            return result != 0
                ? new RepositoryActionResult<TEntity[]>(enumerable, RepositoryActionStatus.Updated)
                : new RepositoryActionResult<TEntity[]>(enumerable, RepositoryActionStatus.NothingModified);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity[]>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity>> DeleteAsync(TEntity entity)
    {
        try
        {
            var existingEntity = await ItemToGetAsync(entity);
            if (existingEntity == null)
                return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.NotFound);

            Context.Entry(existingEntity).State = EntityState.Detached;

            DbSet.Remove(existingEntity);

            var result = await SaveChangesAsync();
            return result > 0
                ? new RepositoryActionResult<TEntity>(entity, RepositoryActionStatus.Deleted)
                : new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.NotFound);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity>> TruncateAsync()
    {
        try
        {
            var itemsToDelete = await DbSet.ToArrayAsync();
            DbSet.RemoveRange(itemsToDelete);
            await SaveChangesAsync();
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Deleted);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<IEnumerable<TEntity>>> LoadAsync(IEnumerable<TEntity> entities)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var truncate = await TruncateAsync();
            if (truncate.Status != RepositoryActionStatus.Deleted)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<IEnumerable<TEntity>>(null, RepositoryActionStatus.Error);
            }

            var result = await AddManyAsync(entities);
            if (result.Status != RepositoryActionStatus.Created) await tx.RollbackAsync();
            return result;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<IEnumerable<TEntity>>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity>> DeleteManyAsync(Expression<Func<TEntity, bool>> where)
    {
        try
        {
            var entities = DbSet.Where(where).AsEnumerable();

            var identifiableEntities = entities as TEntity[] ?? entities.ToArray();
            if (!identifiableEntities.Any())
                return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.NotFound);

            foreach (var entity in identifiableEntities) DbSet.Remove(entity);

            var result = await SaveChangesAsync();
            return result > 0
                ? new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Deleted)
                : new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.NotFound);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity>> EditAsync(TEntity entity)
    {
        try
        {
            var existingEntity = await ItemToGetAsync(entity);
            if (existingEntity == null)
                return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(existingEntity);

            // Handle detached entities
            if (entry.State == EntityState.Detached)
            {
                DbSet.Attach(existingEntity);
            }

            // Identify concurrency token properties
            var concurrencyProperties = entry.Metadata.GetProperties()
                .Where(p => p.IsConcurrencyToken)
                .ToList();

            // Store original concurrency values
            var originalConcurrencyValues = new Dictionary<string, object?>();
            foreach (var property in concurrencyProperties)
            {
                originalConcurrencyValues[property.Name] = entry.Property(property.Name).OriginalValue;
            }

            // Update createDepartment with new values
            entry.CurrentValues.SetValues(entity);

            // Restore original concurrency values
            foreach (var property in concurrencyProperties)
            {
                entry.Property(property.Name).CurrentValue = originalConcurrencyValues[property.Name];
                entry.Property(property.Name).OriginalValue = originalConcurrencyValues[property.Name];
            }

            // Check for changes
            var modifiedProperties = new List<string>();
            foreach (var property in entry.Properties)
            {
                if (concurrencyProperties.Any(p => p.Name == property.Metadata.Name)) continue;

                if (!Equals(property.OriginalValue, property.CurrentValue))
                {
                    property.IsModified = true;
                    modifiedProperties.Add(property.Metadata.Name);
                }
            }

            // Early exit if no changes
            if (modifiedProperties.Count == 0)
            {
                return new RepositoryActionResult<TEntity>(existingEntity, RepositoryActionStatus.NothingModified);
            }

            // Save changes
            var result = await SaveChangesAsync();
            if (result > 0)
            {
                // Reload the entity to return a fresh copy from the DB
                await entry.ReloadAsync();
                return new RepositoryActionResult<TEntity>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<TEntity>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public virtual async Task<RepositoryActionResult<TEntity>> PatchAsync(TEntity incomingEntity,
        params Expression<Func<TEntity, object>>[] updatedProperties)
    {
        try
        {
            var existingEntity = await ItemToGetAsync(incomingEntity);
            if (existingEntity == null)
                return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(existingEntity);

            // Attach if detached
            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            // Apply updated values only for specified properties
            foreach (var propertyExpr in updatedProperties)
            {
                var propertyName = GetPropertyName(propertyExpr);
                var propertyEntry = entry.Property(propertyName);
                var newValue = typeof(TEntity).GetProperty(propertyName)?.GetValue(incomingEntity);

                if (!Equals(propertyEntry.CurrentValue, newValue))
                {
                    propertyEntry.CurrentValue = newValue;
                    propertyEntry.IsModified = true;
                }
            }

            // Early exit if no changes
            if (!entry.Properties.Any(p => p.IsModified))
                return new RepositoryActionResult<TEntity>(existingEntity, RepositoryActionStatus.NothingModified);

            var result = await SaveChangesAsync();

            if (result > 0)
            {
                await entry.ReloadAsync(); // Return the fully updated object from DB
                return new RepositoryActionResult<TEntity>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<TEntity>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<TEntity>(null, RepositoryActionStatus.Error, ex);
        }
    }

    private static string GetPropertyName(Expression<Func<TEntity, object>> expression)
    {
        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is MemberExpression memberExpr)
            return memberExpr.Member.Name;

        throw new ArgumentException("Invalid property expression");
    }

    public virtual async Task<TEntity[]> GetAllAsync() => await ItemsToGetAsync();

    public virtual async Task<TId[]> GetAllIdsAsync() => await DbSet.AsNoTracking().Select(x => x.Id).ToArrayAsync();

    public virtual async Task<TEntity?> GetAsync(TEntity entity) => await ItemToGetAsync(entity);

    public virtual async Task<TEntity?> GetAsync(TId id) => await ItemToGetAsync(id);

    public virtual async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> where) =>
        await DbSet.Where(where).FirstOrDefaultAsync();

    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> where) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(where);

    public virtual async Task<TId[]> GetIdsAsync() => await DbSet.Select(x => x.Id).ToArrayAsync();

    public virtual async Task<TEntity?> GetFirstOrDefaultAsync() =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync();

    public virtual async Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> where) =>
        await DbSet.AsNoTracking().Where(where).ToArrayAsync();

    public virtual async Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, string>> orderBy) =>
        await DbSet.AsNoTracking().Where(where).OrderBy(orderBy).ToArrayAsync();

    public virtual async Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, string>> orderBy, Expression<Func<TEntity, string>> thenBy) =>
        await DbSet.AsNoTracking().Where(where).OrderBy(orderBy).ThenBy(thenBy).ToArrayAsync();

    protected virtual async Task<TEntity[]> ItemsToGetAsync() => await DbSet.AsNoTracking().OrderBy(x=>x.Id).ToArrayAsync();

    protected virtual async Task<TEntity?> ItemToGetAsync(TEntity entity)
    {
        if (entity.Id == null)
            throw new ArgumentNullException(nameof(entity.Id), "Entity SequentialId cannot be null.");

        return await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(entity.Id));
    }

    protected virtual async Task<TEntity?> ItemToGetAsync(TId id) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id.Equals(id));

    public async Task<string> GenerateReferenceAsync(Expression<Func<TEntity, string>> columnSelector,
        DateTime transactionDate, int serialLength = 4)
    {
        var yearPart = transactionDate.ToString("yy"); // Last two digits of the year
        var monthPart = transactionDate.ToString("MM"); // Two-digit month
        var prefix = $"{yearPart}{monthPart}"; // e.g., "2502" for Feb 2025

        // Extract column name dynamically
        var columnName = GetPropertyName(columnSelector);

        // Build the filter dynamically
        var param = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Property(param, columnName);
        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var prefixValue = Expression.Constant(prefix);
        var startsWithExpression = Expression.Call(property, startsWithMethod!, prefixValue);
        var filterLambda = Expression.Lambda<Func<TEntity, bool>>(startsWithExpression, param);

        // Get the latest reference
        var latestReference = await DbSet
            .Where(filterLambda)
            .OrderByDescending(columnSelector)
            .Select(columnSelector)
            .FirstOrDefaultAsync();

        var nextSerial = 1; // Default if no records exist

        if (!string.IsNullOrEmpty(latestReference) && latestReference.Length == 4 + serialLength)
        {
            if (int.TryParse(latestReference.Substring(4, serialLength), out var lastSerial))
            {
                nextSerial = lastSerial + 1;
            }
        }

        // Generate new reference (YYMMXXXX format)
        var newReference = $"{prefix}{nextSerial.ToString().PadLeft(serialLength, '0')}";
        return newReference;
    }

    private static string GetPropertyName(Expression<Func<TEntity, string>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberExpr)
            return memberExpr.Member.Name;

        throw new ArgumentException("Invalid property expression.");
    }

    protected async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();

    public async Task<int> GetCountAsync() => await DbSet.CountAsync();

    protected override void DisposeCore() => Context.Dispose();
}

public abstract class DataRepository<TEntity, TId>(IDatabaseFactory databaseFactory)
    : DataRepositoryBase<TEntity, EduCareContext, TId>(databaseFactory.GetContext())
    where TEntity : Entity<TId> where TId : notnull
{
    protected IDatabaseFactory DatabaseFactory = databaseFactory;

    protected override void DisposeCore()
    {
        DatabaseFactory.Dispose();
        base.DisposeCore();
    }
}
