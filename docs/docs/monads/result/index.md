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
  - cancellation token
  - pattern matching
---

import ResultDemo from '@site/src/components/ResultDemo';

# Result&lt;T&gt; Monad

The `Result<TData>` class represents the outcome of an operation, encapsulating both success and failure states along with optional data or error information. This approach provides explicit handling of operation results with better control over error handling, transformation, and asynchronous workflows.

<ResultDemo title="Interactive Result Example" />

## Features

The `Result<TData>` class provides:

- **Success and Failure States**: Indicates if the operation was successful or failed
- **Data and Error Handling**: Holds data on success or error information on failure
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
| `Error` | The error from a failed operation |
| `Map` | Transforms data if successful |
| `MapAsync` | Async version of `Map` (with `CancellationToken` overload) |
| `Bind` | Chains with another operation (doesn't pass data) |
| `BindAsync` | Async version of `Bind` (with `CancellationToken` overload) |
| `BindWithData` | Chains with another operation, passing current data (same return type) |
| `BindWithDataAsync` | Async version of `BindWithData` (with `CancellationToken` overload) |
| `BindAndTransform` | Chains and transforms, passing `Data` to the function (different return type) |
| `BindAndTransformAsync` | Async version of `BindAndTransform` (with `CancellationToken` overload) |
| `OnSuccess` | Executes an action if successful |
| `OnFailure` | Executes an action if failed |
| `Tap` | Executes an action regardless of outcome (useful for logging) |
| `Deconstruct` | Deconstructs into `(IsSuccess, Data, Error)` for pattern matching |

## Basic Usage

```csharp title="BasicResult.cs"
public static Result<int> PerformOperation(bool isSuccess)
{
    if (isSuccess)
        return Result.Success(42);
    else
        return Result.Failure<int>(Error.Create("Something went wrong"));
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
        return Task.FromResult(Result.Failure<int>(Error.Create("Async operation failed")));
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
        .OnFailure(error => _logger.LogWarning("User {Id} not found: {Error}", userId, error.Message));
}
```

## Tap — Side Effects Regardless of Outcome

`Tap` executes an action on the result regardless of success or failure, then returns the result unchanged. Useful for logging, metrics, or tracing in the middle of a pipeline:

```csharp title="TapExample.cs"
public Result<Order> ProcessOrder(OrderRequest request)
{
    return ValidateOrder(request)
        .Tap(r => _logger.LogInformation("Validation result: {Success}", r.IsSuccess))
        .BindWithData(order => SaveOrder(order))
        .Tap(r => _metrics.RecordOrderAttempt(r.IsSuccess));
}
```

## Deconstruct — Pattern Matching

`Deconstruct` enables C# deconstruction syntax, letting you extract all components in a single assignment:

```csharp title="DeconstructExample.cs"
var (success, data, error) = GetUser(userId);

if (success)
{
    Console.WriteLine($"Found user: {data!.Name}");
}
else
{
    Console.WriteLine($"Failed: {error!.Message}");
}
```

## BindWithData — Chaining with Access to Current Data

`BindWithData` is like `Bind`, but passes the current `Data` to the next operation. Use it when the next operation needs the current value and returns the **same type**:

```csharp title="BindWithDataExample.cs"
public Result<Order> FulfillOrder(int orderId)
{
    return GetOrder(orderId)
        .BindWithData(order => ValidateInventory(order))   // receives Order, returns Result<Order>
        .BindWithData(order => ApplyDiscount(order))       // same — still Result<Order>
        .BindWithData(order => ChargePayment(order));      // same type throughout
}

// Compare with Bind (no data passed — for independent operations):
// .Bind(() => NotifyWarehouse())    // doesn't need the order data

// Compare with BindAndTransform (data passed — for type changes):
// .BindAndTransform(order => CreateShipment(order))  // Order → Result<Shipment>
```

## CancellationToken Support

All async operations have overloads that accept a `CancellationToken`, enabling proper cancellation propagation through result chains:

```csharp title="CancellationTokenExample.cs"
public async Task<Result<OrderConfirmation>> ProcessOrderAsync(
    OrderRequest request, CancellationToken ct)
{
    return await ValidateOrderAsync(request, ct)
        .BindWithDataAsync(
            async (order, token) => await SaveOrderAsync(order, token), ct)
        .BindAndTransformAsync(
            async (order, token) => await ConfirmOrderAsync(order, token), ct);
}
```

Available `CancellationToken` overloads:
- `MapAsync(Func<TData, CancellationToken, Task<TNewData>>, CancellationToken)`
- `BindAsync(Func<CancellationToken, Task<Result<TData>>>, CancellationToken)`
- `BindWithDataAsync(Func<TData, CancellationToken, Task<Result<TData>>>, CancellationToken)`
- `BindAndTransformAsync(Func<TData, CancellationToken, Task<Result<TNewData>>>, CancellationToken)`

:::info

The Result pattern is a powerful tool for managing operation results, improving error handling, and enabling functional approaches to handling success/failure states in C#. It works seamlessly with both synchronous and asynchronous operations.

:::

## See Also

- [Option Monad](../option/index.md) - For values that may or may not exist
- [Business Rules + Result](../../business-rules/result-extensions.md) - Combining with validation
