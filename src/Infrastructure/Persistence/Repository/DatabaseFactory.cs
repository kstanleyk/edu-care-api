using System.Data;
using EduCare.Application.Helpers;
using EduCare.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EduCare.Infrastructure.Persistence.Repository;

public class DatabaseFactory : Disposable, IDatabaseFactory
{
    public DatabaseFactory(EduCareContext dataContext)
    {
        _dataContext = dataContext;
        _db = new NpgsqlConnection(GetContext().Database.GetDbConnection().ConnectionString);
    }

    private readonly EduCareContext _dataContext;
    private readonly IDbConnection _db;

    public EduCareContext GetContext()
    {
        return _dataContext;
    }

    public IDbConnection GetConnection()
    {
        return _db;
    }

    protected override void DisposeCore()
    {
        _dataContext.Dispose();
    }
}

public interface IDatabaseFactory : IDisposable
{
    EduCareContext GetContext();
    IDbConnection GetConnection();
}