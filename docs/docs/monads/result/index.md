---
sidebar_position: 1
title: Result Monad
description: Represent operation outcomes with explicit success/failure states using Result<T>
keywords:
  - result
  - monad
  - error handling
  - functional programming
  - railway oriented programming
  - map
  - bind
---

import ResultDemo from '@site/src/components/ResultDemo';

# Result&lt;T&gt; Monad

The `Result<TData>` class represents the outcome of an operation, encapsulating both success and failure states along with optional data or error information. This approach provides explicit handling of operation results with better control over error handling, transformation, and asynchronous workflows.

<ResultDemo title="Interactive Result Example" />

## Features

The `Result<TData>` class provides:

- **Success and Failure States**: Indicates if the operation was successful or failed
- **Data and Error Handling**: Holds data on success or error information (exception) on failure
- **Functional Operations**: Methods for transforming data (`Map`), chaining operations (`Bind`), and performing side effects (`OnSuccess`, `OnFailure`)
- **Asynchronous Support**: Async versions of all transformation and chaining functions

## Use Cases

- **Error Handling**: Cleanly represent and manage error states without throwing exceptions
- **Chaining Operations**: Chain multiple operations together with automatic failure propagation
- **Data Transformation**: Safely transform data on success, or propagate failures
- **Asynchronous Workflows**: Full support for async operations and I/O-bound tasks

## API Summary

| Member | Description |
|--------|-------------|
| `IsSuccess` | Returns `true` if the operation succeeded |
| `IsFailure` | Returns `true` if the operation failed |
| `Data` | The data from a successful operation |
| `Error` | The exception from a failed operation |
| `Map` | Transforms data if successful |
| `MapAsync` | Async version of `Map` |
| `Bind` | Chains with another operation |
| `BindAsync` | Async version of `Bind` |
| `BindAndTransform` | Chains and transforms, passing `Data` to the function |
| `BindAndTransformAsync` | Async version of `BindAndTransform` |
| `OnSuccess` | Executes an action if successful |
| `OnFailure` | Executes an action if failed |
| `Tap` | Executes an action regardless of outcome (useful for logging) |
| `Deconstruct` | Deconstructs into `(IsSuccess, Data, Error)` |

## Basic Usage

```csharp title="BasicResult.cs"
public static Result<int> PerformOperation(bool isSuccess)
{
    if (isSuccess)
        return Result.Success(42);
    else
        return Result.Failure<int>(new Exception("Something went wrong"));
}

// Check the result
var result = PerformOperation(true);
if (result.IsSuccess)
{
    Console.WriteLine($"Operation succeeded with data: {result.Data}");
}
else
{
    Console.WriteLine($"Operation failed with error: {result.Error.Message}");
}
```

:::tip[Null Safety]

The `Result<T>` type uses `[MemberNotNullWhen]` attributes, so the compiler knows:
- When `IsSuccess` is `true`, `Data` is guaranteed non-null
- When `IsFailure` is `true`, `Error` is guaranteed non-null

:::

## Transformation and Chaining

Chain operations together to build pipelines that short-circuit on failure:

```csharp title="AsyncChaining.cs"
public static async Task ExampleAsync()
{
    var result = await PerformOperationAsync(true);

    var transformedResult = await result
        .MapAsync(async data => await Task.FromResult(data * 2))
        .BindAsync(async () => await AnotherAsyncOperation());

    if (transformedResult.IsSuccess)
    {
        Console.WriteLine($"Transformed result: {transformedResult.Data}");
    }
    else
    {
        Console.WriteLine(transformedResult.ToString());
    }
}

public static Task<Result<int>> PerformOperationAsync(bool isSuccess)
{
    if (isSuccess)
        return Task.FromResult(Result.Success(42));
    else
        return Task.FromResult(Result.Failure<int>(new Exception("Async operation failed")));
}

public static async Task<Result<int>> AnotherAsyncOperation()
{
    return await Task.FromResult(Result.Success(100));
}
```

## Real-World Pattern

```csharp title="UserService.cs"
public Result<UserDto> GetUserProfile(int userId)
{
    return GetUser(userId)
        .Map(user => new UserDto
        {
            Name = user.Name,
            Email = user.Email
        })
        .OnSuccess(dto => _cache.Set($"user:{userId}", dto))
        .OnFailure(error => _logger.LogWarning(error, "User {Id} not found", userId));
}
```

:::info

The Result pattern is a powerful tool for managing operation results, improving error handling, and enabling functional approaches to handling success/failure states in C#. It works seamlessly with both synchronous and asynchronous operations.

:::

## See Also

- [Option Monad](../option/index.md) - For values that may or may not exist
- [Business Rules + Result](../../business-rules/result-extensions.md) - Combining with validation
