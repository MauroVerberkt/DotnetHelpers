# Option Monad

This repository contains a C# implementation of the **Option Monad**, a functional programming concept used to represent
a value that may or may not be present. The implementation is designed to handle cases where a value might be missing
without relying on `null`, which can lead to runtime errors like `NullReferenceException`. Instead, it provides a safer
way to work with optional values.

## What is an Option Monad?

The **Option Monad** is a construct used to encapsulate a value that may or may not exist. It is typically used to:

- Avoid `null` references.
- Make code safer and more predictable.
- Explicitly handle cases where a value may be missing.

In this implementation, there are two main states:

- **Some**: The option contains a value.
- **None**: The option does not contain a value.

## Components

### `Option<TValue>`

This is the abstract base class for the Option Monad. It has the following key features:

- **HasValue**: Indicates whether the option contains a value.
- **Value**: The value inside the option, accessible only if `HasValue` is true.
- **Match<TResult>**: A method that allows you to apply a function depending on whether the option contains a value or
  not. It takes two functions: one for the case when the option contains a value (`some`), and one for when the option
  is empty (`none`).

### `Some<TValue>`

This class represents an option that contains a value. It is derived from `Option<TValue>` and overrides the `HasValue`
and `Value` properties to reflect that the option has a value.

### `None<TValue>`

This class represents an empty option (i.e., no value is present). It overrides the `HasValue` property to return
`false` and throws an exception if the `Value` property is accessed.

### `OptionIsNoneException` and `OptionNotPresentException`

These exceptions are used to handle invalid operations on options that do not contain a value.

## Use Cases

The Option Monad can be used in scenarios where:

- A function may return a value or may not, and you want to avoid null checks or exceptions.
- You need to handle cases where a value is missing explicitly.
- You prefer using functional constructs to handle optional values in a safer way.

Examples of common use cases:

- Optional parameters that may or may not be provided.
- Results of database queries that may return a null value.
- Any scenario where a value can be optionally present.

## Example Usage

Here’s a simple example of how to use the `Option<TValue>` class:

```csharp
using System;
using HelperMonads.Option;

public class Program
{
    public static void Main()
    {
        // Example 1: Using Some to represent a value
        Option<string> someOption = Option<string>.Some("Hello, world!");

        // Match the option
        string result1 = someOption.Match(
            some: value => $"Value is: {value}",
            none: () => "No value present"
        );

        Console.WriteLine(result1); // Output: Value is: Hello, world!

        // Example 2: Using None to represent an absence of value
        Option<string> noneOption = Option<string>.None;

        // Match the option
        string result2 = noneOption.Match(
            some: value => $"Value is: {value}",
            none: () => "No value present"
        );

        Console.WriteLine(result2); // Output: No value present

        // Example 3: Using FromNullable to create an option from a nullable value
        string? nullableValue1 = null;
        Option<string> optionFromNullable = Option<string>.FromNullable(nullableValue1);

        string result3 = optionFromNullable.Match(
            some: value => $"Value is: {value}",
            none: () => "No value present"
        );

        Console.WriteLine(result3); // Output: No value present
        
        // Example 4: Using implicit conversion to create an option from a nullable value
        string? nullableValue2 = null;
        Option<string> implicitOptionFromNullable =nullableValue;

        string result4 = implicitOptionFromNullable.Match(
            some: value => $"Value is: {value}",
            none: () => "No value present"
        );

        Console.WriteLine(result4); // Output: No value present
    }
}
```

### Explanation of Example:

1. **Using `Some<TValue>`**:
    - `Option<string>.Some("Hello, world!")` creates an option containing the value `"Hello, world!"`.
    - We then use the `Match` method to handle the case where the value is present (`some` case).

2. **Using `None<TValue>`**:
    - `Option<string>.None` represents an option with no value.
    - The `Match` method handles this case with the `none` function, returning a message indicating that no value is
      present.

3. **Using `FromNullable`**:
    - `Option<string>.FromNullable(null)` converts a nullable string (`string?`) into an `Option<string>`. Since the
      value is `null`, it returns `None`, and the `Match` method handles it as "No value present".

4. **Using `Implicit conversion`**:
    - `Implicitly` converts a nullable string (`string?`) into an `Option<string>`.
      Since the value is `null`, it returns `None`, and the `Match` method handles it as "No value present".

## Benefits of Using the Option Monad

- **Null Safety**: It explicitly avoids using `null` values, which helps in avoiding `NullReferenceException`.
- **Cleaner Code**: Using `Option<TValue>` makes your code more readable and self-explanatory by indicating that a value
  might be missing and forcing you to handle this case explicitly.
- **Functional Programming Style**: This implementation leverages functional programming concepts like monads and
  pattern matching, which are useful for handling complex flows with optional values.
