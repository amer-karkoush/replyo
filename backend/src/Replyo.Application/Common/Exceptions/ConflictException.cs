namespace Replyo.Application.Common.Exceptions;

/// <summary>
/// Thrown when a command would violate a uniqueness or state-conflict rule the database
/// enforces but the validator can't (e.g., duplicate email, slug already taken). Mapped
/// to HTTP 409 by the API layer.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}