---
sidebar_position: 1
title: "PROP-001: Async Helper Library (Goroutines-style)"
tags: [NewProject]
---

# PROP-001: Async Helper Library (Goroutines-style)

**Status:** idea  
**Size:** new-project  
**Created:** 2026-05-25  

## Problem / Motivation

.NET's async/await is powerful but low-level for concurrent workflows. Go's goroutines + channels pattern makes it trivial to spin up concurrent work, communicate between tasks, and handle cancellation. .NET lacks a lightweight, opinionated abstraction over `Task`, `Channel<T>`, and structured concurrency.

## Sketch

A `HelperAsync` package providing:

- **Goroutine-style dispatch**: `Go.Run(async () => ...)` — fire-and-forget with structured lifetime (tied to a scope/parent)
- **Typed channels**: `var ch = Channel.Create<int>()` with `Send`/`Receive` that feel like Go channels
- **Select**: `await Select.Case(ch1, ...).Case(ch2, ...).Run()` for multiplexing
- **Scoped concurrency**: `await using var scope = Go.Scope()` — child tasks cancel when scope disposes (like errgroup)

```csharp
await using var scope = Go.Scope();

var results = Channel.Create<int>(capacity: 10);

scope.Go(async ct => {
    await foreach (var item in source.ReadAllAsync(ct))
        await results.SendAsync(item * 2, ct);
    results.Complete();
});

await foreach (var r in results.ReadAllAsync())
    Console.WriteLine(r);
```

## Open Questions

- Build on top of `System.Threading.Channels` or custom?
- How to handle exception propagation across goroutines?
- Does `Result<T>` integrate (channel of results)?
- Naming: HelperAsync? HelperConcurrency? HelperChannels?

## Prior Art / References

- Go goroutines + channels
- Kotlin coroutines + structured concurrency
- `System.Threading.Channels` (low-level building block)
- `Open.ChannelExtensions` NuGet package

## Outcome

_Pending_
