using identity.api.Models;

namespace identity.api.Utils;

public interface ITokenGenerator
{
    string Generate(User user);
}
