---
sidebar_position: 5
title: "PROP-005: Replace BusinessRuleResolver Runtime Reflection"
tags: [BusinessRules]
---

# PROP-005: Replace BusinessRuleResolver Runtime Reflection

**Status:** exploring  
**Size:** medium  
**Created:** 2026-05-25  

## Problem / Motivation

`BusinessRuleResolver.FindBusinessRuleByKey()` scans ALL loaded assemblies, ALL types, and ALL static fields via reflection — invoked from the constructors of `BusinessRuleAttribute` and `ImplementsBusinessRuleAttribute` with no caching. This means:

- Every attribute instantiation triggers a full AppDomain scan
- `ReflectionTypeLoadException` risk from unloadable assemblies
- Timing issues: attributes may be constructed before the rule-containing assembly is loaded
- Performance degrades linearly as the application grows

## Sketch

### Option A: Remove resolver entirely

Attributes become purely declarative (string key only). Remove the `Requirement` property that triggers resolution. The analyzers already enforce correctness at compile-time — runtime resolution adds no safety.

```csharp
// Before
public class BusinessRuleAttribute(string ruleKey, ...) : Attribute
{
    public BusinessRuleBase Requirement { get; } = 
        BusinessRuleResolver.FindBusinessRuleByKey(ruleKey) ?? throw ...;
}

// After
public class BusinessRuleAttribute(string ruleKey, ...) : Attribute
{
    public string RuleKey { get; } = ruleKey;
}
```

### Option B: Lazy-cached resolver

Keep the runtime resolution but defer and cache:

```csharp
internal static class BusinessRuleResolver
{
    private static readonly ConcurrentDictionary<string, BusinessRuleBase?> _cache = new();

    internal static BusinessRuleBase? FindBusinessRuleByKey(string key)
        => _cache.GetOrAdd(key, ScanForKey);

    private static BusinessRuleBase? ScanForKey(string key) { ... }
}
```

### Option C: Source-generated lookup

Extend the existing `BusinessRuleSourceGenerator` to also emit a static dictionary:

```csharp
// Auto-generated
internal static class BusinessRuleLookup
{
    public static BusinessRuleBase? FindByKey(string key) => key switch
    {
        "USER_MUST_BE_AUTHENTICATED" => new UserMustBeAuthenticated(),
        _ => null
    };
}
```

## Open Questions

- Do consumers actually use the `Requirement` property on attributes at runtime?
- If Option A, is the breaking change acceptable at v1.0.0?
- If Option C, should the lookup be per-assembly or global?

## Prior Art / References

- Current implementation: `src/BusinessRules/Utilities/BusinessRuleResolver.cs`
- Roslyn source generators: already in the project (`BusinessRulesGenerator`)

## Outcome

_Pending_
