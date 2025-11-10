# BusinessRules.Wcf

## Overview

**BusinessRules.Wcf** is an integration library that bridges the **BusinessRules** framework with **WCF (Windows Communication Foundation)**. This library provides extension methods and types that enable business rule violations to be transmitted as structured WCF faults across service boundaries.

## Why a Separate Project?

The WCF integration was moved to a separate project to:

1. **Remove Legacy Dependencies**: Keep the core BusinessRules library free of WCF dependencies
2. **Optional Integration**: Only applications using WCF need to reference this package
3. **Modern by Default**: New users don't get legacy technology dependencies
4. **Clean Architecture**: Follows the same pattern as BusinessRules.ResultExtensions

## Features

- **FaultException Creation**: Convert business rules to structured WCF fault exceptions
- **Type Safety**: Strongly-typed fault exceptions with generic type parameters
- **Rich Metadata**: Fault exceptions include the full business rule context
- **Multiple Overloads**: Create fault exceptions from rules, exceptions, or rule instances

## Installation

Add references to **BusinessRules** and **BusinessRules.Wcf**:

```xml
<ItemGroup>
  <ProjectReference Include="..\BusinessRules\BusinessRules.csproj" />
  <ProjectReference Include="..\BusinessRules.Wcf\BusinessRules.Wcf.csproj" />
</ItemGroup>
```

## Core Types

### BusinessRuleFault

A record type that wraps a `BusinessRuleBase` for use in WCF fault exceptions. This is designed for WCF service scenarios where faults need to be transmitted across service boundaries.

```csharp
[DataContract]
public record BusinessRuleFault(
    [property: DataMember]
    [property: Required]
    BusinessRuleBase BusinessRule);
```

## Extension Methods

### 1. `ToFaultException<T>()` (Static)

Creates a `FaultException<BusinessRuleFault>` from a business rule type.

```csharp
public static FaultException<BusinessRuleFault> ToFaultException<T>()
    where T : BusinessRule<T>, new()
```

**Example:**
```csharp
using BusinessRules.Wcf;

// Create a fault exception from a business rule type
var faultException = BusinessRuleWcfExtensions.ToFaultException<UserMustBeAdult>();
throw faultException;
```

### 2. `ToFaultException()` (from BusinessRuleBase)

Creates a `FaultException<BusinessRuleFault>` from a business rule instance.

```csharp
public static FaultException<BusinessRuleFault> ToFaultException(this BusinessRuleBase businessRule)
```

**Example:**
```csharp
var rule = new UserMustBeAdult();
var faultException = rule.ToFaultException();
throw faultException;
```

### 3. `ToFaultException()` (from BusinessRuleViolationException)

Creates a `FaultException<BusinessRuleFault>` from a business rule violation exception.

```csharp
public static FaultException<BusinessRuleFault> ToFaultException(this BusinessRuleViolationException exception)
```

**Example:**
```csharp
try
{
    ValidateUser(user);
}
catch (BusinessRuleViolationException ex)
{
    throw ex.ToFaultException();
}
```

## Complete Usage Examples

### Example 1: WCF Service with Business Rules

```csharp
using System.ServiceModel;
using BusinessRules;
using BusinessRules.Wcf;
using BusinessRules.Rules.UserValidation;

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
            // Validate age
            if (age < 18)
                throw UserMustBeAdult.ToException();

            // Validate password
            if (password.Length < 8)
                throw PasswordMinLength.ToException();

            // Create user
            return new User 
            { 
                Username = username, 
                Age = age, 
                Password = password 
            };
        }
        catch (BusinessRuleViolationException ex)
        {
            // Convert to WCF fault exception
            throw ex.ToFaultException();
        }
    }
}
```

### Example 2: WCF Client Handling Faults

```csharp
using System.ServiceModel;
using BusinessRules.Wcf;

public class UserServiceClient
{
    private readonly IUserService _service;

    public void CreateUserWithErrorHandling(string username, int age, string password)
    {
        try
        {
            var user = _service.CreateUser(username, age, password);
            Console.WriteLine($"User {user.Username} created successfully");
        }
        catch (FaultException<BusinessRuleFault> faultEx)
        {
            // Access the business rule information from the fault
            var rule = faultEx.Detail.BusinessRule;
            Console.WriteLine($"Business rule violation:");
            Console.WriteLine($"  Key: {rule.Key}");
            Console.WriteLine($"  Rule: {rule.Rule}");
            Console.WriteLine($"  Description: {rule.Description}");
            Console.WriteLine($"  Category: {rule.Category}");
            
            // Log or display to user
            LogBusinessRuleViolation(rule);
        }
        catch (FaultException ex)
        {
            Console.WriteLine($"Service fault: {ex.Message}");
        }
    }

    private void LogBusinessRuleViolation(BusinessRuleBase rule)
    {
        // Log the violation for auditing
        Console.WriteLine($"[{DateTime.UtcNow}] Rule {rule.Key} violated: {rule.Rule}");
    }
}
```

