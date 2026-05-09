namespace Replyo.Application.Auth.Commands.Login;

/// <summary>
/// Authenticates an existing user by email and password.
/// </summary>
/// <param name="Email">The user's email. Compared against the lowercase-normalized stored value.</param>
/// <param name="Password">Plaintext password. Verified against the stored hash; never logged.</param>
/// <param name="CreatedByIp">Request IP, recorded on the issued refresh token. Null for non-HTTP entry points.</param>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? CreatedByIp);