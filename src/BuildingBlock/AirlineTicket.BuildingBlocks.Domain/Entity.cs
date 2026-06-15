namespace AirlineTicket.BuildingBlocks.Domain;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    public bool IsDeleted { get; protected set; } = false;

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}

public abstract class Entity : Entity<Guid>
{
}
