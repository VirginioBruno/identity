using System.Linq.Expressions;
using identity.api.Infrastructure;
using identity.api.Models;
using Microsoft.EntityFrameworkCore;

namespace identity.api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;
    
    public UserRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> GetByUsername(string username) =>
        await _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Username.Equals(username) && u.IsActive);
    
    public async Task<User> GetById(Guid id) =>
        await _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id.Equals(id));

    public async Task<User> GetByExpression(Expression<Func<User, bool>> expression) =>
        await _dbContext.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(expression);
    
    public async Task<User> Insert(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> Update(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }
}
