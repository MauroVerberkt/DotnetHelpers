---
sidebar_position: 8
title: "ADR-008: Remove BusinessRuleResolver Runtime Reflection"
tags: [BusinessRules]
---

# ADR-008: Remove BusinessRuleResolver Runtime Reflection

**Status:** Accepted

## Context

`BusinessRuleResolver.FindBusinessRuleByKey()` was an internal utility that scanned all loaded assemblies via reflection to find a `BusinessRuleBase` instance matching a string key. It was called from the constructors of `BusinessRuleAttribute` and `ImplementsBusinessRuleAttribute` to populate a `Requirement` property.

Problems with this approach:

1. **Violates core principle**: The project explicitly states "source generators over runtime reflection" and "don't add runtime reflection where a source generator can solve the problem."
2. **Performance**: Every attribute instantiation triggered a full `AppDomain.GetAssemblies()` scan with no caching — O(assemblies × types × fields).
3. **Reliability**: `ReflectionTypeLoadException` risk from unloadable assemblies, and timing issues where attributes could be constructed before the rule-defining assembly was loaded.
4. **Redundancy**: Roslyn analyzers (BR001–BR004) already validate rule keys at compile time. The runtime check added no safety that wasn't already provided.

## Decision

Remove `BusinessRuleResolver` entirely (PROP-005, Option A). Attributes become purely declarative markers carrying only the string key. The `Requirement` property is removed from both attributes.

This is a binary-breaking change for any consumer that accessed `.Requirement` at runtime, but:
- The package is pre-1.0
- The property's value was unreliable (timing-dependent)
- Consumers needing the rule instance should reference the generated class directly

## Consequences

**Positive:**
- Zero runtime reflection in the BusinessRules package
- Attributes are now trivial, side-effect-free metadata
- No more `ArgumentException` from attribute constructors when assembly load order varies
- Simpler mental model: attributes mark intent, analyzers enforce correctness, generators provide typed access

**Negative:**
- Breaking change: `BusinessRuleAttribute.Requirement` and `ImplementsBusinessRuleAttribute.Requirement` removed
- Any consumer relying on runtime resolution must refactor to use generated classes directly

**Supersedes:** The "runtime fallback" clause in ADR-002 is no longer applicable.
