using identity.api.Requests;
using System.Text.Json.Serialization;
using BC = BCrypt.Net.BCrypt;

namespace identity.api.Models;

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [JsonIgnore]
    public string HashPassword { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = new();

    public static User Create(CreateUserRequest request, Role role) =>
        new()
        {
            Username = request.Username,
            Email = request.Email,
            RoleId = role.Id,
            Role = role,
            CreationAt = DateTime.UtcNow,
            IsActive = true,
            HashPassword = BC.HashPassword(request.Password)
        };
}
