# BusinessRules

This repository contains a C# implementation of a **BusinessRules** framework, a code generation-based approach to defining, managing, and enforcing business rules throughout an application. The implementation uses **Source Generators** to automatically create strongly-typed business rule classes from JSON configuration files, making rules reusable, type-safe, and easily discoverable across your codebase.

## What is the BusinessRules Framework?

The **BusinessRules** framework is a construct used to:

- Define business rules in a declarative JSON format that serves as a single source of truth.
- Automatically generate strongly-typed C# classes from JSON definitions using Source Generators.
- Associate business rules with methods and classes through attributes.
- Centralize business rule definitions for consistency across the application.
- Generate standardized exceptions and fault exceptions when rules are violated.
- Enable compile-time validation of business rule keys through Roslyn Analyzers.

In this implementation, business rules are defined in JSON files (named `*.BusinessRules.json`) and the framework automatically generates C# classes organized by category into namespaces.

## Components

### Core Classes

#### `BusinessRuleBase`

This is the abstract base class for all business rules. It defines the core properties that every business rule must have:

- **InternalKey**: A unique identifier for the business rule.
- **InternalRule**: The human-readable description of the rule.
- **InternalDescription**: Optional detailed description of the rule.
- **InternalCategory**: Optional category for organizing rules.

The class is decorated with `[DataContract]` to support serialization scenarios.

#### `BusinessRule<T>`

This is the generic abstract class for defining concrete business rules. It inherits from `BusinessRuleBase` and provides:

- **Constructor Parameters**:
  - `key`: Unique identifier for the rule.
  - `rule`: The human-readable rule statement.
  - `description`: Optional detailed description (default: empty string).
  - `category`: Optional category for organization (default: empty string).

- **Static Methods**:
  - `ToFaultException()`: Creates a `FaultException<BusinessRuleFault>` for WCF scenarios.
  - `ToException()`: Creates a `BusinessRuleViolationException` with the rule's message.
  - `ToException(string message)`: Creates a `BusinessRuleViolationException` with a custom message.

This class uses the self-referencing generic pattern (`where T : BusinessRule<T>, new()`) to enable static factory methods that work with generated classes.

#### `BusinessRuleFault`

A record type that wraps a `BusinessRuleBase` for use in fault exceptions. This is designed for WCF service scenarios where faults need to be transmitted across service boundaries.

#### `BusinessRuleViolationException`

A custom exception class that encapsulates a violated business rule. It provides:

- **Properties**:
  - `BusinessRule`: The violated business rule.
  - `Key`: The rule's unique key.
  - `Rule`: The rule's human-readable statement.
  - `Description`: The rule's detailed description.
  - `Category`: The rule's category.

- **Constructors**:
  - `BusinessRuleViolationException(BusinessRuleBase businessRule)`: Creates an exception with the rule's message.
  - `BusinessRuleViolationException(BusinessRuleBase businessRule, string message)`: Creates an exception with a custom message.
  - `BusinessRuleViolationException(BusinessRuleBase businessRule, string message, Exception innerException)`: Creates an exception with a custom message and inner exception.

### Attributes

#### `BusinessRuleAttribute`

An attribute for marking methods or classes as being governed by a specific business rule. Features:

- **Parameters**:
  - `ruleKey`: The unique key of the business rule to apply.
  - `enforceValidation`: Whether validation should be enforced (default: `true`).

- **Properties**:
  - `RuleKey`: The rule's unique key.
  - `Rule`: The resolved `BusinessRuleBase` instance.
  - `EnforceValidation`: Whether validation is enforced.

The attribute automatically resolves the business rule at construction time using `BusinessRuleResolver`.

#### `ImplementsBusinessRuleAttribute`

An attribute for marking methods or classes that implement a specific business rule. Features:

- **Parameters**:
  - `ruleKey`: The unique key of the business rule being implemented.

- **Properties**:
  - `RuleKey`: The rule's unique key.
  - `Rule`: The resolved `BusinessRuleBase` instance.

This attribute is useful for documentation and discovery, allowing you to find all implementations of a particular rule.

### Utilities

#### `BusinessRuleResolver`

A static utility class that provides runtime discovery of business rules through reflection:

- **FindBusinessRuleByKey(string key)**: Searches all loaded assemblies for a static field of type `BusinessRuleBase` with a matching key.

This enables dynamic rule lookup and validation at runtime.

### Source Generator

#### `BusinessRuleSourceGenerator`

An incremental source generator that:

- Detects all `*.BusinessRules.json` files added as `AdditionalFiles` to the project.
- Parses the JSON configuration to extract business rule definitions.
- Generates strongly-typed C# classes that inherit from `BusinessRule<T>`.
- Organizes generated classes into namespaces based on the `category` field (e.g., `BusinessRules.Rules.Authentication`).
- Creates constant fields for `Key`, `Rule`, `Description`, and `Category` in each generated class.

