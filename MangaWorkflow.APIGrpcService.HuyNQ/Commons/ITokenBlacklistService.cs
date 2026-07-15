namespace MangaWorkflow.APIGrpcService.HuyNQ.Commons;

/// <summary>
/// Keeps track of revoked JWT ids (jti) so that a logged-out token is rejected
/// before its natural expiry. Backed by an in-memory store.
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>Revokes a token by its jti until the given UTC expiry.</summary>
    void Revoke(string jti, DateTime expiresUtc);

    /// <summary>Returns true if the given jti has been revoked and is not yet expired.</summary>
    bool IsRevoked(string jti);
}
