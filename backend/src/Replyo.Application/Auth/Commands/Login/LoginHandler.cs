using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Replyo.Application.Common.Abstractions;
using Replyo.Application.Common.Exceptions;
using Replyo.Domain.Entities;

namespace Replyo.Application.Auth.Commands.Login;

/// <summary>
/// Handles user login: verifies credentials, issues a token pair, transparently rehashes
/// the password if its stored hash uses outdated parameters.
/// </summary>
public interface ILoginHandler : ICommandHandler<LoginCommand, LoginResult>;

internal sealed class LoginHandler : ILoginHandler
{
    private readonly IValidator<LoginCommand> _validator;
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;

    public LoginHandler(
        IValidator<LoginCommand> validator,
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService)
    {
        _validator = validator;
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Generic message regardless of whether the email exists or the password is wrong —
        // prevents enumeration of valid accounts via login error responses.
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        var verification = _passwordHasher.Verify(user.PasswordHash, command.Password);

        if (verification == PasswordVerificationOutcome.Failed)
            throw new UnauthorizedException("Invalid email or password.");

        // Transparent rehash on outdated parameters — see PasswordVerificationOutcome.SuccessRehashNeeded.
        // The rehash happens on a successful login, so the user pays no additional latency.
        if (verification == PasswordVerificationOutcome.SuccessRehashNeeded)
        {
            var newHash = _passwordHasher.Hash(command.Password);
            user.UpdatePasswordHash(newHash);
        }

        user.RecordLogin();

        var tokens = _tokenService.Issue(user, command.CreatedByIp);
        var refreshToken = Domain.Entities.RefreshToken.Issue(
            user.Id,
            tokens.RefreshTokenHash,
            tokens.RefreshTokenExpiresAt,
            command.CreatedByIp);

        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            user.TenantId,
            user.Id,
            tokens.AccessToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt);
    }
}