---
sidebar_position: 15
title: "PROP-015: Generated Discriminated Unions"
tags: [HelperUnions]
---

# PROP-015: Generated Discriminated Unions

**Status:** ready  
**Size:** new-project  
**Created:** 2026-05-29  

## Problem / Motivation

DotnetHelpers provides rich functional programming abstractions:

- **Result** — success and failure
- **Option** — presence and absence
- **Async helpers** — safe composition over async boundaries
- **Source generators** — compile-time code generation
- **Roslyn analyzers** — compile-time validation

These solve operational outcomes but do not address domain modeling scenarios where a value represents one of several known variants:

```csharp
Customer | Supplier | Partner
Created | Updated | Deleted
CreditCard | BankTransfer | PayPal
```

Developers commonly model these using inheritance hierarchies, marker interfaces, or generic union types. These approaches lack:

- Exhaustive handling guarantees
- Strong discoverability
- Clear domain intent
- Compile-time safety

Named discriminated unions provide a more expressive and maintainable solution.

## Decisions

### Declaration Syntax

Unions are declared as `partial record` types with nested `sealed record` variants:

```csharp
[Union]
public partial record BusinessParty
{
    public sealed record Customer(CustomerInfo Info);

    public sealed record Supplier(SupplierInfo Info);

    public sealed record Partner(PartnerInfo Info, PartnerTier Tier);

    public sealed record Prospect();
}
```

Key rules:

- **`partial record` only** — `partial class` is not supported. Records provide value equality, `with` expressions, immutability signaling, and `ToString()` for free. These are all desirable properties for domain values.
- **No variant naming prefix** — Variants use plain names (`Customer`, not `AsCustomer`). The `As` prefix is already a .NET convention for type conversions and would conflict.
- **Variants support all arities** — zero fields, single field, multiple fields.

### Why Record

Records are the right choice because:

- **Value equality** — Two `BusinessParty.Customer(same)` are equal without manual `Equals`/`GetHashCode`.
- **Immutability** — DUs are values. Records communicate this intent.
- **Deconstruction** — Positional records deconstruct naturally in pattern matching.
- **ToString** — Free readable representation including variant data.
- **`with` expressions** — Enable transformations on variants.

### Internal Representation

The internal representation is an inheritance hierarchy. Each nested variant record inherits from the outer union type. The CLR type itself serves as the discriminator — no explicit tag field is needed.

This is dictated by the declaration syntax: nested `sealed record` types within a `partial record` base.

### Equality Semantics

Two union values are equal when they hold the same variant with the same payload. This follows directly from record equality semantics and requires no custom implementation.

### Generated API

#### Construction

```csharp
BusinessParty party = new BusinessParty.Customer(customerInfo);

// Implicit conversion (single-field variants only):
BusinessParty party = customerInfo;
```

#### Variant Inspection

```csharp
party.IsCustomer    // bool
party.IsSupplier    // bool
party.IsPartner     // bool
party.IsProspect    // bool
```

#### Variant Extraction

```csharp
party.TryGetCustomer(out var info)                  // bool
party.TryGetPartner(out var info, out var tier)      // bool
```

#### Match (Named Builder)

```csharp
var name = party.Match()
    .Customer(info => info.Name)
    .Supplier(info => info.CompanyName)
    .Partner((info, tier) => $"{info.Name} ({tier})")
    .Prospect(() => "Unknown")
    .Result();
```

Design properties:

- **Order-independent** — adding a variant breaks at `.Result()`, not silently at a positional index.
- **Self-documenting** — variant names appear in the chain.
- **Unwrapped** — lambdas receive the payload fields directly, not the variant wrapper.
- **Type-safe exhaustiveness** — `.Result()` only exists on the builder type when all variants are covered. Missing a variant is a compile error.
- **No-payload shorthand** — variants with no fields accept a constant value:

```csharp
.Prospect("Unknown")
```

#### Match Async

