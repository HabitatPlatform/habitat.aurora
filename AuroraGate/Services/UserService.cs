using AuroraGate.Data;
using AuroraGate.Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Linq;  
namespace AuroraGate.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> users;
    private readonly IMongoCollection<Role> roles;
    private readonly IPasswordHasher hasher;

    public UserService(
        IMongoCollection<User> users,
        IMongoCollection<Role> roles,
        IPasswordHasher hasher)
    {
        this.users = users;
        this.roles = roles;
        this.hasher = hasher;
    }

    public async Task<User> CreateAsync(string username, string email, string password, string? role, CancellationToken ct = default)
    {
        // unicidad por índice; validar amigable
        var exists = await users.Find(x => x.Username == username || x.Email == email).AnyAsync(ct);
        if (exists) throw new InvalidOperationException("Usuario ya existe");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hasher.Hash(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = await roles.Find(x => x.Name == role).FirstOrDefaultAsync(ct);
            if (r is null)
            {
                r = new Role { Name = role };
                await roles.InsertOneAsync(r, cancellationToken: ct);
            }
            user.UserRoles.Add(new UserRole { RoleId = r.Id, Role = r });
        }

        await users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public async Task<User?> GetAsync(Guid id, CancellationToken ct = default) =>
        await users.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<User>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        if (take <= 0) take = 50;
        return await users.Find(FilterDefinition<User>.Empty)
                          .SortByDescending(x => x.CreatedAt)
                          .Skip(skip)/*.Take(take)*/
                          .ToListAsync(ct);
    }

    public async Task UpdateAsync(Guid id, string? email, bool? isActive, string? newPassword, string? role, CancellationToken ct = default)
    {
        var updateDef = new List<UpdateDefinition<User>>();
        var ub = Builders<User>.Update;

        if (!string.IsNullOrWhiteSpace(email)) updateDef.Add(ub.Set(x => x.Email, email));
        if (isActive.HasValue) updateDef.Add(ub.Set(x => x.IsActive, isActive.Value));
        if (!string.IsNullOrWhiteSpace(newPassword)) updateDef.Add(ub.Set(x => x.PasswordHash, hasher.Hash(newPassword)));

        // rol: sustituir por uno
        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = await roles.Find(x => x.Name == role).FirstOrDefaultAsync(ct);
            if (r is null)
            {
                r = new Role { Name = role };
                await roles.InsertOneAsync(r, cancellationToken: ct);
            }
            // Reemplazar colección con un solo rol
            updateDef.Add(ub.Set(x => x.UserRoles, new List<UserRole> { new() { RoleId = r.Id, Role = r } }));
        }

        if (updateDef.Count == 0) return;

        var result = await users.UpdateOneAsync(x => x.Id == id, ub.Combine(updateDef), cancellationToken: ct);
        if (result.MatchedCount == 0) throw new KeyNotFoundException("No existe");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var result = await users.DeleteOneAsync(x => x.Id == id, ct);
        if (result.DeletedCount == 0) throw new KeyNotFoundException("No existe");
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct)
    {
        var user = await users.Find(x => x.Username == username && x.IsActive).FirstOrDefaultAsync(ct);
        if (user is null) return null;
        return hasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
