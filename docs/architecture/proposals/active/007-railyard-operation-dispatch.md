---
sidebar_position: 7
title: "PROP-007: Railyard — Compile-Time Operation Dispatch"
tags: [Railyard]
---

# PROP-007: Railyard — Compile-Time Operation Dispatch

**Status:** exploring  
**Size:** new-project  
**Created:** 2026-05-25  

## Problem / Motivation

A recurring pattern exists at serialization boundaries: something external sends a
name and a payload, and you need to route it to typed, validated code. Examples:

- **Hardware gateways** — TCP messages dispatched to device actions
- **Tool orchestrators** — MCP tool calls routed to implementations (code, CLI tools, APIs)
- **CLI applications** — subcommands dispatched to handlers
- **Message processors** — queue messages routed by type

The typical experience: the *interesting* code for a new operation is 10 lines, but
the *plumbing* — registration, deserialization, routing, error translation — is 50.
Every framework that solves this either relies on runtime reflection (not trim/AOT-safe,
no compile-time validation) or requires heavy ceremony per operation (marker interfaces,
manual registration, request/response type pairs).

**Railyard's thesis:** if you write one class with a declared name, input shape, and
execute method, everything else should be generated at compile time — the DI registration,
the dispatch routing, the input schema, and the tool manifest.

## Core Concepts

| Metaphor | Role |
|----------|------|
| **Yard** | Generated dispatch registry. Maps operation names to handlers, provides manifest of available operations. |
| **Operation** | A named unit of work. Declares its name, input shape, and pipeline (validate → execute). |
| **Manifest** | Auto-generated metadata: operation name, description, JSON schema for input. Consumable as MCP tool definitions, CLI help text, or API docs. |

## Pipeline

Each operation invocation flows through a railway-oriented pipeline:

```
name + JSON payload
       │
       ▼
  Resolve ──────► Option<Operation>     (name lookup — compile-time dispatch table)
       │
       ▼
  Deserialize ──► Result<TInput>        (JSON → typed record)
       │
       ▼
  Validate ─────► Result<TInput>        (business rules)
       │
       ▼
  Execute ──────► Result<TOutput>       (the actual work)
       │
       ▼
  Serialize ────► Result<string>        (typed output → JSON response)
```

Every step short-circuits on failure via `Result.Bind`. No exceptions cross the boundary.

## Developer Experience

### Defining an Operation

```csharp
[Operation("greet", Description = "Greets the user by name.")]
public class GreetOperation : Operation<GreetInput, GreetOutput>
{
    protected override Result<GreetInput> Validate(GreetInput input)
        => string.IsNullOrWhiteSpace(input.Name)
            ? Result.Failure<GreetInput>(Errors.Required(nameof(input.Name)))
            : Result.Success(input);

    protected override async Task<Result<GreetOutput>> Execute(GreetInput input)
        => Result.Success(new GreetOutput($"Hello, {input.Name}!"));
}

public record GreetInput(string Name);
public record GreetOutput(string Message);
```

That's it. No registration, no routing config, no serialization boilerplate.

### Bootstrapping

```csharp
services.AddRailyard(); // Generated extension method — registers all discovered operations
```

### Dispatching

```csharp
var yard = serviceProvider.GetRequiredService<IYard>();

Result<string> result = await yard.DispatchAsync("greet", """{"name": "World"}""");
```

### Getting the Manifest

```csharp
IReadOnlyList<OperationDescriptor> manifest = yard.Manifest;
// Each descriptor: Name, Description, InputSchema (JsonSchema)
```

## Source Generation Strategy

The Roslyn source generator:

1. **Discovers** all classes with `[Operation]` attribute
2. **Validates** at compile time:
   - TInput is a record
   - Name is unique across the assembly
   - Required overrides are present
3. **Emits:**
   - `AddRailyard()` extension method with all DI registrations
   - Dispatch table (name → type mapping, no reflection at runtime)
   - `OperationDescriptor` instances with JSON schemas derived from input records
   - Diagnostic errors for violations (missing attribute, duplicate names, non-record input)

