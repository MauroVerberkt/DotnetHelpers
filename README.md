# DotnetHelpers

Functional patterns for .NET: **Result**, **Option**, and **Business Rules**.

## Libraries

| Package | Description |
|---------|-------------|
| **HelperMonads** | `Result<T>` and `Option<T>` monadic types for safer C# |
| **BusinessRulesManagement** | Code generation-based business rule framework with Roslyn analyzers |
| **BusinessRules.ResultExtensions** | Bridge between BusinessRules and Result pattern |
| **BusinessRules.Wcf** | WCF fault exception support for business rules |

## Quick Example

```csharp
// Result pattern - explicit error handling without exceptions
public Result<User> GetUser(int id)
{
    var user = _repository.Find(id);
    return user is not null
        ? Result.Success(user)
        : Result.Failure<User>(new UserNotFoundException(id));
}

// Chain operations safely
var result = GetUser(42)
    .Map(user => user.Email)
    .OnFailure(error => _logger.LogError(error, "Failed"));
```

## Documentation

The full documentation site is built with [Docusaurus](https://docusaurus.io/).

```bash
cd docs
npm install
npm run start
```

Then open [http://localhost:3000](http://localhost:3000).

## Requirements

- .NET 8.0+
- C# 12+
