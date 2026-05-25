---
sidebar_position: 3
title: "ADR-003: Separate WCF Package"
tags: [BusinessRules]
---

# ADR-003: Separate WCF Package

**Status:** Accepted

## Context

The original `BusinessRules` library included WCF support (`ToFaultException()`, `BusinessRuleFault`) directly. This forced all consumers to take a dependency on `System.ServiceModel.Primitives`, even if they had no WCF services.

WCF is a legacy technology that most new .NET projects don't use. Bundling it into the core library:
- Increases package size for all consumers
- Pulls in unnecessary transitive dependencies
- Signals that WCF is a primary concern rather than optional

## Decision

Extract all WCF-related types and methods into a separate `BusinessRules.Wcf` package:

- `BusinessRuleFault` record moves to `BusinessRules.Wcf` namespace
- `ToFaultException()` becomes an extension method in `BusinessRuleWcfExtensions`
- Core `BusinessRules` library retains zero external dependencies

Consumers using WCF explicitly opt in by referencing `BusinessRules.Wcf`.

## Consequences

**Positive:**
- Core library is dependency-free and lightweight
- New users don't inherit legacy technology assumptions
- WCF support is opt-in, not forced
- Follows the same extension pattern as `BusinessRules.ResultExtensions`
- Clean package graph: consumers only pay for what they use

**Negative:**
- Breaking change for existing code using `ToFaultException()` from the core library
- Requires `InternalsVisibleTo` for the Wcf package to access internal properties
- Additional package to manage and version
- Migration guide needed (namespace change + new reference)
