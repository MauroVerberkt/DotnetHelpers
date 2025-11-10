# BusinessRules.ResultExtensions

## Overview

**BusinessRules.ResultExtensions** is an integration library that bridges the **BusinessRules** framework with the **Result** monad pattern from **HelperMonads**. This library provides extension methods that enable functional error handling with business rules, allowing you to work with business rule validations in a more composable, declarative way without relying on exception-based control flow.

## Features

- **Seamless Integration**: Combine business rules with Result monad for functional error handling
- **Exception Avoidance**: Handle validation failures without throwing exceptions
- **Composable Validations**: Chain multiple business rule validations using Result's functional methods
- **Async Support**: Full support for asynchronous operations
- **Type Safety**: Strongly-typed extension methods with generic type parameters
- **Flexible API**: Multiple overloads for different use cases

## Use Cases

- **Functional Error Handling**: Replace exception-based validation with Result-based validation
- **Validation Pipelines**: Chain multiple business rule validations in a functional pipeline
- **API Controllers**: Return Result types from controllers for consistent error responses
- **Domain Logic**: Implement domain validation logic without exceptions
- **Railway-Oriented Programming**: Build validation chains that short-circuit on first failure

## Installation

Add references to both **BusinessRules** and **HelperMonads** projects, then add **BusinessRules.ResultExtensions**:

```xml
<ItemGroup>
  <ProjectReference Include="..\BusinessRules\BusinessRules.csproj" />
  <ProjectReference Include="..\HelperMonads\HelperMonads.csproj" />
  <ProjectReference Include="..\BusinessRules.ResultExtensions\BusinessRules.ResultExtensions.csproj" />
</ItemGroup>
```

## Core Extension Methods

### 1. `ToResult<T>` (from BusinessRuleViolationException)

Converts a `BusinessRuleViolationException` to a failed `Result<T>`.

```csharp
public static Result<T> ToResult<T>(this BusinessRuleViolationException exception) where T : notnull
```

**Example:**
```csharp
try
{
    throw UserMustBeAdult.ToException();
}
catch (BusinessRuleViolationException ex)
{
    Result<User> result = ex.ToResult<User>();
    // result.IsFailure == true
}
```

### 2. `ToResult<TRule, TData>` (from BusinessRule)

Creates a failed `Result<TData>` from a `BusinessRule<T>` with a specified error.

```csharp
public static Result<TData> ToResult<TRule, TData>(this BusinessRule<TRule> rule, Exception error)
    where TRule : BusinessRule<TRule>, new()
    where TData : notnull
```

**Example:**
```csharp
var error = new InvalidOperationException("Age validation failed");
Result<int> result = UserMustBeAdult.ToResult<UserMustBeAdult, int>(error);
// result.IsFailure == true
// result.Error is BusinessRuleViolationException with inner exception
```

### 3. `ValidateAndReturn<T>`

Executes an operation and wraps the result in a `Result<T>`, catching any `BusinessRuleViolationException`.

```csharp
public static Result<T> ValidateAndReturn<T>(Func<T> operation) where T : notnull
public static Result<T> ValidateAndReturn<T>(Func<T> operation, BusinessRuleBase rule) where T : notnull
```

**Example:**
```csharp
// Without rule context
Result<User> result = BusinessRuleResultExtensions.ValidateAndReturn(() =>
{
    if (age < 18)
        throw UserMustBeAdult.ToException();
    
    return new User { Age = age };
});

// With rule context (for documentation)
Result<User> result = BusinessRuleResultExtensions.ValidateAndReturn(
    () => CreateUser(username, age),
    UserMustBeAdult);
```

### 4. `ValidateAndReturnAsync<T>`

Async version of `ValidateAndReturn`.

```csharp
public static async Task<Result<T>> ValidateAndReturnAsync<T>(Func<Task<T>> operation) where T : notnull
public static async Task<Result<T>> ValidateAndReturnAsync<T>(Func<Task<T>> operation, BusinessRuleBase rule) where T : notnull
```

**Example:**
```csharp
Result<User> result = await BusinessRuleResultExtensions.ValidateAndReturnAsync(async () =>
{
    var user = await LoadUserAsync(userId);
    if (!user.IsActive)
        throw UserMustBeActive.ToException();
    
    return user;
});
```

### 5. `EnsureBusinessRule<T>`

Validates a value against a predicate and returns a `Result<T>`.

