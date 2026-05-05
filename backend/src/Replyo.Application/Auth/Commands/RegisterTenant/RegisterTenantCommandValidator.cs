using FluentValidation;

namespace Replyo.Application.Auth.Commands.RegisterTenant;

/// <summary>
/// Surface-level validation for <see cref="RegisterTenantCommand"/>. Cross-aggregate checks
/// (email uniqueness, slug uniqueness) live in the handler.
/// </summary>
public sealed class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.OwnerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320); // RFC 5321 upper bound

        RuleFor(x => x.OwnerPassword)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(128); // upper bound is a DoS guard against expensive hashing

        RuleFor(x => x.OwnerFullName)
            .NotEmpty()
            .MaximumLength(200);
    }
}