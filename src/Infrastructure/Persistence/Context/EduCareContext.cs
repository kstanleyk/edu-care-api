using EduCare.Domain.Entity.Auth;
using EduCare.Domain.Entity.Core;
using EduCare.Infrastructure.Persistence.Configurations.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Context;

public class EduCareContext(DbContextOptions<EduCareContext> options) : DbContext(options)
{
    //Auth
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();

    //Core
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<FeeItem> FeeItems => Set<FeeItem>();
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Scholarship> Scholarships => Set<Scholarship>();
    public DbSet<Bursary> Bursaries => Set<Bursary>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new SchoolConfiguration());
        modelBuilder.ApplyConfiguration(new AcademicYearConfiguration());
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new ClassConfiguration());
        modelBuilder.ApplyConfiguration(new FeeItemConfiguration());
        modelBuilder.ApplyConfiguration(new FeeStructureConfiguration());
        modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new ParentConfiguration());
        modelBuilder.ApplyConfiguration(new ScholarshipConfiguration());
        modelBuilder.ApplyConfiguration(new BursaryConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
    }
}

