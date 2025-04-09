using System;
using System.Threading;
using System.Threading.Tasks;

namespace HelperMonads.Option;

/// <summary>
/// Defines the contract for an option that represents a value that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the value, which must be a reference type (class).</typeparam>
public interface IOption<out T>
{
    /// <summary>
    /// Gets a value indicating whether the option contains a value.
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// Gets the value contained within the option.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Applies a function to the value if present, otherwise applies a function for when no value is present.
    /// </summary>
    /// <typeparam name="TResult">The result type of the match function.</typeparam>
    /// <param name="some">The function to apply if the option contains a value.</param>
    /// <param name="none">The function to apply if the option does not contain a value.</param>
    /// <returns>The result of the appropriate function based on the option's value presence.</returns>
    TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none);

    /// <summary>
    /// Applies an asynchronous function to the value if present, otherwise applies an asynchronous function for when no value is
    /// present.
    /// </summary>
    /// <param name="some">The asynchronous function to apply if the option contains a value.</param>
    /// <param name="none">The asynchronous function to apply if the option does not contain a value.</param>
    /// <typeparam name="TResult">The result type of the match function.</typeparam>
    /// <returns>A task representing the result of the appropriate function based on the option's value presence.</returns>
    Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<Task<TResult>> none);

    /// <summary>
    /// Applies an asynchronous function to the value if present, otherwise applies an asynchronous function for when no value is
    /// present.
    /// </summary>
    /// <param name="some">
    /// The asynchronous function to apply if the option contains a value, accepting a
    /// <see cref="CancellationToken" />.
    /// </param>
    /// <param name="none">
    /// The asynchronous function to apply if the option does not contain a value, accepting a
    /// <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <typeparam name="TResult">The result type of the match function.</typeparam>
    /// <returns>A task representing the result of the appropriate function based on the option's value presence.</returns>
    Task<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, Task<TResult>> some, Func<CancellationToken, Task<TResult>> none,
        CancellationToken cancellationToken);
}