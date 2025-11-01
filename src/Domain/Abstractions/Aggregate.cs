namespace EduCare.Domain.Abstractions;


// Base Aggregate Root
public abstract class Aggregate<TId> : Entity<TId> where TId : notnull
{
    protected Aggregate() { }
}