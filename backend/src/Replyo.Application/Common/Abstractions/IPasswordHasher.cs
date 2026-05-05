namespace Replyo.Application.Common.Abstractions;

/// <summary>
/// Hashes and verifies user passwords. Implementations must use a memory-hard or
/// iteration-tunable algorithm (PBKDF2, Argon2, bcrypt) and embed algorithm parameters
/// in the hash output to allow forward-compatible rehashing.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Produces a hash for the supplied plaintext password. The returned string is
    /// self-describing — it contains the algorithm, parameters, and salt — and is the
    /// only value that should be persisted.
    /// </summary>
    /// <param name="password">The plaintext password to hash. Must not be null.</param>
    /// <returns>An opaque, self-describing hash string suitable for storage.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against a previously stored hash.
    /// </summary>
    /// <param name="hashedPassword">The stored hash produced by <see cref="Hash"/>.</param>
    /// <param name="providedPassword">The plaintext password supplied by the user.</param>
    /// <returns>
    /// A <see cref="PasswordVerificationOutcome"/> indicating success, failure, or
    /// success-but-needs-rehash (when the stored hash uses outdated parameters and should
    /// be replaced on this login).
    /// </returns>
    PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword);
}

/// <summary>
/// The outcome of verifying a password against a stored hash.
/// </summary>
public enum PasswordVerificationOutcome
{
    Failed = 0,

    Success = 1,

    /// <summary>
    /// The password matched, but the stored hash uses outdated parameters (older iteration count,
    /// older algorithm version). The caller should rehash and update the user's stored hash on this login.
    /// </summary>
    SuccessRehashNeeded = 2,
}