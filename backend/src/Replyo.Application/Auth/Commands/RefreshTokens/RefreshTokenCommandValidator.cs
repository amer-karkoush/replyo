using FluentValidation;

namespace Replyo.Application.Auth.Commands.RefreshTokens;

/// <summary>
/// Surface-level validation for <see cref="RefreshTokenCommand"/>. The token's existence,
/// expiry, and revocation state are checked in the handler against the database.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(512); // upper bound is a DoS guard; legitimate tokens are ~88 chars (base64 of 64 bytes)
    }
}