## Async-First Design

The primary pipeline is async (`Task<Result<TOutput>>`). A synchronous convenience
exists but the expectation is most operations hit I/O (hardware, external tools, APIs).

```csharp
// Async (primary)
public abstract class Operation<TInput, TOutput> : IOperation
    where TInput : class
    where TOutput : class
{
    protected abstract Result<TInput> Validate(TInput input);
    protected abstract Task<Result<TOutput>> Execute(TInput input);
}

// Sync convenience
public abstract class SyncOperation<TInput, TOutput> : Operation<TInput, TOutput>
{
    protected sealed override Task<Result<TOutput>> Execute(TInput input)
        => Task.FromResult(ExecuteSync(input));

    protected abstract Result<TOutput> ExecuteSync(TInput input);
}
```

## Typed Output

Operations declare both `TInput` and `TOutput`. The framework serializes `TOutput` to
the JSON response string at the boundary. This gives:

- Type safety within the operation
- Typed composition when operations call each other in-process
- String serialization only at the external boundary

## Middleware / Behaviors (Future)

Pipeline behaviors wrap operations for cross-cutting concerns:

```csharp
public class TimingBehavior<TInput, TOutput> : IOperationBehavior<TInput, TOutput>
{
    public async Task<Result<TOutput>> Handle(
        TInput input,
        Func<Task<Result<TOutput>>> next)
    {
        var sw = Stopwatch.StartNew();
        var result = await next();
        Log.Information("Operation took {Elapsed}ms", sw.ElapsedMilliseconds);
        return result;
    }
}
```

Registered globally or per-operation. Generated into the pipeline at compile time or
resolved from DI at runtime (TBD).

## Target Use Cases

### 1. Tool Orchestrator (MCP)

Railyard sits between MCP skill declarations and actual tool implementations. The
manifest generates MCP-compatible tool definitions. Incoming tool calls dispatch
through the pipeline. Implementations can be C# code, external CLI invocations, or
API calls.

### 2. Hardware Gateway

TCP/serial messages arrive as `name + payload`. Railyard dispatches to hardware
operation handlers. Adding a new hardware action = one class.

### 3. Background Job Processor

Message queue delivers typed jobs. Railyard provides the dispatch, validation, and
error-handling pipeline without requiring a full framework like Hangfire or Wolverine.

## Open Questions

- **Multi-assembly support** — How does the generator handle operations spread across
  multiple assemblies? Likely: each assembly gets its own partial registry, a host
  generator aggregates them.
- **Output serialization** — Always JSON via System.Text.Json? Or pluggable serializers?
- **Operation dependencies** — Can operations invoke other operations in-process
  (typed, bypassing serialization)? If so, how does the DI scope work?
- **Behaviors registration** — Compile-time woven vs. runtime resolved? Compile-time
  is faster but less flexible.
- **Error taxonomy** — Should Railyard define standard error codes/categories
  (validation, not-found, unauthorized, infrastructure) or leave that to consumers?
- **Cancellation** — `CancellationToken` threading through the async pipeline.
- **Streaming** — Some operations (especially tool orchestration) may want to stream
  output. Does that fit the Result model or need a separate path?

## Prior Art / References

- **Railway-oriented programming** (Scott Wlaschin) — the Result monad composition model
- **MediatR** — dispatch-by-type, behaviors pipeline. Heavy on ceremony, no schema generation.
- **Wolverine** — convention-based message handling. Heavier, targets full messaging scenarios.
- **ASP.NET Minimal APIs** — source-generated request binding (similar compile-time philosophy)
- **MCP Tool Protocol** — name + JSON schema + execute. Railyard's manifest aligns naturally.
- **HelperMonads** — `Result<T>` and `Option<T>` from this repo, the foundation Railyard builds on.

## Outcome

_Exploring. Capturing the full vision before implementation begins._
