using Asp.Versioning;
using identity.api.Models;
using identity.api.Repositories;
using identity.api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity.api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Post([FromBody] CreateUserRequest request)
    {
        var user = (User)request;
        var storedUser = await _userRepository.Insert(user);
        return Created(nameof(Post), storedUser);
    }
    
    [HttpPatch]
    [Route("/inactivate/{userId:guid}")]
    public async Task<ActionResult<User>> Inactivate([FromQuery] Guid userId)
    {
        var user = await _userRepository.GetByExpression(x => x.IsActive && x.Id.Equals(userId));
        
        if (user is null)
            return NotFound($"The user was not found");
        
        user.IsActive = false;
        var updatedUser = await _userRepository.Update(user);
        return Ok(updatedUser);
    }
    
    [HttpPatch]
    [Route("/activate/{userId:guid}")]
    public async Task<ActionResult<User>> Activate([FromQuery] Guid userId)
    {
        var user = await _userRepository.GetByExpression(x => !x.IsActive && x.Id.Equals(userId));
        
        if (user is null)
            return NotFound($"The user was not found");
        
        user.IsActive = true;
        var updatedUser = await _userRepository.Update(user);
        return Ok(updatedUser);
    }
}