The generator runs automatically during compilation, ensuring your code always has access to the latest rule definitions.

## Use Cases

The BusinessRules framework can be used in scenarios where:

- Business rules need to be defined once and referenced consistently throughout the application.
- You want to generate standardized exceptions with contextual information when rules are violated.
- Rules need to be discoverable at runtime for validation or documentation purposes.
- You're working with distributed systems (e.g., WCF) and need to transmit rule violations as structured faults.
- You want to associate business rules with specific methods or classes for documentation and validation.

Examples of common use cases:

- Domain-driven design implementations where business rules are first-class domain objects.
- API validation where specific endpoints enforce specific business rules.
- Service layer validation in multi-tier applications.
- Generating consistent error responses with rule metadata.

## Example Usage

### Step 1: Define Business Rules in JSON

Create a JSON file named `MyApp.BusinessRules.json` with your business rule definitions:

```json
{
  "$schema": "../schemas/businessrules-schema.json",
  "businessRules": [
    {
      "className": "UserMustBeAdult",
      "key": "USER_AGE_MIN",
      "rule": "User must be at least 18 years old",
      "description": "Users under 18 cannot create accounts due to legal requirements",
      "category": "UserValidation"
    },
    {
      "className": "PasswordMinLength",
      "key": "PWD_MIN_LENGTH",
      "rule": "Password must contain at least 8 characters",
      "description": "Passwords must meet minimum security requirements",
      "category": "Security"
    },
    {
      "className": "UserMustBeAuthenticated",
      "key": "USER_AUTH",
      "rule": "User must be authenticated",
      "description": "User must provide valid authentication credentials",
      "category": "Authentication"
    }
  ]
}
```

### Step 2: Add the JSON File to Your Project

Edit your `.csproj` file to include the JSON file as an additional file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <AdditionalFiles Include="MyApp.BusinessRules.json" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="BusinessRules" Version="1.0.0" />
    <PackageReference Include="BusinessRules.Analyzers.Package" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Step 3: Build and Use Generated Classes

After building your project, the source generator will create classes in namespaces based on the category. You can now use them:

```csharp
using System;
using BusinessRules;
using BusinessRules.Attributes;
using BusinessRules.Rules.UserValidation;
using BusinessRules.Rules.Security;
using BusinessRules.Rules.Authentication;

// First, define validators using [ImplementsBusinessRule]
// These contain the actual validation logic and throw exceptions
public class Validators
{
    [ImplementsBusinessRule(UserMustBeAdult.Key)]
    public void ValidateUserAge(int age)
    {
        if (age < 18)
        {
            throw UserMustBeAdult.ToException();
        }
    }

    [ImplementsBusinessRule(PasswordMinLength.Key)]
    public void ValidatePassword(string password)
    {
        if (password.Length < 8)
        {
            throw PasswordMinLength.ToException($"Password is too short");
        }
    }

    [ImplementsBusinessRule(UserMustBeAuthenticated.Key)]
    public void ValidateAuthentication(User user)
    {
        if (user == null || !user.IsAuthenticated)
        {
            throw UserMustBeAuthenticated.ToException();
        }
    }
}

// Then use [BusinessRule] attribute on methods that require these rules
// and call the validators from those methods
public class UserService
{
    private readonly Validators _validators = new Validators();

    // Declare that this method is governed by the authentication rule
    [BusinessRule(UserMustBeAuthenticated.Key)]
    public void CreateUser(string username, int age, string password, User currentUser)
    {
        // Call validators - they will throw if validation fails
        _validators.ValidateAuthentication(currentUser);
        _validators.ValidateUserAge(age);
        _validators.ValidatePassword(password);

        Console.WriteLine($"User {username} created successfully");
    }

    // You can also use string literals for the key
    [BusinessRule("USER_AGE_MIN")]
    public void UpdateUserAge(int userId, int newAge)
    {
        _validators.ValidateUserAge(newAge);
        // Update logic here
    }
}

public class Program
{
    public static void Main()
    {
        var service = new UserService();
        var authenticatedUser = new User { IsAuthenticated = true };
        var unauthenticatedUser = new User { IsAuthenticated = false };

        // Example 1: Valid user creation
        try
        {
            service.CreateUser("john_doe", 25, "SecurePass123", authenticatedUser);
            // Output: User john_doe created successfully
        }
        catch (BusinessRuleViolationException ex)
        {
            Console.WriteLine($"Rule violated: {ex.Rule}");
            Console.WriteLine($"Key: {ex.Key}");
            Console.WriteLine($"Category: {ex.Category}");
        }

        // Example 2: Age validation failure
        try
        {
            service.CreateUser("jane_doe", 16, "SecurePass123", authenticatedUser);
        }
        catch (BusinessRuleViolationException ex)
        {
            Console.WriteLine($"Rule violated: {ex.Rule}"); 
            // Output: User must be at least 18 years old
            Console.WriteLine($"Key: {ex.Key}"); 
            // Output: USER_AGE_MIN
            Console.WriteLine($"Category: {ex.Category}"); 
            // Output: UserValidation
        }

        // Example 3: Password validation failure
        try
        {
            service.CreateUser("bob", 30, "short", authenticatedUser);
        }
        catch (BusinessRuleViolationException ex)
        {
            Console.WriteLine($"Custom message: {ex.Message}"); 
            // Output: Password is too short
            Console.WriteLine($"Rule: {ex.Rule}"); 
            // Output: Password must contain at least 8 characters
        }

        // Example 4: Authentication failure
        try
        {
            service.CreateUser("alice", 25, "SecurePass123", unauthenticatedUser);
        }
        catch (BusinessRuleViolationException ex)
        {
            Console.WriteLine($"Rule violated: {ex.Rule}"); 
            // Output: User must be authenticated
            Console.WriteLine($"Key: {ex.Key}"); 
            // Output: USER_AUTH
        }
    }
}

public class User
{
    public bool IsAuthenticated { get; set; }
}
```