```csharp
var name = await party.MatchAsync()
    .Customer(async info => await LoadDetails(info))
    .Supplier(async info => await LoadSupplier(info))
    .Partner(async (info, tier) => await LoadPartner(info, tier))
    .Prospect(() => Task.FromResult("Unknown"))
    .ResultAsync();
```

Async support is provided from day one, consistent with Result and Option.

#### Switch

```csharp
party.Switch()
    .Customer(info => HandleCustomer(info))
    .Supplier(info => HandleSupplier(info))
    .Partner((info, tier) => HandlePartner(info, tier))
    .Prospect(() => LogProspect())
    .Execute();
```

Same builder pattern as Match, but returns `void` via `.Execute()`.

### Pattern Matching Support

Generated unions work with existing C# pattern matching:

```csharp
switch (party)
{
    case BusinessParty.Customer { Info: var info }:
        break;
    case BusinessParty.Supplier { Info: var info }:
        break;
    case BusinessParty.Partner { Info: var info, Tier: var tier }:
        break;
    case BusinessParty.Prospect:
        break;
}
```

No custom matching syntax is required. The proposal embraces existing C# language features.

### Adding a Variant — Loud Breaking

When a developer adds a new variant, the following breaks occur:

- **All `.Match().Result()` calls** — compile error. The builder type for the incomplete match does not expose `.Result()`.
- **All `.Switch().Execute()` calls** — compile error. Same mechanism.
- **All C# `switch` statements/expressions** — analyzer warning (configurable to error via `.editorconfig`).

This is intentionally loud. Silent breakage is unacceptable for domain modeling.

### Why Not Inheritance

C# inheritance is the most common alternative to discriminated unions. The generated union implementation uses inheritance internally, but the value proposition is the exhaustive API layer on top. Key differences:

**Inheritance models "is-a"; unions model "is one of."**

Inheritance implies a taxonomic relationship — all subtypes share a common identity. Unions express that a value can be exactly one of several known possibilities without implying any shared nature. A `SearchResult` union may contain a `Customer`, `Product`, or `Order`, even though those types are completely unrelated:

```csharp
[Union]
public partial record SearchResult
{
    public sealed record FoundCustomer(Customer Value);
    public sealed record FoundProduct(Product Value);
    public sealed record FoundOrder(Order Value);
}
```

Inheritance would be artificial here — `Customer`, `Product`, and `Order` have no meaningful common base. The union simply expresses a finite set of alternatives.

**Inheritance hierarchies are open; unions are closed.**

Anyone can add a subclass to an abstract base. Unions are closed by design — the set of variants is fixed at compile time. C# has no `permits` clause (unlike Java/Kotlin), so the compiler cannot enforce closure on plain inheritance. The generator provides this guarantee through the builder API.

**Inheritance lacks exhaustiveness guarantees.**

With a plain abstract base class and `switch`, missing a subtype is silent unless you add a default/discard case. The generated Match builder makes incomplete handling a compile error — `.Result()` does not exist until all variants are covered.

**Summary:** The generator uses inheritance under the hood but provides guarantees that raw inheritance cannot: closed variant sets, exhaustive matching via the type system, and domain modeling without requiring "is-a" relationships.

### Integration With Result and Option

Unions model alternatives. Result models outcomes. Option models absence. These concepts are complementary and should not be conflated.

| Abstraction | Models | Example |
|---|---|---|
| **Result** | Success or failure | Operation may fail |
| **Option** | Present or absent | Value may not exist |
| **Union** | One of N alternatives | Value is one of several domain variants |

These compose orthogonally:

```csharp
Result<BusinessParty>          // Operation may fail; success is a domain variant
Option<BusinessParty>          // Party may not exist; when present, it's a variant
Result<Option<BusinessParty>>  // Lookup may fail; if it succeeds, party might not exist
```

The anti-pattern to avoid:

```csharp
// Conflating domain alternatives with operational outcomes:
OneOf<Customer, Supplier, NotFound, ValidationError, DatabaseError>
```

