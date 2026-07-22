using identity.api.Models;

namespace identity.api.Responses;

public sealed record UserResponse(
    Guid Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreationAt)
{
    public static UserResponse From(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.Role.RoleName,
        user.IsActive,
        user.CreationAt);
}
