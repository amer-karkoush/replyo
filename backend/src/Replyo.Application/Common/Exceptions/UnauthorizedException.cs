namespace Replyo.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request lacks valid authentication or presents credentials that fail
/// verification (wrong password, unknown email, expired/revoked refresh token). Mapped
/// to HTTP 401 by the API layer.
/// </summary>
/// <remarks>
/// Messages should be deliberately vague to avoid leaking whether an email exists, whether
/// a password is wrong vs. the account being locked, etc. The API layer's exception handler
/// returns a fixed "Invalid credentials" body regardless of the message passed here; the
/// message itself is for logs.
/// </remarks>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}