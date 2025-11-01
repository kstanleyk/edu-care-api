using EduCare.Application.Interfaces.Auth;
using EduCare.Application.Interfaces.Core;
using EduCare.Infrastructure.Persistence.Context;
using EduCare.Infrastructure.Persistence.Repository;
using EduCare.Infrastructure.Persistence.Repository.Auth;
using EduCare.Infrastructure.Persistence.Repository.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduCare.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<EduCareContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Value"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<EduCareDatabaseSeeder>();

        services.AddScoped<IDatabaseFactory, DatabaseFactory>();

        //Auth
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        //Core
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();

        return services;
    }

    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var seeders = serviceScope.ServiceProvider.GetServices<EduCareDatabaseSeeder>();

        foreach (var seeder in seeders) seeder.SeedDatabaseAsync().GetAwaiter().GetResult();

        return app;
    }
}