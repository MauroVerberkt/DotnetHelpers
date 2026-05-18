using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace HelperMonads.Result;

/// <summary>
/// Represents the result of an operation, indicating success or failure, along with additional information.
/// </summary>
public class Result<TData> : IEquatable<Result<TData>> where TData : notnull
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [Pure]
    [MemberNotNullWhen(true, nameof(Data))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    [Pure]
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Data))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Data associated with the <see cref="Result{TData}" />.
    /// </summary>
    [Pure]
    public TData? Data { get; }

    /// <summary>
    /// The exception associated with a failed operation.
    /// </summary>
    [Pure]
    public Exception? Error { get; }

    /// <summary>
    /// An exception message for when no data is provided for a successful result.
    /// </summary>
    private const string NoDataProvidedMessage = "Data must be provided for a successful result.";

    /// <summary>
    /// An exception message for when no error is provided for a failed result.
    /// </summary>
    private const string NoErrorProvidedMessage = "Error must be provided for a failed result.";

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TData}" /> class with the specified parameters.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">Data associated with the <see cref="Result{TData}" />.</param>
    /// <param name="error">The exception associated with the failure, if any.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="isSuccess" /> is true and <paramref name="data" /> is null.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    private Result(bool isSuccess, TData? data, Exception? error)
    {
        IsSuccess = isSuccess;

        if (isSuccess)
        {
            ArgumentNullException.ThrowIfNull(data, NoDataProvidedMessage);
            Data = data;
            Error = null; // Ensure Error is null for success
        }
        else
        {
            ArgumentNullException.ThrowIfNull(error, NoErrorProvidedMessage);
            Error = error;
            Data = default; // Ensure Data is default for failure
        }
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Result<TData>? other)
    {
        return other != null &&
               IsSuccess == other.IsSuccess &&
               EqualityComparer<TData>.Default.Equals(Data, other.Data) &&
               EqualityComparer<Exception>.Default.Equals(Error, other.Error);
    }

    /// <summary>
    /// Chains the current <see cref="Result{TData}" /> with another operation,
    /// invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">The function to invoke if the current <see cref="Result{TData}" /> is successful.</param>
    /// <returns>
    /// The <see cref="Result{TData}" /> of the function if the current <see cref="Result{TData}" /> is successful,
    /// otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    [Pure]
    public Result<TData> Bind(Func<Result<TData>> function)
    {
        return IsSuccess ? function() : this;
    }

    /// <summary>
    /// Chains the current <see cref="Result{TData}" /> with another operation, passing through the <see cref="Data" />,
    /// invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The function to invoke, passing through the <see cref="Data" />, if the current <see cref="Result{TData}" /> is
    /// successful.
    /// </param>
    /// <returns>
    /// The <see cref="Result{TData}" /> of the function if the current <see cref="Result{TData}" /> is successful,
    /// otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    [Pure]
    public Result<TData> BindWithData(Func<TData, Result<TData>> function)
    {
        return IsSuccess ? function(Data) : this;
    }

    /// <summary>
    /// Chains and transforms the current <see cref="Result{TData}" /> with another operation, passing through the
    /// <see cref="Data" />,
    /// invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="function">
    /// The function to invoke, which receives the current data and produces a new <see cref="Result{TNewData}" />.
    /// </param>
    /// <returns>
    /// A new <see cref="Result{TNewData}" /> containing transformed data if the current <see cref="Result{TData}" /> is
    /// successful, or a failure <see cref="Result{TNewData}" /> if the current operation was unsuccessful.
    /// </returns>
    [Pure]
    public Result<TNewData> BindAndTransform<TNewData>(Func<TData, Result<TNewData>> function)
        where TNewData : notnull
    {
        return IsSuccess ? function(Data) : Result<TNewData>.Failure(Error);
    }

    /// <summary>
    /// Chains and transforms the current <see cref="Result{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="Result{TData}" /> is successful.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is the
    /// <see cref="Result{TData}" /> of the provided function
    /// if the current <see cref="Result{TData}" /> is successful, otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    public async Task<Result<TNewData>> BindAndTransformAsync<TNewData>(Func<TData, Task<Result<TNewData>>> function)
        where TNewData : notnull
    {
        return IsSuccess ? await function(Data) : Result<TNewData>.Failure(Error);
    }

    /// <summary>
    /// Chains and transforms the current <see cref="Result{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="Result{TData}" /> is successful, accepting a <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is the
    /// <see cref="Result{TData}" /> of the provided function
    /// if the current <see cref="Result{TData}" /> is successful, otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    public async Task<Result<TNewData>> BindAndTransformAsync<TNewData>(
        Func<TData, CancellationToken, Task<Result<TNewData>>> function, CancellationToken cancellationToken)
        where TNewData : notnull
    {
        return IsSuccess
            ? await function(Data, cancellationToken)
            : Result<TNewData>.Failure(Error);
    }

    /// <summary>
    /// Transforms the data of the current <see cref="Result{TData}" /> if the operation was successful.
    /// If the operation failed, the same failure <see cref="Result{TData}" /> is returned.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="transform">The function that transforms the current <see cref="Result{TData}" />'s data.</param>
    /// <returns>
    /// A new <see cref="Result{TNewData}" /> instance, either with transformed data if the current
    /// <see cref="Result{TData}" /> is successful,
    /// or the same failure <see cref="Result{TData}" /> if the current <see cref="Result{TData}" /> is unsuccessful.
    /// </returns>
    [Pure]
    public Result<TNewData> Map<TNewData>(Func<TData, TNewData> transform) where TNewData : notnull
    {
        if (IsFailure) return Result<TNewData>.Failure(Error);
        var newData = transform(Data);
        return Result<TNewData>.Success(newData);
    }

    /// <summary>
    /// Transforms the data of the current <see cref="Result{TData}" /> asynchronously if the operation was successful.
    /// If the operation failed, the same failure <see cref="Result{TData}" /> is returned.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="transform">The asynchronous function that transforms the current <see cref="Result{TData}" />'s data.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is a new
    /// <see cref="Result{TNewData}" /> instance,
    /// either with transformed data if the current <see cref="Result{TData}" /> is successful, or the same failure
    /// <see cref="Result{TData}" /> if the current <see cref="Result{TData}" /> is unsuccessful.
    /// </returns>
    [Pure]
    public async Task<Result<TNewData>> MapAsync<TNewData>(Func<TData, Task<TNewData>> transform)
        where TNewData : notnull
    {
        if (IsFailure) return Result<TNewData>.Failure(Error);
        var newData = await transform(Data);
        return Result<TNewData>.Success(newData);
    }

    /// <summary>
    /// Transforms the data of the current <see cref="Result{TData}" /> asynchronously if the operation was successful.
    /// If the operation failed, the same failure <see cref="Result{TData}" /> is returned.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new data to transform to.</typeparam>
    /// <param name="transform">
    /// The asynchronous function that transforms the current <see cref="Result{TData}" />'s data,
    /// accepting a <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is a new
    /// <see cref="Result{TNewData}" /> instance,
    /// either with transformed data if the current <see cref="Result{TData}" /> is successful, or the same failure
    /// <see cref="Result{TData}" /> if the current <see cref="Result{TData}" /> is unsuccessful.
    /// </returns>
    [Pure]
    public async Task<Result<TNewData>> MapAsync<TNewData>(
        Func<TData, CancellationToken, Task<TNewData>> transform, CancellationToken cancellationToken)
        where TNewData : notnull
    {
        if (IsFailure) return Result<TNewData>.Failure(Error);
        var newData = await transform(Data, cancellationToken);
        return Result<TNewData>.Success(newData);
    }

    /// <summary>
    /// Executes an action only if the <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result.</returns>
    public Result<TData> OnSuccess(Action<TData> action)
    {
        if (IsSuccess)
            action(Data);

        return this;
    }

    /// <summary>
    /// Executes an action only if the <see cref="Result{TData}" /> has failed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This result.</returns>
    public Result<TData> OnFailure(Action<Exception> action)
    {
        if (IsFailure)
            action(Error);

        return this;
    }

    /// <summary>
    /// Executes an action regardless of whether the <see cref="Result{TData}" /> is successful or failed.
    /// Useful for logging, tracing, or other side effects that should occur in both cases.
    /// </summary>
    /// <param name="action">The action to execute, receiving this result.</param>
    /// <returns>This result.</returns>
    public Result<TData> Tap(Action<Result<TData>> action)
    {
        action(this);
        return this;
    }

    /// <summary>
    /// Deconstructs the current <see cref="Result{TData}" /> instance into its individual components:
    /// success status, associated data, and any error that occurred.
    /// </summary>
    /// <param name="isSuccess">A boolean indicating whether the operation was successful.</param>
    /// <param name="data">The data associated with the <see cref="Result{TData}" />.</param>
    /// <param name="error">The exception associated with the failure.</param>
    [Pure]
    public void Deconstruct(out bool isSuccess, out TData? data, out Exception? error)
    {
        isSuccess = IsSuccess;
        data = Data;
        error = Error;
    }

    /// <summary>
    /// Chains the current <see cref="Result{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="Result{TData}" /> is successful.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is the
    /// <see cref="Result{TData}" /> of the provided function
    /// if the current <see cref="Result{TData}" /> is successful, otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    [Pure]
    public async Task<Result<TData>> BindWithDataAsync(Func<TData, Task<Result<TData>>> function)
    {
        return IsSuccess ? await function(Data) : this;
    }

    /// <summary>
    /// Chains the current <see cref="Result{TData}" /> with an asynchronous operation, passing through the
    /// <see cref="Data" />, invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke, passing through the <see cref="Data" />, if the current
    /// <see cref="Result{TData}" /> is successful, accepting a <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is the
    /// <see cref="Result{TData}" /> of the provided function
    /// if the current <see cref="Result{TData}" /> is successful, otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    [Pure]
    public async Task<Result<TData>> BindWithDataAsync(
        Func<TData, CancellationToken, Task<Result<TData>>> function, CancellationToken cancellationToken)
    {
        return IsSuccess ? await function(Data, cancellationToken) : this;
    }

    /// <summary>
    /// Chains the current <see cref="Result{TData}" /> with an asynchronous operation,
    /// invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke if the current <see cref="Result{TData}" /> is successful.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is the
    /// <see cref="Result{TData}" /> of the provided function
    /// if the current <see cref="Result{TData}" /> is successful, otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    [Pure]
    public async Task<Result<TData>> BindAsync(Func<Task<Result<TData>>> function)
    {
        return IsSuccess ? await function() : this;
    }

    /// <summary>
    /// Chains the current <see cref="Result{TData}" /> with an asynchronous operation,
    /// invoking the provided function if the current <see cref="Result{TData}" /> is successful.
    /// </summary>
    /// <param name="function">
    /// The asynchronous function to invoke if the current <see cref="Result{TData}" /> is successful, accepting a
    /// <see cref="CancellationToken" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task <see cref="Result{TData}" /> is the
    /// <see cref="Result{TData}" /> of the provided function
    /// if the current <see cref="Result{TData}" /> is successful, otherwise the current <see cref="Result{TData}" />.
    /// </returns>
    [Pure]
    public async Task<Result<TData>> BindAsync(
        Func<CancellationToken, Task<Result<TData>>> function, CancellationToken cancellationToken)
    {
        return IsSuccess ? await function(cancellationToken) : this;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        if (obj is Result<TData> other)
            return Equals(other);
        return false;
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(IsSuccess, Data, Error);
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        return IsSuccess ? $"Success: {Data}" : $"Failure: {Error.Message}";
    }

    /// <summary>
    /// Creates a successful <see cref="Result{TData}" /> instance.
    /// </summary>
    /// <param name="data">Data associated with the success <see cref="Result{TData}" />.</param>
    /// <returns>A successful <see cref="Result{TData}" /> instance.</returns>
    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static Result<TData> Success(TData data)
    {
        return new Result<TData>(true, data, null);
    }

    /// <summary>
    /// Creates a failed <see cref="Result{TData}" /> instance with data but no exception.
    /// </summary>
    /// <param name="error">The exception associated with the failure <see cref="Result{TData}" />.</param>
    /// <returns>A failed <see cref="Result{TData}" /> instance.</returns>
    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static Result<TData> Failure(Exception error)
    {
        return new Result<TData>(false, default, error);
    }
}

/// <summary>
/// Non-generic factory class for <see cref="Result{TData}" />>
/// </summary>
public static class Result
{
    /// <inheritdoc cref="Result{TData}.Success" />
    [Pure]
    public static Result<TData> Success<TData>(TData data) where TData : notnull
    {
        return Result<TData>.Success(data);
    }

    /// <inheritdoc cref="Result{TData}.Failure" />
    [Pure]
    public static Result<TData> Failure<TData>(Exception error) where TData : notnull
    {
        return Result<TData>.Failure(error);
    }
}