```csharp
public static Result<T> EnsureBusinessRule<T>(
    this T value,
    Func<T, bool> predicate,
    BusinessRuleBase rule) where T : notnull

public static Result<T> EnsureBusinessRule<T>(
    this T value,
    Func<T, bool> predicate,
    BusinessRuleBase rule,
    string errorMessage) where T : notnull
```

**Example:**
```csharp
// Basic usage
Result<int> result = age.EnsureBusinessRule(
    a => a >= 18,
    UserMustBeAdult);

// With custom error message
Result<string> result = password.EnsureBusinessRule(
    p => p.Length >= 8,
    PasswordMinLength,
    $"Password has only {password.Length} characters");

// Chaining with Result.Map
Result<User> userResult = age
    .EnsureBusinessRule(a => a >= 18, UserMustBeAdult)
    .Map(validAge => new User { Age = validAge });
```

### 6. `ValidateAll<T>`

Validates a value against multiple business rules, short-circuiting on first failure.

```csharp
public static Result<T> ValidateAll<T>(
    this T value,
    params (Func<T, bool> predicate, BusinessRuleBase rule)[] validations) where T : notnull

public static Result<T> ValidateAll<T>(
    this T value,
    params (Func<T, bool> predicate, BusinessRuleBase rule, string errorMessage)[] validations) where T : notnull
```

**Example:**
```csharp
// Basic usage
Result<string> result = password.ValidateAll(
    (p => p.Length >= 8, PasswordMinLength),
    (p => p.Any(char.IsUpper), PasswordMustContainUppercase),
    (p => p.Any(char.IsDigit), PasswordMustContainNumber)
);

// With custom error messages
Result<User> result = user.ValidateAll(
    (u => u.Age >= 18, UserMustBeAdult, $"User is only {user.Age} years old"),
    (u => !string.IsNullOrEmpty(u.Email), UserMustHaveEmail, "Email is required"),
    (u => u.IsActive, UserMustBeActive, "User account is inactive")
);
```

### 7. `ToBusinessRuleException<T>`

Converts a failed `Result<T>` back to a `BusinessRuleViolationException`.

```csharp
public static BusinessRuleViolationException ToBusinessRuleException<T>(
    this Result<T> result,
    BusinessRuleBase rule) where T : notnull
```

**Example:**
```csharp
Result<User> result = ValidateUser(user);

if (result.IsFailure)
{
    BusinessRuleViolationException ex = result.ToBusinessRuleException(UserMustBeValid);
    throw ex;
}
```

## Complete Usage Examples

### Example 1: Simple Validation Chain

```csharp
using BusinessRules.ResultExtensions;
using BusinessRules.Rules.UserValidation;
using HelperMonads.Result;

public class UserService
{
    public Result<User> CreateUser(string username, int age, string password)
    {
        return ValidateAge(age)
            .BindAndTransform(validAge => ValidatePassword(password))
            .Map(validPassword => new User 
            { 
                Username = username, 
                Age = age, 
                Password = validPassword 
            });
    }

    private Result<int> ValidateAge(int age)
    {
        return age.EnsureBusinessRule(
            a => a >= 18,
            UserMustBeAdult,
            $"Age {age} is below minimum requirement of 18");
    }

    private Result<string> ValidatePassword(string password)
    {
        return password.ValidateAll(
            (p => p.Length >= 8, PasswordMinLength, "Password is too short"),
            (p => p.Any(char.IsUpper), PasswordMustContainUppercase, "Password must have uppercase letters"),
            (p => p.Any(char.IsDigit), PasswordMustContainNumber, "Password must contain numbers")
        );
    }
}
```

### Example 2: Async Validation Pipeline

```csharp
public class UserService
{
    private readonly IUserRepository _repository;

    public async Task<Result<User>> RegisterUserAsync(UserDto dto)
    {
        var validationResult = await ValidateUserDataAsync(dto);
        
        return await validationResult
            .BindAndTransformAsync(async validDto => 
                await CreateUserInDatabaseAsync(validDto))
            .MapAsync(async user => 
                await SendWelcomeEmailAsync(user));
    }

    private async Task<Result<UserDto>> ValidateUserDataAsync(UserDto dto)
    {
        return await BusinessRuleResultExtensions.ValidateAndReturnAsync(async () =>
        {
            // Check if username is taken
            var existingUser = await _repository.FindByUsernameAsync(dto.Username);
            if (existingUser != null)
                throw UsernameMustBeUnique.ToException();

            // Validate age
            if (dto.Age < 18)
                throw UserMustBeAdult.ToException();

            // Validate email
            if (!IsValidEmail(dto.Email))
                throw EmailMustBeValid.ToException();

            return dto;
        });
    }

    private async Task<Result<User>> CreateUserInDatabaseAsync(UserDto dto)
    {
        try
        {
            var user = new User
            {
                Username = dto.Username,
                Age = dto.Age,
                Email = dto.Email
            };

            await _repository.AddAsync(user);
            return Result.Success(user);
        }
        catch (Exception ex)
        {
            return Result.Failure<User>(ex);
        }
    }

    private async Task<User> SendWelcomeEmailAsync(User user)
    {
        // Send email...
        return user;
    }
}
```

