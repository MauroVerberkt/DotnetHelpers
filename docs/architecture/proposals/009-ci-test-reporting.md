---
sidebar_position: 9
title: "PROP-009: CI Pipeline & Live Test Reporting"
tags: [infra, testing]
---

# PROP-009: CI Pipeline & Live Test Reporting

**Status:** idea  
**Size:** medium  
**Created:** 2025-05-25  

## Problem / Motivation

There is currently no CI/CD pipeline for this repository. Tests run locally only, and there's no visibility into test health, code coverage trends, or regression history. For a project that aims to be production-grade and portfolio-worthy, a publicly visible test report with coverage metrics would:

- Demonstrate code quality to consumers and employers
- Catch regressions automatically on every push/PR
- Provide coverage badges for the README and docs site
- Enable confident refactoring with immediate feedback

This proposal is broader than just reporting — it encompasses the full pipeline story: automated builds, test execution, coverage collection, and hosted reporting.

## Sketch

### Pipeline Scope

```
push/PR → build → run tests → collect coverage → publish report → update badges
```

### Components to Investigate

1. **CI Provider** — GitHub Actions (already on GitHub, free for public repos)
2. **Test Execution** — `dotnet test` across all test projects
3. **Coverage Collection** — Coverlet (already standard in .NET), output as Cobertura XML or OpenCover
4. **Report Generation** — ReportGenerator, or a hosted service that consumes raw coverage data
5. **Hosted Report Dashboard** — Where the live report lives publicly

### Hosting Options for Reports

| Option | Pros | Cons |
|--------|------|------|
| **Codecov** | Free for OSS, PR comments with diff coverage, historical trends | Third-party dependency |
| **Coveralls** | Free for OSS, badge support, GitHub integration | Less active development |
| **GitHub Pages (self-hosted)** | Full control, ReportGenerator HTML output | More setup, no trend tracking OOTB |
| **SonarCloud** | Free for OSS, code quality + coverage + security, very comprehensive | Heavier setup, opinionated |
| **Allure Report** | Beautiful test reports, history tracking, hosted via GitHub Pages | Primarily test results, coverage is secondary |

### Nice-to-Haves

- **Test result reporting** (not just coverage) — which tests passed/failed, duration trends
- **PR annotations** — inline coverage gaps on changed files
- **Coverage badges** — embed in README and docs site
- **Matrix builds** — test across multiple .NET versions if packages support netstandard2.0+
- **Branch protection** — require passing tests + minimum coverage on PRs

### Stretch Goal: Live Developer Portal

Inspiration: companies running TDD at scale often have a live portal that goes far beyond simple coverage badges. Think a unified dashboard showing:

- **Test results per branch** — not just main, but every active feature branch's health at a glance
- **Coverage trends over time** — regressions visible before they're merged
- **Benchmark history** — performance tracking with trend charts (BenchmarkDotNet results over commits)
- **Infrastructure/build health** — build times, flaky test detection, deployment status
- **Branch comparison** — diff coverage between branches, which branch is healthiest

This is the "big vision" end state. Tools in this space:

| Tool | What it does |
|------|-------------|
| **Grafana** | Custom dashboards pulling CI data, test metrics, benchmarks |
| **Backstage** (Spotify) | Developer portal: service catalog, CI status, docs, all unified |
| **BenchmarkDotNet + GitHub Pages** | .NET benchmark history with trend charts per commit |
| **BuildPulse / Launchable** | Test intelligence — flaky detection, test prioritization |
| **Custom static site** | Pull GitHub Actions API data into a lightweight dashboard |

An incremental path toward this:
1. MVP: CI + coverage + Codecov (this proposal's core)
2. Add BenchmarkDotNet runs, publish results to GitHub Pages
3. Add a dashboard page aggregating test/coverage/benchmark data across branches
4. Grow into a full portal as the project warrants it

This doubles as a portfolio piece — linking employers to a live dashboard showing TDD discipline, benchmark history, and build health tells a story that a README badge alone cannot.

### Rough GitHub Actions Structure

```yaml
# .github/workflows/ci.yml
on: [push, pull_request]
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet build
      - run: dotnet test --collect:"XPlat Code Coverage"
      - # upload coverage to chosen provider
      - # generate and deploy report
```

## Open Questions

- Is the repo going public, or do we need solutions that work for private repos too?
- Minimum coverage threshold to enforce (e.g., 80%)?
- Should this block on PROP-008 (docs hosting), or are they independent deployments?
- Do we want full static analysis (SonarCloud) or just coverage?
- Test across multiple TFMs, or just net8.0?
- Should the Roslyn analyzer/generator projects have integration test coverage too?

## Prior Art / References

- [Coverlet documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [Codecov GitHub Action](https://github.com/codecov/codecov-action)
- [SonarCloud .NET guide](https://docs.sonarsource.com/sonarcloud/advanced-setup/languages/dotnet/)
- [Allure Report](https://allurereport.org/)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/use-cases-and-examples/building-and-testing/building-and-testing-net)

## Outcome

_Filled when status changes to done/parked. Link to ADR(s) if applicable._
