using Microsoft.EntityFrameworkCore;
using SmartPark.Domain.IdentityAccess;

namespace SmartPark.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(SmartParkDbContext db) : IUserRepository
{
    public Task<UserAccount?> GetByEmailAsync(Email email, CancellationToken ct = default)
        => db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.Email == email, ct);

    public void Add(UserAccount user) => db.Users.Add(user);
}
