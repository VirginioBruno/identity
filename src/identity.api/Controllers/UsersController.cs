using Asp.Versioning;
using identity.api.Models;
using identity.api.Repositories;
using identity.api.Requests;
using identity.api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity.api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository) => _userRepository = userRepository;

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Post([FromBody] CreateUserRequest request)
    {
        if (await _userRepository.UsernameExists(request.Username))
            return Conflict("The username is already in use.");

        var role = await _userRepository.GetRoleByName(request.Role.ToLowerInvariant());
        if (role is null)
            return BadRequest("The requested role is not available.");

        var user = identity.api.Models.User.Create(request, role);
        var storedUser = await _userRepository.Insert(user);
        return Created($"api/v1/users/{storedUser.Id}", UserResponse.From(storedUser));
    }
    
    [HttpPatch("{userId:guid}/inactivate")]
    public async Task<ActionResult<UserResponse>> Inactivate(Guid userId)
    {
        var user = await _userRepository.GetByExpression(x => x.IsActive && x.Id.Equals(userId));
        
        if (user is null)
            return NotFound($"The user was not found");
        
        user.IsActive = false;
        var updatedUser = await _userRepository.Update(user);
        return Ok(UserResponse.From(updatedUser));
    }
    
    [HttpPatch("{userId:guid}/activate")]
    public async Task<ActionResult<UserResponse>> Activate(Guid userId)
    {
        var user = await _userRepository.GetByExpression(x => !x.IsActive && x.Id.Equals(userId));
        
        if (user is null)
            return NotFound($"The user was not found");
        
        user.IsActive = true;
        var updatedUser = await _userRepository.Update(user);
        return Ok(UserResponse.From(updatedUser));
    }
}
