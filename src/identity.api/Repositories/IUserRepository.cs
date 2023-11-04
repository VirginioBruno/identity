using System.Linq.Expressions;
using identity.api.Models;

namespace identity.api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsername(string username);
    Task<User> Insert(User user);
    Task<User> Update(User user);
    Task<User?> GetById(Guid id);
    Task<User?> GetByExpression(Expression<Func<User, bool>> expression);
}