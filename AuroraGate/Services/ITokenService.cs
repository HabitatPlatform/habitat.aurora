using AuroraGate.Domain;

namespace AuroraGate.Services;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken)> IssueTokensAsync(User user, CancellationToken ct);
    Task<(string accessToken, string refreshToken)> RotateRefreshAsync(string refreshToken, CancellationToken ct);
}
