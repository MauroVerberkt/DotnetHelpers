---
sidebar_position: 2
title: "ADR-002: Source Generators for Business Rules"
tags: [BusinessRules]
---

# ADR-002: Source Generators for Business Rules

**Status:** Accepted

## Context

Business rules need to be defined once and referenced consistently across the application. Several approaches were considered:

1. **Manual classes**: Developers hand-write rule classes. Error-prone, no single source of truth.
2. **Reflection-based**: Rules defined as constants or in config, resolved at runtime via reflection. Runtime cost, no IntelliSense, no compile-time validation.
3. **Attributes-only**: Rules defined entirely through attributes. Limited metadata, hard to enumerate all rules.
4. **Source generators**: Rules defined in JSON, classes generated at compile time. Zero runtime cost, full IntelliSense, compile-time type safety.

## Decision

Use an **Incremental Source Generator** (`IIncrementalGenerator`) that reads `*.BusinessRules.json` files from `AdditionalTexts` and generates strongly-typed C# classes at compile time. Each generated class:

- Inherits from `BusinessRule<T>` (CRTP pattern)
- Has `const string` fields for Key, Rule, Description, Category
- Is organized into namespaces by category (e.g., `BusinessRules.Rules.Authentication`)

A `BusinessRuleResolver` using reflection is retained as a runtime fallback for attribute-based lookup.

## Consequences

**Positive:**
- JSON file serves as single source of truth for all business rules
- Generated `const` fields enable IntelliSense and compile-time references
- Roslyn analyzers can validate rule keys exist (BR001) at compile time
- Zero runtime reflection cost for rule creation
- Rules are automatically organized into namespace categories
- Updating rules only requires editing JSON; regeneration is automatic

**Negative:**
- Source generators add complexity to the build pipeline
- Debugging generated code requires understanding the generator
- `System.Text.Json` must be bundled as a private dependency (netstandard2.0 doesn't include it)
- Two-step mental model: edit JSON first, then use generated classes
