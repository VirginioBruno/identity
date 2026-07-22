using System.ComponentModel.DataAnnotations;

namespace identity.api.Requests;

public class CreateUserRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(12), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required, RegularExpression("^(admin|user)$", ErrorMessage = "Role must be either 'admin' or 'user'.")]
    public string Role { get; set; } = "user";
}
