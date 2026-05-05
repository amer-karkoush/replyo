namespace Replyo.Application.Common.Abstractions;

/// <summary>
/// Handles a single application command, producing a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <remarks>
/// Implementations are responsible for validating their own input — typically by injecting
/// <see cref="FluentValidation.IValidator{T}"/> and invoking it as the first line of the handler.
/// This contract holds regardless of entry point (HTTP endpoint, Hangfire job, SignalR hub):
/// validation, authorization, and domain invariants are the handler's responsibility, not the caller's.
/// </remarks>
/// <typeparam name="TCommand">The command type carrying the inputs for this operation.</typeparam>
/// <typeparam name="TResult">The result type returned on successful completion.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
{
    /// <summary>
    /// Executes the command. Throws <see cref="FluentValidation.ValidationException"/> on invalid
    /// input and domain-specific exceptions on business rule violations.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the operation to complete.</param>
    /// <returns>The result of the command on success.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}