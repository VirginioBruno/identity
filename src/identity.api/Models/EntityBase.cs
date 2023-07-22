namespace identity.api.Models;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreationAt { get; set; }
    public bool IsActive { get; set; }
}