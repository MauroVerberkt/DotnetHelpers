---
sidebar_position: 4
title: "PROP-004: SpudLog - Potato-Themed Logging Framework"
tags: [NewProject]
---

# PROP-004: SpudLog - Potato-Themed Logging Framework

**Status:** idea  
**Size:** new-project  
**Created:** 2026-05-25  

## Problem / Motivation

High-performance .NET logging (Serilog, NLog) requires boilerplate and runtime allocations for structured logging. Microsoft's `LoggerMessage.Define` is zero-alloc but verbose. Beyond performance:

- No built-in crash-safe (durable) pipeline — messages lost on process crash
- Object introspection requires manual destructuring
- No quick "breakpoint-style" debug tracing without attaching a debugger

SpudLog combines high performance with developer ergonomics and potato-themed fun.

## Sketch

**Core pillars:**

1. **Roslyn-generated zero-allocation logging** — source generator emits `LoggerMessage`-style methods from partial method signatures
2. **Reflection-based object introspection** — auto-destructure any object into structured log properties (opt-in, depth-limited)
3. **Async crash-safe pipeline** — buffered channel-based sink with WAL (write-ahead log) so messages survive process crashes
4. **Potato utilities** — `SpudLog.Potato()` as a breakpoint-style trace

```csharp
// Zero-alloc generated logging
[SpudLog]
public static partial class Log
{
    [LogEvent(Level.Info, "Order {OrderId} placed by {UserId}")]
    public static partial void OrderPlaced(Guid orderId, Guid userId);
}

// Object introspection
logger.Inspect(complexObject); // auto-destructures to structured properties

// Potato debugging — logs caller info, breaks if debugger attached
SpudLog.Potato();
SpudLog.Potato(someVariable); // also snapshots the variable value
```

**Possible log levels (potato-themed):**

| Level | Potato | Use |
|-------|--------|-----|
| Trace | Seed | Extremely verbose |
| Debug | Sprout | Development diagnostics |
| Info | Tuber | Normal operation |
| Warning | Eyes | Something's growing wrong |
| Error | Blight | Operation failed |
| Critical | Famine | System-level failure |

## Open Questions

- Build on `Microsoft.Extensions.Logging` abstractions or standalone?
- WAL implementation: custom file-based or leverage SQLite?
- Introspection depth/cycle handling — max depth? `[SpudIgnore]` attribute?
- How far does the potato theming go? (joke vs. actually usable in commercial code)
- Package split: SpudLog.Core, SpudLog.Generator, SpudLog.Sinks.*?

## Prior Art / References

- Serilog (structured logging, sinks ecosystem)
- ZeroLog (zero-allocation approach)
- `Microsoft.Extensions.Logging` + `LoggerMessage.Define`
- Seq (structured log server / sink target)

## Outcome

_Pending_
