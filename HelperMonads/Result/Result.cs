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
public class Result<TData> : IResult<TData> where TData : notnull
{
    /// <inheritdoc />
    [Pure]
    [MemberNotNullWhen(true, nameof(Data))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <inheritdoc />
    [Pure]
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Data))]
    public bool IsFailure => !IsSuccess;

    /// <inheritdoc />
    [Pure]
    public TData? Data { get; }

    /// <inheritdoc />
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
    public bool Equals(IResult<TData>? other)
    {
        return other != null &&
               IsSuccess == other.IsSuccess &&
               EqualityComparer<TData>.Default.Equals(Data, other.Data) &&
               EqualityComparer<Exception>.Default.Equals(Error, other.Error);
    }

    /// <inheritdoc />
    [Pure]
    public IResult<TData> Bind(Func<IResult<TData>> function)
    {
        return IsSuccess ? function() : this;
    }

    /// <inheritdoc />
    [Pure]
    public IResult<TData> BindWithData(Func<TData, IResult<TData>> function)
    {
        return IsSuccess ? function(Data) : this;
    }

    /// <inheritdoc />
    [Pure]
    public IResult<TNewData> BindAndTransform<TNewData>(Func<TData, IResult<TNewData>> function)
        where TNewData : notnull
    {
        return IsSuccess ? function(Data) : Result<TNewData>.Failure(Error);
    }

    /// <inheritdoc />
    public async Task<IResult<TNewData>> BindAndTransformAsync<TNewData>(Func<TData, Task<IResult<TNewData>>> function)
        where TNewData : notnull
    {
        return IsSuccess ? await function(Data) : Result<TNewData>.Failure(Error);
    }

    /// <inheritdoc />
    public async Task<IResult<TNewData>> BindAndTransformAsync<TNewData>(
        Func<TData, CancellationToken, Task<IResult<TNewData>>> function, CancellationToken cancellationToken)
        where TNewData : notnull
    {
        return IsSuccess
            ? await function(Data, cancellationToken)
            : Result<TNewData>.Failure(Error);
    }

    /// <inheritdoc />
    [Pure]
    public IResult<TNewData> Map<TNewData>(Func<TData, TNewData> transform) where TNewData : notnull
    {
        if (IsFailure) return Result<TNewData>.Failure(Error);
        var newData = transform(Data);
        return Result<TNewData>.Success(newData);
    }

    /// <inheritdoc />
    [Pure]
    public async Task<IResult<TNewData>> MapAsync<TNewData>(Func<TData, Task<TNewData>> transform)
        where TNewData : notnull
    {
        if (IsFailure) return Result<TNewData>.Failure(Error);
        var newData = await transform(Data);
        return Result<TNewData>.Success(newData);
    }

    /// <inheritdoc />
    [Pure]
    public async Task<IResult<TNewData>> MapAsync<TNewData>(
        Func<TData, CancellationToken, Task<TNewData>> transform, CancellationToken cancellationToken)
        where TNewData : notnull
    {
        if (IsFailure) return Result<TNewData>.Failure(Error);
        var newData = await transform(Data, cancellationToken);
        return Result<TNewData>.Success(newData);
    }

    /// <inheritdoc />
    public IResult<TData> OnSuccess(Action<TData> action)
    {
        if (IsSuccess)
            action(Data);

        return this;
    }

    /// <inheritdoc />
    public IResult<TData> OnFailure(Action<Exception> action)
    {
        if (IsFailure)
            action(Error);

        return this;
    }

    /// <inheritdoc />
    [Pure]
    public void Deconstruct(out bool isSuccess, out TData? data, out Exception? error)
    {
        isSuccess = IsSuccess;
        data = Data;
        error = Error;
    }

    /// <inheritdoc />
    [Pure]
    public async Task<IResult<TData>> BindWithDataAsync(Func<TData, Task<IResult<TData>>> function)
    {
        return IsSuccess ? await function(Data) : this;
    }

    /// <inheritdoc />
    [Pure]
    public async Task<IResult<TData>> BindWithDataAsync(
        Func<TData, CancellationToken, Task<IResult<TData>>> function, CancellationToken cancellationToken)
    {
        return IsSuccess ? await function(Data, cancellationToken) : this;
    }

    /// <inheritdoc />
    [Pure]
    public async Task<IResult<TData>> BindAsync(Func<Task<IResult<TData>>> function)
    {
        return IsSuccess ? await function() : this;
    }

    /// <inheritdoc />
    [Pure]
    public async Task<IResult<TData>> BindAsync(
        Func<CancellationToken, Task<IResult<TData>>> function, CancellationToken cancellationToken)
    {
        return IsSuccess ? await function(cancellationToken) : this;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        if (obj is IResult<TData> other)
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