### Example 3: Multiple Validations with Different Rules

```csharp
public class OrderService
{
    public Result<Order> CreateOrder(OrderDto orderDto)
    {
        return ValidateOrder(orderDto)
            .BindAndTransform(dto => CreateOrderFromDto(dto));
    }

    private Result<OrderDto> ValidateOrder(OrderDto dto)
    {
        // Validate customer age
        var customerResult = dto.CustomerAge.EnsureBusinessRule(
            age => age >= 18,
            CustomerMustBeAdult);

        if (customerResult.IsFailure)
            return Result.Failure<OrderDto>(customerResult.Error);

        // Validate order amount
        var amountResult = dto.TotalAmount.EnsureBusinessRule(
            amount => amount > 0,
            OrderAmountMustBePositive);

        if (amountResult.IsFailure)
            return Result.Failure<OrderDto>(amountResult.Error);

        // Validate all items
        foreach (var item in dto.Items)
        {
            var itemResult = item.ValidateAll(
                (i => i.Quantity > 0, ItemQuantityMustBePositive),
                (i => !string.IsNullOrEmpty(i.ProductCode), ItemMustHaveProductCode),
                (i => i.Price >= 0, ItemPriceMustBeNonNegative)
            );

            if (itemResult.IsFailure)
                return Result.Failure<OrderDto>(itemResult.Error);
        }

        return Result.Success(dto);
    }

    private Result<Order> CreateOrderFromDto(OrderDto dto)
    {
        // Create order...
        return Result.Success(new Order());
    }
}
```

### Example 4: Web API Controller Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(
            request.Username,
            request.Age,
            request.Password);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new 
            { 
                Error = result.Error.Message,
                RuleKey = (result.Error as BusinessRuleViolationException)?.Key,
                RuleDescription = (result.Error as BusinessRuleViolationException)?.Rule
            });
    }
}
```

## Benefits

1. **No Exception Throwing**: Avoid using exceptions for control flow
2. **Composable**: Chain validations using Result's functional methods (Map, Bind, etc.)
3. **Railway-Oriented**: Failed validations automatically short-circuit the pipeline
4. **Type-Safe**: Full IntelliSense support and compile-time checking
5. **Async-First**: Full support for async/await patterns
6. **Clear Intent**: Code clearly expresses validation logic and error handling
7. **Testable**: Easy to test without try/catch blocks

## Comparison: Exception-Based vs Result-Based

### Exception-Based Approach
```csharp
public User CreateUser(string username, int age)
{
    try
    {
        if (age < 18)
            throw UserMustBeAdult.ToException();

        return new User { Username = username, Age = age };
    }
    catch (BusinessRuleViolationException ex)
    {
        // Handle error
        LogError(ex);
        throw;
    }
}
```

### Result-Based Approach
```csharp
public Result<User> CreateUser(string username, int age)
{
    return age
        .EnsureBusinessRule(a => a >= 18, UserMustBeAdult)
        .Map(validAge => new User { Username = username, Age = validAge })
        .OnFailure(error => LogError(error));
}
```

## Best Practices

1. **Use ValidateAndReturn for Exception-Throwing Code**: Wrap legacy or third-party code that throws exceptions
2. **Use EnsureBusinessRule for Simple Checks**: Single predicate validations
3. **Use ValidateAll for Multiple Rules**: When you need to validate multiple rules on the same value
4. **Chain with Result Methods**: Leverage Map, Bind, BindAndTransform for pipelines
5. **Keep Predicates Pure**: Ensure validation predicates have no side effects
6. **Provide Custom Messages**: Use overloads with custom error messages for better user feedback
7. **Async When Needed**: Use async variants when validation involves I/O operations

## See Also

- [BusinessRules Documentation](../BusinessRules/BusinessRules-Documentation.md)
- [Result Monad Documentation](../HelperMonads/Result/Result-Documentation.md)