### Example 3: Validator Method with WCF Faults

```csharp
using BusinessRules;
using BusinessRules.Wcf;
using BusinessRules.Attributes;
using BusinessRules.Rules.UserValidation;

[ServiceContract]
public interface IValidationService
{
    [OperationContract]
    [FaultContract(typeof(BusinessRuleFault))]
    bool ValidateUserAge(int age);
}

public class ValidationService : IValidationService
{
    [ImplementsBusinessRule(UserMustBeAdult.Key)]
    public bool ValidateUserAge(int age)
    {
        if (age < 18)
        {
            // Throw WCF fault directly
            throw BusinessRuleWcfExtensions.ToFaultException<UserMustBeAdult>();
        }
        
        return true;
    }
}
```

### Example 4: Custom Fault Handler

```csharp
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using BusinessRules.Wcf;

public class BusinessRuleFaultHandler : IErrorHandler
{
    public bool HandleError(Exception error)
    {
        // Log the error
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
            // Convert to WCF fault
            var faultException = brException.ToFaultException();
            var messageFault = faultException.CreateMessageFault();
            fault = Message.CreateMessage(version, messageFault, faultException.Action);
        }
    }

    private void LogBusinessRuleViolation(BusinessRuleBase rule)
    {
        Console.WriteLine($"Business rule violation: {rule.Key} - {rule.Rule}");
    }
}
```

### Example 5: Service Behavior Attribute

```csharp
using System.ServiceModel;
using System.ServiceModel.Description;
using BusinessRules;
using BusinessRules.Wcf;

public class BusinessRuleFaultBehaviorAttribute : Attribute, IServiceBehavior
{
    public void AddBindingParameters(
        ServiceDescription serviceDescription,
        ServiceHostBase serviceHostBase,
        Collection<ServiceEndpoint> endpoints,
        BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(
        ServiceDescription serviceDescription,
        ServiceHostBase serviceHostBase)
    {
        foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
        {
            dispatcher.ErrorHandlers.Add(new BusinessRuleFaultHandler());
        }
    }

    public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }
}

// Apply to your service
[ServiceContract]
[BusinessRuleFaultBehavior]
public class UserService : IUserService
{
    // Service methods...
}
```

## Benefits of Using BusinessRules.Wcf

1. **Structured Faults**: Business rule violations are transmitted with full metadata
2. **Type Safety**: Strongly-typed fault contracts for compile-time checking
3. **Consistent Error Handling**: Standardized approach to handling business rule violations in WCF
4. **Client-Friendly**: Clients receive detailed information about what rule was violated
5. **Auditing Support**: Full rule context available for logging and auditing
6. **Legacy Support**: Maintains WCF compatibility for existing systems

## Migration from Core BusinessRules

If you were previously using `ToFaultException()` from the core BusinessRules library:

### Before (Old Code):
```csharp
using BusinessRules;

// This no longer exists in BusinessRules
var fault = UserMustBeAdult.ToFaultException();
```

### After (New Code):
```csharp
using BusinessRules;
using BusinessRules.Wcf;

// Use the extension method from BusinessRules.Wcf
var fault = BusinessRuleWcfExtensions.ToFaultException<UserMustBeAdult>();

// Or use instance extension method
var rule = new UserMustBeAdult();
var fault = rule.ToFaultException();

// Or from exception
var exception = UserMustBeAdult.ToException();
var fault = exception.ToFaultException();
```

## Important Notes

1. **Namespace Change**: `BusinessRuleFault` moved from `BusinessRules` to `BusinessRules.Wcf`
2. **Extension Methods**: `ToFaultException` is now an extension method, not a static method on `BusinessRule<T>`
3. **Additional Reference Required**: Projects using WCF features must reference `BusinessRules.Wcf`
4. **No Breaking Changes to Core**: The core `BusinessRules` library remains unchanged for non-WCF scenarios

## See Also

- [BusinessRules Documentation](../BusinessRules/BusinessRules-Documentation.md)
- [BusinessRules.ResultExtensions Documentation](../BusinessRules.ResultExtensions/BusinessRules.ResultExtensions-Documentation.md)
- [WCF Fault Contracts (Microsoft Docs)](https://docs.microsoft.com/en-us/dotnet/framework/wcf/specifying-and-handling-faults-in-contracts-and-services)
