using Asp.Versioning;
using identity.api.Repositories;
using identity.api.Requests;
using identity.api.Responses;
using identity.api.Utils;
using identity.api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace identity.api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthenticationController(IUserRepository userRepository, ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
    }

    [HttpPost]
    public async Task<ActionResult<AuthenticationResponse>> Post([FromBody] AuthenticationRequest request)
    {
        var user = await _userRepository.GetByUsername(request.Username);

        if (user is null || !UserValidator.Validate(user.HashPassword, request.Password))
            return Unauthorized("The username or password is invalid");

        var token = _tokenGenerator.Generate(user);
        return Ok(new AuthenticationResponse(token));
    }
}
