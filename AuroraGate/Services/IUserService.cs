using AuroraGate.Domain;

namespace AuroraGate.Services
{
    public interface IUserService
    {
        Task<User> CreateAsync(string username, string email, string password, string? role = null, CancellationToken ct = default);
        Task<User?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<User>> ListAsync(int skip, int take, CancellationToken ct = default);
        Task UpdateAsync(Guid id, string? email, bool? isActive, string? newPassword, string? role, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct);
    }
}
