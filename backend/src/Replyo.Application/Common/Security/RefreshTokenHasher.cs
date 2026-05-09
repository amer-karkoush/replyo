using System.Security.Cryptography;
using System.Text;

namespace Replyo.Application.Common.Security;

/// <summary>
/// Hashes refresh token plaintexts for storage and lookup. The plaintext is shown to the
/// client exactly once (at issue time); only the hash is persisted. Lookup happens by
/// hashing the presented plaintext and matching against the stored hash.
/// </summary>
/// <remarks>
/// SHA-256 is the right primitive here — refresh tokens are high-entropy random bytes
/// (not user-chosen passwords), so a slow KDF like PBKDF2 or Argon2 would be wasted work.
/// Every issuer and every verifier in the system must use this helper; producing a hash
/// any other way breaks the cross-component invariant that issued tokens are findable.
/// </remarks>
public static class RefreshTokenHasher
{
    /// <summary>
    /// Returns the lowercase hex SHA-256 of the UTF-8 encoded plaintext. Deterministic;
    /// safe to call repeatedly on the same input.
    /// </summary>
    /// <param name="plaintext">The refresh token plaintext as issued to the client.</param>
    public static string Hash(string plaintext)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}