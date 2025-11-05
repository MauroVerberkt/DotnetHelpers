# Result Monad

## Overview

This repository contains an implementation of the **Result** pattern in C#. The `Result<TData>` class is used to represent the outcome of an operation, encapsulating both success and failure states along with optional data or error information. This approach allows for more explicit handling of operation results, providing better control over error handling, transformation, and asynchronous workflows.

## Features

The `Result<TData>` class provides:

- **Success and Failure States**: Indicates if the operation was successful or failed.
- **Data and Error Handling**: Optionally holds data on success or error information (exception) on failure.
- **Functional Operations**: Provides methods for transforming data (`Map`), chaining operations (`Bind`), and performing actions on success (`IfSuccess`).
- **Asynchronous Support**: Asynchronous versions of transformation and chaining functions are available (`MapAsync`, `BindAsync`).

## Use Cases

- **Error Handling**: Cleanly represent and manage error states without throwing exceptions.
- **Chaining Operations**: Chain multiple operations together, ensuring that failures are propagated automatically without needing to check for success after each operation.
- **Data Transformation**: Safely transform or map data if the operation is successful, or propagate the failure if it wasn't.
- **Asynchronous Workflows**: The implementation supports async operations for handling I/O-bound or long-running tasks.

## Class: `Result<TData>`

The `Result<TData>` class supports the following:

- `IsSuccess`: Returns `true` if the operation was successful.
- `IsFailure`: Returns `true` if the operation failed.
- `Data`: The data associated with the operation if successful.
- `Error`: The exception associated with the failure.
- `Map`: Transforms the data if the operation was successful.
- `MapAsync`: Asynchronously transforms the data if the operation was successful.
- `Bind`: Chains the current result with another operation.
- `BindAsync`: Chains the current result with an asynchronous operation.
- `BindAndTransform`: Chains and transforms the current result, passing through the `Data` and invoking the provided function if the operation is successful.
- `BindAndTransformAsync`: Chains and transforms the current result asynchronously, passing through the `Data` if the operation is successful.
- `OnSuccess`: Executes an action if the operation was successful.
- `OnFailure`: Executes an action if the operation was a failure.
- `Deconstruct`: Deconstructs the result into its components (`IsSuccess`, `Data`, and `Error`).
- **Success Creation**: Use `Result<TData>.Success(data)` to create a successful result.
- **Failure Creation**: Use `Result<TData>.Failure(error)` to represent a failure with an exception.
- **Chaining and Transformation**: Chain operations or transform data using methods like `Bind`, `Map`, etc.
- **Error Representation**: If an operation fails, an exception is captured in the `Error` property.

### Example of Usage

```csharp
using System;

public class Program
{
    public static void Main()
    {
        // Example 1: Successful Operation
        var result1 = PerformOperation(true);
        if (result1.IsSuccess)
        {
            Console.WriteLine($"Operation succeeded with data: {result1.Data}");
        }
        else
        {
            Console.WriteLine($"Operation failed with error: {result1.Error.Message}");
        }

        // Example 2: Failed Operation
        var result2 = PerformOperation(false);
        if (result2.IsSuccess)
        {
            Console.WriteLine($"Operation succeeded with data: {result2.Data}");
        }
        else
        {
            Console.WriteLine($"Operation failed with error: {result2.Error.Message}");
        }
    }

    public static Result<int> PerformOperation(bool isSuccess)
    {
        if (isSuccess)
        {
            return Result.Success(42);
        }
        else
        {
            return Result.Failure<int>(new Exception("Something went wrong"));
        }
    }
}
```

### Explanation

- `PerformOperation` returns a `Result<int>`. If the operation succeeds, it creates a successful result with a data value (42); otherwise, it creates a failure result with an error message.
- The result is checked for success or failure, and appropriate actions are taken based on the result.

### Example of Transformation and Chaining

```csharp
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
    {
        return Task.FromResult(Result.Success(42));
    }
    else
    {
        return Task.FromResult(Result.Failure<int>(new Exception("Async operation failed")));
    }
}

public static async Task<Result<int>> AnotherAsyncOperation()
{
    return await Task.FromResult(Result.Success(100));
}
```

### Explanation

- `PerformOperationAsync` returns an asynchronous result.
- `MapAsync` is used to transform the data (doubling the value).
- `BindAsync` chains another asynchronous operation.
- The result is checked for success, and actions are taken accordingly.

## Conclusion

The `Result<TData>` pattern is a powerful tool for managing operation results, improving error handling, and enabling more functional approaches to handling success/failure states in C#. This implementation makes it easy to manage both synchronous and asynchronous operations, transform data, and chain multiple operations together with minimal boilerplate code.