### Step 4: Generated Code Example

The source generator will create files like `MyApp.BusinessRules.UserValidation.g.cs`:

```csharp
// <auto-generated />
using BusinessRules;

namespace BusinessRules.Rules.UserValidation;

public class UserMustBeAdult() : BusinessRule<UserMustBeAdult>(Key, Rule, Description, Category)
{
    public const string Key = "USER_AGE_MIN";

    public const string Rule = "User must be at least 18 years old";

    public const string Description = "Users under 18 cannot create accounts due to legal requirements";

    public const string Category = "UserValidation";
}
```

### Explanation of Example:

1. **Defining Business Rules**:
   - Rules are defined in a JSON file with a specific schema.
   - Each rule has a `className` (PascalCase), `key` (typically UPPER_SNAKE_CASE), `rule` (human-readable), optional `description`, and optional `category`.

2. **Code Generation**:
   - The source generator automatically creates strongly-typed classes during compilation.
   - Classes are organized into namespaces based on their category (e.g., `BusinessRules.Rules.Authentication`).
   - Each class has constant fields for easy access to metadata.

3. **Using Attributes**:
   - `[ImplementsBusinessRule(key)]` marks validator methods that contain the actual validation logic and throw exceptions.
   - `[BusinessRule(key)]` marks business methods that are governed by a rule and should call the validators.
   - The analyzers will validate that the keys exist in your JSON file at compile-time.

4. **Throwing Exceptions**:
   - **Only throw business rule exceptions inside methods marked with `[ImplementsBusinessRule]`**.
   - Use `ToException()` to create a standard exception with the rule's message.
   - Use `ToException(message)` to create an exception with a custom message while preserving rule metadata.
   - Use `ToFaultException()` for WCF scenarios.
   - Business methods with `[BusinessRule]` should call validators, not throw directly (to avoid BR004 warnings).

5. **Handling Violations**:
   - Catch `BusinessRuleViolationException` to access rule metadata (Key, Rule, Description, Category).
   - Use this information for logging, user feedback, or auditing.

6. **Compile-Time Safety**:
   - Roslyn analyzers validate that business rule keys used in attributes are defined in the JSON file.
   - Analyzer `BR001` reports an error if you reference a non-existent rule key.
   - Analyzer `BR002` reports an error if a rule is used but has no validator (method with `[ImplementsBusinessRule]`).
   - Analyzer `BR003` reports a warning if `enforceValidation: false` is used without a validator.
   - Analyzer `BR004` reports a warning if you throw a business rule exception without the `[ImplementsBusinessRule]` attribute.

## Benefits of Using the BusinessRules Framework

- **Single Source of Truth**: Define all business rules in a centralized JSON file that serves as the definitive source for your entire application.
- **Code Generation**: Automatically generate strongly-typed classes from JSON, eliminating manual coding and reducing errors.
- **Compile-Time Validation**: Roslyn analyzers ensure that business rule keys are valid and validators exist, catching errors before runtime.
- **Type Safety**: Generated classes provide IntelliSense support and prevent typos in rule keys.
- **Consistency**: Ensure consistent messaging and behavior across your application with centralized rule definitions.
- **Rich Metadata**: Each rule violation carries full context (key, rule, description, category) for logging and debugging.
- **Organized Structure**: Rules are automatically organized into namespaces based on categories for better code organization.
- **Testability**: Rules can be easily tested in isolation, and their usage can be validated through attributes and analyzers.
- **Documentation**: Attributes serve as self-documenting code, clearly indicating which rules apply to which methods.
- **Distributed Systems**: Built-in support for WCF fault exceptions enables rule violations to cross service boundaries with full fidelity.
- **Maintainability**: Update rules in JSON without modifying source code; the generator handles the rest automatically.
