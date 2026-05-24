---
sidebar_position: 7
title: "ADR-007: Incremental Generator Over Legacy API"
tags: [BusinessRules]
---

# ADR-007: Incremental Generator Over Legacy API

**Status:** Accepted

## Context

Roslyn offers two source generator APIs:

1. **`ISourceGenerator`** (legacy): Runs on every keystroke, receives the full compilation. Simple but causes IDE lag with large projects.
2. **`IIncrementalGenerator`** (modern): Pipeline-based with built-in caching. Only re-runs when inputs change. Introduced in .NET 6 / Roslyn 4.0.

The BusinessRules generator reads JSON files and produces C# classes. On every edit, the legacy API would re-parse all JSON files and regenerate all classes, even if unrelated code changed.

## Decision

Use `IIncrementalGenerator` with the incremental pipeline API:

```csharp
[Generator]
public class BusinessRuleSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("BusinessRules.json"));
        // ...pipeline with caching
    }
}
```

Target `netstandard2.0` for maximum host compatibility (VS, Rider, CLI).

## Consequences

**Positive:**
- Significantly better IDE performance - only re-runs when JSON files actually change
- Built-in caching eliminates redundant work
- Pipeline model makes data flow explicit and testable
- `netstandard2.0` target ensures compatibility with all Roslyn hosts
- Modern API with better long-term support from Microsoft

**Negative:**
- More complex API than the legacy `ISourceGenerator`
- Pipeline model has a steeper learning curve (transforms, combines, etc.)
- Must bundle `System.Text.Json` as a private dependency (netstandard2.0 doesn't include it)
- Requires a custom MSBuild target (`GetDependencyTargetPaths`) to ensure the bundled DLL is available at analyzer load time
- Debugging requires attaching to the compiler process
