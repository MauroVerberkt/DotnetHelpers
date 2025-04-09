using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace HelperMonads.Result;

/// <summary>
/// Defines the contract for an option that represents the result of an operation, indicating success or failure, along
/// with additional information.
/// </summary>
public interface IResult<TData> : IEquatable<IResult<TData>> where TData : notnull
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Data))]
    [MemberNotNullWhen(false, nameof(Error))]
    bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Data))]
    bool IsFailure { get; }

    /// <summary>
    /// Data associated with the <see cref="IResult{TData}" />.
    /// </summary>
    TData? Data { get; }

    /// <summary>
    /// The exception associated with a failed operation.
    /// </summary>
    Exception? Error { get; }

    /// <summary>
    /// Transforms the data of the current <see cref="IResult{TData}" /> if the operation was successful.
    /// If the operation failed, the same failure <see cref="IResult{TData}" /> is returned.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="transform">The function that transforms the current <see cref="IResult{TData}" />'s data.</param>
    /// <returns>
    /// A new <see cref="IResult{TNewData}" /> instance, either with transformed data if the current
    /// <see cref="IResult{TData}" /> is successful,
    /// or the same failure <see cref="IResult{TData}" /> if the current <see cref="IResult{TData}" /> is unsuccessful.
    /// </returns>
    IResult<TNewData> Map<TNewData>(Func<TData, TNewData> transform) where TNewData : notnull;

    /// <summary>
    /// Transforms the data of the current <see cref="IResult{TData}" /> asynchronously if the operation was successful.
    /// If the operation failed, the same failure <see cref="IResult{TData}" /> is returned.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="transform">The asynchronous function that transforms the current <see cref="IResult{TData}" />'s data.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is a new
    /// <see cref="IResult{TNewData}" /> instance,
    /// either with transformed data if the current <see cref="IResult{TData}" /> is successful, or the same failure
    /// <see cref="IResult{TData}" /> if the current <see cref="IResult{TData}" /> is unsuccessful.
    /// </returns>
    Task<IResult<TNewData>> MapAsync<TNewData>(Func<TData, Task<TNewData>> transform) where TNewData : notnull;

    /// <summary>
    /// Transforms the data of the current <see cref="IResult{TData}" /> asynchronously if the operation was successful.
    /// If the operation failed, the same failure <see cref="IResult{TData}" /> is returned.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="transform">
    /// The asynchronous function that transforms the current <see cref="IResult{TData}" />'s data,
    /// accepting a <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is a new
    /// <see cref="IResult{TNewData}" /> instance,
    /// either with transformed data if the current <see cref="IResult{TData}" /> is successful, or the same failure
    /// <see cref="IResult{TData}" /> if the current <see cref="IResult{TData}" /> is unsuccessful.
    /// </returns>
    Task<IResult<TNewData>> MapAsync<TNewData>(
        Func<TData, CancellationToken, Task<TNewData>> transform, CancellationToken cancellationToken)
        where TNewData : notnull;

    /// <summary>
    /// Chains the current <see cref="IResult{TData}" /> with another operation,
    /// invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">The function to invoke if the current <see cref="IResult{TData}" /> is successful.</param>
    /// <returns>
    /// The <see cref="IResult{TData}" /> of the function if the current <see cref="IResult{TData}" /> is successful,
    /// otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    IResult<TData> Bind(Func<IResult<TData>> function);

    /// <summary>
    /// Chains the current <see cref="IResult{TData}" /> with an asynchronous operation,
    /// invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke if the current <see cref="IResult{TData}" /> is successful.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is the
    /// <see cref="IResult{TData}" /> of the provided function
    /// if the current <see cref="IResult{TData}" /> is successful, otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    Task<IResult<TData>> BindAsync(Func<Task<IResult<TData>>> function);

    /// <summary>
    /// Chains the current <see cref="IResult{TData}" /> with an asynchronous operation,
    /// invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke if the current <see cref="IResult{TData}" /> is successful, accepting a
    /// <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is the
    /// <see cref="IResult{TData}" /> of the provided function
    /// if the current <see cref="IResult{TData}" /> is successful, otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    Task<IResult<TData>> BindAsync(
        Func<CancellationToken, Task<IResult<TData>>> function, CancellationToken cancellationToken);

    /// <summary>
    /// Chains the current <see cref="IResult{TData}" /> with another operation, passing through the <see cref="Data" />,
    /// invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The function to invoke, passing through the <see cref="Data" />, if the current <see cref="IResult{TData}" /> is
    /// successful.
    /// </param>
    /// <returns>
    /// The <see cref="IResult{TData}" /> of the function if the current <see cref="IResult{TData}" /> is successful,
    /// otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    IResult<TData> BindWithData(Func<TData, IResult<TData>> function);

    /// <summary>
    /// Chains and transforms the current <see cref="IResult{TData}" /> with another operation, passing through the
    /// <see cref="Data" />,
    /// invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="function">
    /// The function to invoke, which receives the current data and produces a new <see cref="IResult{TNewData}" />.
    /// </param>
    /// <returns>
    /// A new <see cref="IResult{TNewData}" /> containing transformed data if the current <see cref="IResult{TData}" /> is
    /// successful, or a failure <see cref="IResult{TNewData}" /> if the current operation was unsuccessful.
    /// </returns>
    IResult<TNewData> BindAndTransform<TNewData>(Func<TData, IResult<TNewData>> function) where TNewData : notnull;

    /// <summary>
    /// Chains and transforms the current <see cref="IResult{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="IResult{TData}" /> is successful.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is the
    /// <see cref="IResult{TData}" /> of the provided function
    /// if the current <see cref="IResult{TData}" /> is successful, otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    Task<IResult<TNewData>> BindAndTransformAsync<TNewData>(Func<TData, Task<IResult<TNewData>>> function)
        where TNewData : notnull;

    /// <summary>
    /// Chains and transforms the current <see cref="IResult{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="IResult{TData}" /> is successful, accepting a <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is the
    /// <see cref="IResult{TData}" /> of the provided function
    /// if the current <see cref="IResult{TData}" /> is successful, otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    Task<IResult<TNewData>> BindAndTransformAsync<TNewData>(
        Func<TData, CancellationToken, Task<IResult<TNewData>>> function, CancellationToken cancellationToken)
        where TNewData : notnull;

    /// <summary>
    /// Chains the current <see cref="IResult{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="IResult{TData}" /> is successful.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is the
    /// <see cref="IResult{TData}" /> of the provided function
    /// if the current <see cref="IResult{TData}" /> is successful, otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    Task<IResult<TData>> BindWithDataAsync(Func<TData, Task<IResult<TData>>> function);

    /// <summary>
    /// Chains the current <see cref="IResult{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="IResult{TData}" /> is successful, accepting a <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="IResult{TData}" /> is the
    /// <see cref="IResult{TData}" /> of the provided function
    /// if the current <see cref="IResult{TData}" /> is successful, otherwise the current <see cref="IResult{TData}" />.
    /// </returns>
    Task<IResult<TData>> BindWithDataAsync(
        Func<TData, CancellationToken, Task<IResult<TData>>> function, CancellationToken cancellationToken);

    /// <summary>
    /// Executes an action only if the <see cref="IResult{TData}" /> is successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result.</returns>
    IResult<TData> OnSuccess(Action<TData> action);

    /// <summary>
    /// Executes an action only if the <see cref="IResult{TData}" /> has failed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result.</returns>
    IResult<TData> OnFailure(Action<Exception> action);

    /// <summary>
    /// Deconstructs the current <see cref="IResult{TData}" /> instance into its individual components:
    /// success status, associated data, and any error that occurred.
    /// </summary>
    /// <param name="isSuccess">A boolean indicating whether the operation was successful.</param>
    /// <param name="data">The data associated with the <see cref="IResult{TData}" />.</param>
    /// <param name="error">The exception associated with the failure.</param>
    void Deconstruct(out bool isSuccess, out TData? data, out Exception? error);
}