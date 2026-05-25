---
sidebar_position: 3
title: WCF Integration
description: Transmit business rule violations as structured WCF faults across service boundaries
keywords:
  - wcf
  - fault exception
  - service boundary
  - legacy
  - distributed systems
---

# BusinessRules.Wcf

**BusinessRules.Wcf** bridges the BusinessRules framework with WCF (Windows Communication Foundation), enabling business rule violations to be transmitted as structured WCF faults across service boundaries.

:::info[Why a Separate Project?]

The WCF integration was separated to:
1. Keep the core BusinessRules library free of WCF dependencies
2. Make WCF support opt-in (only add if you need it)
3. Ensure new users don't inherit legacy technology dependencies
4. Follow the same extensibility pattern as BusinessRules.ResultExtensions

:::

## Installation

```xml title="Project.csproj"
<ItemGroup>
  <ProjectReference Include="..\BusinessRules\BusinessRules.csproj" />
  <ProjectReference Include="..\BusinessRules.Wcf\BusinessRules.Wcf.csproj" />
</ItemGroup>
```

## Core Types

### BusinessRuleFault

A record type wrapping a `BusinessRuleBase` for WCF fault transmission:

```csharp
[DataContract]
public record BusinessRuleFault(
    [property: DataMember]
    [property: Required]
    BusinessRuleBase BusinessRule);
```

## Extension Methods

### ToFaultException&lt;T&gt;() (Static)

Creates a `FaultException<BusinessRuleFault>` from a business rule type:

```csharp title="StaticFault.cs"
var faultException = BusinessRuleWcfExtensions.ToFaultException<UserMustBeAdult>();
throw faultException;
```

### ToFaultException() (from BusinessRuleBase)

Creates a fault from a rule instance:

```csharp title="InstanceFault.cs"
var rule = new UserMustBeAdult();
var faultException = rule.ToFaultException();
throw faultException;
```

### ToFaultException() (from BusinessRuleViolationException)

Converts an exception to a WCF fault:

```csharp title="ExceptionFault.cs"
try
{
    ValidateUser(user);
}
catch (BusinessRuleViolationException ex)
{
    throw ex.ToFaultException();
}
```

## Complete Examples

### WCF Service

```csharp title="UserService.cs"
[ServiceContract]
public interface IUserService
{
    [OperationContract]
    [FaultContract(typeof(BusinessRuleFault))]
    User CreateUser(string username, int age, string password);
}

public class UserService : IUserService
{
    public User CreateUser(string username, int age, string password)
    {
        try
        {
            if (age < 18)
                throw UserMustBeAdult.ToException();

            if (password.Length < 8)
                throw PasswordMinLength.ToException();

            return new User { Username = username, Age = age };
        }
        catch (BusinessRuleViolationException ex)
        {
            throw ex.ToFaultException();
        }
    }
}
```

### WCF Client

```csharp title="UserServiceClient.cs"
public void CreateUserWithErrorHandling(string username, int age, string password)
{
    try
    {
        var user = _service.CreateUser(username, age, password);
        Console.WriteLine($"User {user.Username} created successfully");
    }
    catch (FaultException<BusinessRuleFault> faultEx)
    {
        var rule = faultEx.Detail.BusinessRule;
        Console.WriteLine($"Business rule violation:");
        Console.WriteLine($"  Key: {rule.Key}");
        Console.WriteLine($"  Requirement: {rule.Requirement}");
        Console.WriteLine($"  Description: {rule.Description}");
        Console.WriteLine($"  Category: {rule.Category}");
    }
}
```

### Custom Fault Handler

```csharp title="BusinessRuleFaultHandler.cs"
public class BusinessRuleFaultHandler : IErrorHandler
{
    public bool HandleError(Exception error)
    {
        if (error is BusinessRuleViolationException brException)
        {
            LogBusinessRuleViolation(brException.BusinessRule);
            return true;
        }
        return false;
    }

    public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
    {
        if (error is BusinessRuleViolationException brException)
        {
            var faultException = brException.ToFaultException();
            var messageFault = faultException.CreateMessageFault();
            fault = Message.CreateMessage(version, messageFault, faultException.Action);
        }
    }
}
```

## Migration from Core BusinessRules

If you previously used `ToFaultException()` from the core library:

```csharp title="Before.cs"
// Old - no longer exists in core BusinessRules
var fault = UserMustBeAdult.ToFaultException();
```

```csharp title="After.cs"
using BusinessRules.Wcf;

// Static method
var fault = BusinessRuleWcfExtensions.ToFaultException<UserMustBeAdult>();

// Or instance extension method
var rule = new UserMustBeAdult();
var fault = rule.ToFaultException();

// Or from exception
var exception = UserMustBeAdult.ToException();
var fault = exception.ToFaultException();
```

:::warning[Important Notes]

1. `BusinessRuleFault` moved from `BusinessRules` to `BusinessRules.Wcf` namespace
2. `ToFaultException` is now an extension method, not a static method on `BusinessRule<T>`
3. Projects using WCF features must explicitly reference `BusinessRules.Wcf`
4. The core `BusinessRules` library remains unchanged for non-WCF scenarios

:::

## See Also

- [BusinessRules Overview](./overview.md)
- [Result Extensions](./result-extensions.md) - Modern alternative using Result pattern
