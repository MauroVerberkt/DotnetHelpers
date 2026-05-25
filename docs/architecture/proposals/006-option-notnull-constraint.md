---
sidebar_position: 6
title: "PROP-006: Add notnull Constraint to Option<TValue>"
tags: [HelperMonads]
---

# PROP-006: Add notnull Constraint to Option&lt;TValue&gt;

**Status:** ready  
**Size:** small  
**Created:** 2026-05-25  

## Problem / Motivation

`Option<TValue>` has no generic constraint, allowing:

- `Option<string?>` — defeats the purpose of Option (making nullability explicit)
- `Option<int?>` — wrapping an already-nullable type in an Option is semantically redundant
- No compiler enforcement that the wrapped value is non-null

`Result<TData>` already has `where TData : notnull`. The Option type should be consistent.

## Sketch

Add `where TValue : notnull` to all three types:

```csharp
public abstract class Option<TValue> where TValue : notnull { ... }
public sealed class Some<TValue>(TValue value) : Option<TValue> where TValue : notnull { ... }
public sealed class None<TValue> : Option<TValue> where TValue : notnull { ... }
```

No API changes beyond the constraint. Existing consumers using non-nullable types (the intended use case) are unaffected.

### Impact

- Consumers using `Option<SomeNullableType>` will get a compile error — this is intentional
- Value types (`Option<int>`, `Option<Guid>`) remain valid
- Reference types (`Option<string>`, `Option<MyClass>`) remain valid
- Only explicitly nullable annotations (`Option<string?>`) break

## Open Questions

- Should we also add `IEquatable<Option<TValue>>` while touching these types?
- Any consumer survey needed before this breaking change?

## Prior Art / References

- Rust's `Option<T>` — `T: Sized` (implicitly non-null)
- F#'s `Option<'T>` — value types allowed, null disallowed
- Current `Result<TData> where TData : notnull` in same project
- Current implementation: `src/HelperMonads/Option/Option.cs`

## Outcome

_Pending_
