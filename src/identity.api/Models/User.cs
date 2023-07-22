using identity.api.Requests;
using BC = BCrypt.Net.BCrypt;

namespace identity.api.Models;

public class User : EntityBase
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string HashPassword { get; set; }
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; }

    public static explicit operator User(CreateUserRequest request) =>
        new()
        {
            Username = request.Username,
            Email = request.Email,
            RoleId = request.RoleId,
            CreationAt = DateTime.Now,
            IsActive = true,
            HashPassword = BC.HashPassword(request.Password)
        };
}