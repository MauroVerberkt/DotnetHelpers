---
sidebar_position: 3
title: "PROP-003: Testable Singleton Source Generator"
tags: [NewProject, SourceGenerators]
---

# PROP-003: Testable Singleton Source Generator

**Status:** idea  
**Size:** medium  
**Created:** 2026-05-25  

## Problem / Motivation

Singletons in .NET are either hand-rolled (error-prone double-check locking) or rely on DI container lifetime — which makes them hard to test in isolation. A source generator could emit a testable singleton pattern from an attribute:

- Thread-safe lazy initialization (generated, correct by construction)
- Swappable instance for unit testing
- No service locator — just a static access point with a test seam

## Sketch

```csharp
[TestSingleton]
public partial class FeatureFlagService
{
    public bool IsEnabled(string flag) => ...;
}

// Generated:
public partial class FeatureFlagService
{
    private static readonly Lazy<FeatureFlagService> _instance = new(...);
    public static FeatureFlagService Instance => _override ?? _instance.Value;

    private static FeatureFlagService? _override;

    /// <summary>
    /// Overrides the singleton instance for testing. Dispose to restore.
    /// </summary>
    public static IDisposable OverrideWith(FeatureFlagService mock)
    {
        _override = mock;
        return new OverrideScope(() => _override = null);
    }
}

// In tests:
using (FeatureFlagService.OverrideWith(mockService))
{
    // code under test sees mockService via .Instance
}
```

## Open Questions

- Interface extraction: should the generator also emit an interface?
- Thread safety of override: `AsyncLocal<T>` for async test isolation vs simple static?
- Async initialization support (`Lazy<Task<T>>` variant)?
- Naming: `[TestSingleton]`, `[GeneratedSingleton]`, `[Singleton]`?
- Should it integrate with DI registration (generate an `AddXxx` extension method)?

## Prior Art / References

- Ambient context pattern
- `AsyncLocal<T>` for test isolation in async contexts
- Existing BusinessRules source generator in this repo (incremental generator pattern)

## Outcome

_Pending_
