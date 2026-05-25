---
sidebar_position: 5
title: "ADR-005: CRTP for Static Factory Methods"
tags: [BusinessRules]
---

# ADR-005: CRTP for Static Factory Methods

**Status:** Accepted

## Context

Business rule classes need static factory methods (`ToException()`, `ToFaultException()`) that create instances of the concrete type without requiring the caller to instantiate the class first. In C#, static methods aren't virtual and can't be overridden, so a mechanism is needed to make the base class aware of the derived type.

Alternatives considered:

1. **Instance methods**: Require `new UserMustBeAdult().ToException()` - verbose
2. **Static methods on a helper class**: `BusinessRuleHelper.ToException<UserMustBeAdult>()` - not discoverable
3. **CRTP (Curiously Recurring Template Pattern)**: `BusinessRule<T> where T : BusinessRule<T>, new()` - enables `UserMustBeAdult.ToException()`

## Decision

Use the CRTP pattern on `BusinessRule<T>`:

```csharp
public abstract class BusinessRule<T> : BusinessRuleBase
    where T : BusinessRule<T>, new()
{
    public static BusinessRuleViolationException ToException()
    {
        var instance = new T();
        return new BusinessRuleViolationException(instance);
    }
}
```

This enables clean call-site syntax: `throw UserMustBeAdult.ToException();`

## Consequences

**Positive:**
- Clean, discoverable API: `UserMustBeAdult.ToException()` reads naturally
- No instantiation boilerplate at call sites
- Generated classes automatically get the static methods via inheritance
- `new()` constraint ensures all generated classes have parameterless constructors
- Works with the source generator since generated classes use primary constructors

**Negative:**
- CRTP is an unfamiliar pattern for many C# developers
- Requires a suppress comment in code (`// Intentional CRTP pattern`)
- `new()` constraint allocates on every call (mitigated: exceptions are exceptional)
- Slightly harder to explain in documentation
- Cannot be used with abstract intermediate classes in the hierarchy
