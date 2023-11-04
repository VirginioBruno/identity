using identity.api.Requests;
using BC = BCrypt.Net.BCrypt;

namespace identity.api.Models;

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashPassword { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = new();

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