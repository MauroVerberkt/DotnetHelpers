# DotnetHelpers — Project Instructions

## What This Is

Personal playground and professional portfolio by Mauro Verberkt. Code quality
is production-grade — packages and patterns from this repo regularly move into
production codebases. Treat contributions here with the same rigor as shipping
code.

This is NOT throwaway experimentation. Every public API surface, every naming
choice, every architectural decision is intentional and may end up in client
projects.

## Repository Layout

```
DotnetHelpers/
├── src/                        # NuGet package source code
│   ├── HelperMonads/           # Result<T> and Option<T> monadic types
│   ├── BusinessRules/          # Code-gen business rule framework
│   ├── BusinessRulesAnalyzer/  # Roslyn analyzer for business rules
│   ├── BusinessRulesGenerator/ # Roslyn source generator
│   ├── BusinessRulesFixProvider/ # Code fix provider
│   ├── BusinessRules.ResultExtensions/ # Bridge: BusinessRules ↔ Result
│   ├── BusinessRules.Wcf/      # WCF fault exception support
│   └── Railyard/               # (Railway-oriented programming utilities)
├── tests/                      # Test projects (unit + integration)
│   ├── HelperMonads.UnitTests/
│   ├── BusinessRules.UnitTests/
│   ├── BusinessRules.IntegrationTests/
│   ├── BusinessRules.ResultExtensions.UnitTests/
│   └── BusinessRules.Wcf.UnitTests/
├── tools/                      # Build scripts & NuGet packaging projects
│   ├── build-businessrules.ps1
│   ├── build-monads.ps1
│   ├── BusinessRules.Analyzers.Package/
│   └── HelperMonads.Package/
├── docs/                       # Docusaurus documentation site
│   ├── docs/                   # User-facing library docs
│   │   ├── getting-started/
│   │   ├── monads/
│   │   └── business-rules/
│   └── architecture/           # Architecture docs (separate sidebar)
│       ├── overview.md
│       ├── decisions/          # ADRs
│       ├── proposals/          # Design proposals
│       ├── design/             # Design documents
│       └── by-project/         # Per-project architecture
├── DotnetHelpers.sln           # Full solution
├── BusinessRules.slnf          # Solution filter: business rules only
├── Monads.slnf                 # Solution filter: monads only
└── Directory.Build.props       # Shared build properties
```

## Architecture Decision Records (ADRs)

- **Location:** `docs/architecture/decisions/`
- **Naming:** `NNN-kebab-title.md` (zero-padded to 3 digits)
- **Frontmatter:** `sidebar_position`, `title` as `"ADR-NNN: Title"`, `tags` by project
- **Statuses:** Accepted | Superseded | Deprecated
- **Purpose:** Record *why* a decision was made, not just what. ADRs are
  immutable once accepted — supersede rather than edit.

## Proposals

- **Location:** `docs/architecture/proposals/`
- **Template:** `_TEMPLATE.md` (always use this as starting point)
- **Naming:** `NNN-kebab-title.md` (zero-padded to 3 digits)
- **Lifecycle:** idea → exploring → ready → building → done | parked
- **Size:** small | medium | large | new-project
- **Completion:** When done/parked, fill the Outcome section and link to
  resulting ADR(s) if applicable.

Proposals are the thinking-out-loud space. They can be messy, incomplete, and
revised freely until they reach "done" status.

## Design Documents

- **Location:** `docs/architecture/design/`
- **Purpose:** Living documents describing how a system works today. Unlike
  ADRs (point-in-time decisions) or proposals (exploratory), these stay updated.

## Technical Stack & Conventions

- .NET 8.0+, C# 12+
- Functional patterns: Result, Option, monadic composition
- Source generators over runtime reflection
- Each NuGet package must be independently consumable
- Roslyn analyzers enforce business rule conventions at compile time

## Key Principles

1. **Explicit over implicit** — Result types over exceptions, source generators
   over reflection, strong typing over stringly-typed APIs
2. **Composability** — Operations chain via Map/Bind/Match; avoid side effects
   in core logic
3. **Package independence** — A consumer should be able to use HelperMonads
   without BusinessRules and vice versa
4. **Production-ready** — XML docs, nullable annotations, deterministic builds,
   proper packaging metadata

## Don'ts

- Don't add exception-based error handling to HelperMonads (use Result/Option)
- Don't merge proposal content into the decisions folder — proposals become
  ADRs through the lifecycle, they don't transform in place
- Don't add runtime reflection where a source generator can solve the problem
- Don't create cross-package dependencies unless explicitly designed (that's
  what the Extensions packages are for)