This mixes *what the thing is* with *what went wrong trying to get it*. Every consumer must handle error cases even when they only want to branch on domain variants, and error handling becomes coupled to domain knowledge it shouldn't need. Keeping these axes separate produces cleaner, more composable code.

### Analyzer Support

#### Exhaustive Switch/Pattern Match Validation

```csharp
switch (party)
{
    case BusinessParty.Customer customer:
        break;
}
```

Diagnostic:

```text
DNHU0001: Non-exhaustive union match. Missing variants: Supplier, Partner, Prospect.
```

Default severity: **Warning**, configurable to Error via `.editorconfig`.

The named builder Match/Switch API does not need analyzer support — exhaustiveness is enforced by the type system directly.

#### Invalid Union Declaration

```csharp
[Union]
public partial class BusinessParty { }  // Not a record
```

Diagnostic:

```text
DNHU0003: [Union] may only be applied to partial record declarations.
```

Default severity: **Error**.

#### Future: Discarded Union Detection

```csharp
GetBusinessParty();  // Return value ignored
```

Diagnostic:

```text
DNHU0002: Union value was never inspected.
```

This rule is opt-in and deferred to a future version.

### Package Structure

Consumers install a single NuGet package:

```
dotnet add package HelperUnions
```

The source projects remain separate for separation of concerns and testability, but the consumer never sees that distinction. The `HelperUnions.Package` project bundles the attribute assembly, source generator, analyzer, and code fix provider into one package.

```
src/
├── HelperUnions/                  # [Union] attribute, builder base types
├── HelperUnionsGenerator/         # Source generator
├── HelperUnionsAnalyzer/          # Exhaustiveness analyzer
├── HelperUnionsFixProvider/       # "Add missing variants" code fix
tools/
├── HelperUnions.Package/          # NuGet packaging (single consumer-facing package)
tests/
├── HelperUnions.UnitTests/        # Generator output + analyzer diagnostic tests
```

### Performance

Generated unions:

- Avoid reflection
- Avoid boxing (variants are reference types via record)
- Support trimming
- Support NativeAOT
- Minimize allocations (no wrapper beyond the variant itself)

## Sketch

### Attribute

```csharp
namespace HelperUnions;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UnionAttribute : Attribute { }
```

### Generated Builder (conceptual)

```csharp
// Generated for BusinessParty:
public partial record BusinessParty
{
    public MatchBuilder<TResult> Match<TResult>() => new();

    public struct MatchBuilder_0<TResult>
    {
        public MatchBuilder_1<TResult> Customer(Func<CustomerInfo, TResult> handler);
    }

    // Each .Variant() call returns the next builder type
    // .Result() only appears on the final builder type
}
```

The exact generated shape requires prototyping, but the principle is: each builder state is a distinct type, and only the fully-saturated state exposes the terminal method.

## Open Questions

- **Serialization strategy** — Should serialization support be built into the generator, a separate optional package, or a separate generator? Leaning toward separate package to keep the core focused.
- **Generic variants** — Should variants with generic type parameters be supported in V1? Example: `public sealed record Success<T>(T Value)`. Likely deferred.
- **Code fix scope** — Should the "add missing variants" code fix target Match builder chains, switch statements, or both?

## Prior Art / References

- [F# Discriminated Unions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions) — primary inspiration for ergonomics and safety
- [OneOf](https://github.com/mcintyre321/OneOf) — generic discriminated union for .NET
- [Thinktecture.Runtime.Extensions](https://github.com/PawelGerr/Thinktecture.Runtime.Extensions) — source-generated discriminated unions
- [language-ext](https://github.com/louthy/language-ext) — functional programming framework for .NET
- [Rust enums](https://doc.rust-lang.org/book/ch06-00-enums.html) — tagged unions with exhaustive matching
- [Swift enums](https://docs.swift.org) — enums with associated values and exhaustive matching

## Outcome

_Filled when status changes to done/parked._
