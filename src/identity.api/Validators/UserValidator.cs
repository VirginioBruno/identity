using BC = BCrypt.Net.BCrypt;

namespace identity.api.Validators;

public static class UserValidator
{
    public static bool Validate(string storedPassword, string password) =>
        BC.Verify(password, storedPassword);
}