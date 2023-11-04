namespace identity.api.Models;

public class Role : EntityBase
{
    public string RoleName { get; set; } = string.Empty;

    public virtual List<User> Users { get; set; } = new();
}