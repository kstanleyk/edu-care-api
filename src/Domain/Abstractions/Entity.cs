namespace EduCare.Domain.Abstractions;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedOn { get; protected set; }
    public DateTime? ModifiedOn { get; protected set; }

    protected Entity() { }

    public void SetId(TId id)
    {
        Id = id;
    }
}

