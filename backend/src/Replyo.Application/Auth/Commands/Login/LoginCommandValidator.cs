using FluentValidation;

namespace Replyo.Application.Auth.Commands.Login;

/// <summary>
/// Surface-level validation for <see cref="LoginCommand"/>. Authentication itself
/// (does this email exist, does the password match) is the handler's job — those
/// checks need database access and produce <see cref="Common.Exceptions.UnauthorizedException"/>.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320); // RFC 5321 upper bound

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128); // upper bound is a DoS guard against expensive hashing
    }
}