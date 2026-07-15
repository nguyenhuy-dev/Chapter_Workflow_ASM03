using System.Collections.Concurrent;

namespace MangaWorkflow.APIGrpcService.HuyNQ.Commons;

/// <summary>
/// In-memory implementation of <see cref="ITokenBlacklistService"/>.
/// Revoked entries are removed lazily once their expiry has passed, so the
/// store never grows beyond the set of currently-valid tokens.
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _revoked = new();

    public void Revoke(string jti, DateTime expiresUtc)
    {
        if (string.IsNullOrEmpty(jti)) return;

        _revoked[jti] = expiresUtc;
    }

    public bool IsRevoked(string jti)
    {
        if (string.IsNullOrEmpty(jti)) return false;

        if (!_revoked.TryGetValue(jti, out var expiresUtc))
            return false;

        // Token already expired on its own: drop it and treat as not revoked.
        if (expiresUtc <= DateTime.UtcNow)
        {
            _revoked.TryRemove(jti, out _);
            return false;
        }

        return true;
    }
}
