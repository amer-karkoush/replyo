using Replyo.Application.Common.Abstractions;

namespace Replyo.Application.Tests.TestInfrastructure;

/// <summary>
/// Test double for <see cref="IPasswordHasher"/>. The "hash" is the password prefixed
/// with a marker so test fixtures can spot it; verification is plain string comparison.
/// </summary>
/// <remarks>
/// Real-world insecure (no salt, no work factor) but irrelevant for tests of handlers,
/// which care about control flow around verification outcomes, not about cryptographic
/// strength. Set <see cref="NextVerificationOutcome"/> before a call to control the
/// returned verification result for testing rehash and failure paths.
/// </remarks>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    private const string HashPrefix = "fake-hash:";

    /// <summary>
    /// If set, <see cref="Verify"/> returns this outcome instead of computing the
    /// real (string-comparison) result. Useful for testing rehash and failure paths
    /// without manipulating stored hashes.
    /// </summary>
    public PasswordVerificationOutcome? NextVerificationOutcome { get; set; }

    public string Hash(string password) => HashPrefix + password;

    public PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword)
    {
        if (NextVerificationOutcome.HasValue)
        {
            var forced = NextVerificationOutcome.Value;
            NextVerificationOutcome = null; // single-shot; consumed by the next Verify call
            return forced;
        }

        return hashedPassword == HashPrefix + providedPassword
            ? PasswordVerificationOutcome.Success
            : PasswordVerificationOutcome.Failed;
    }
}