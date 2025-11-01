namespace EduCare.Domain.Entity;

public interface IEntity<TId>: IEntity
{
    TId Id { get; set; }
}

public interface IEntity
{

}