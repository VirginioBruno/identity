namespace identity.api.Models;

public class Role : EntityBase
{
    public string RoleName { get; set; }

    public virtual List<User> Users { get; set; }
}