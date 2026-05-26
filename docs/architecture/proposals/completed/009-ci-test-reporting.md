---
sidebar_position: 9
title: "PROP-009: CI Pipeline & Live Test Reporting"
tags: [infra, testing]
---

# PROP-009: CI Pipeline & Live Test Reporting

**Status:** done  
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
PR → build → run tests → collect coverage → publish report → update badges
push to main (post-merge) → same + deploy artifacts + update badges on default branch
```

### Chosen Components

1. **CI Provider** — GitHub Actions (free for public repos)
2. **Test Execution** — `dotnet test` across all test projects
3. **Coverage Collection** — Coverlet, output both Cobertura (for Codecov) and OpenCover (for ReportGenerator if ever needed). No cost to producing both.
4. **Coverage Reporting** — Codecov (MVP). May run parallel with self-hosted dashboard later (PROP-008 stretch goal); they serve different purposes (Codecov for PR diff feedback, self-hosted for unified portal).
5. **Artifact Storage** — `test-summary.json` published to `gh-pages` branch (persistent, accessible to Docusaurus build, no expiry unlike Actions artifacts)

### CI Job Structure

Two logical jobs, triggered by path filters:

| Trigger | Jobs that run |
|---------|--------------|
| PR with code changes | `build-and-test` + `docs-validate` |
| PR with docs only | `docs-validate` only |
| Push to main (post-merge) | `build-and-test` + `docs-validate` + badge/artifact updates |

Code PRs always validate docs too, since code changes can affect API docs or generated content.

### Nice-to-Haves (Post-MVP)

- **Test result reporting** (not just coverage) — which tests passed/failed, duration trends
- **PR annotations** — inline coverage gaps on changed files
- **Coverage badges** — embed in README and docs site
- **SonarCloud** — phase 2, layered on top of base pipeline

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
on:
  pull_request:
  push:
    branches: [main]  # post-merge runs for badge/artifact updates

jobs:
  build-and-test:
    if: # path filter — skip when only docs/** changed (on PRs)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
      - run: dotnet build
      - run: dotnet test --collect:"XPlat Code Coverage"
      - # upload coverage to Codecov
      - # generate test-summary.json
      - # publish test-summary.json to gh-pages branch

  docs-validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - # npm ci && npm run build (validate docs build)
      - # deploy step placeholder (owned by PROP-008)
```

## Decisions

- **Public repo** — going public, so free-for-OSS tiers of Codecov/SonarCloud are available.
- **Coverage threshold** — no hard gate initially. Measure baseline first, then ratchet. Want the ability to switch between coverage metrics (line, branch, method) in the reporting UI.
- **Independent from PROP-008** — CI is higher priority and has no dependency on docs hosting.
- **Static analysis (SonarCloud)** — not in MVP, but a strong candidate for phase 2. Provides reviewer-like feedback (code smells, complexity, security hotspots) which is valuable given no human reviewers. Layer in after the base pipeline is working.
- **Multi-TFM** — deferred to PROP-010. For now, test on net8.0 only.
- **Analyzer/generator coverage** — yes, include these in CI. They're the most likely to break silently since they run at compile-time in consumers' builds.
- **Machine-readable output** — CI publishes a `test-summary.json` artifact (coverage %, test count, pass/fail, timestamp) to the `gh-pages` branch alongside the Codecov upload. Persistent, no expiry, and directly consumable by the Docusaurus build (PROP-008 stretch goal).
- **Branch protection** — require PRs for all changes to main, no exceptions. CI uses path filters to determine which jobs run: docs-only PRs skip build/test and only run docs validation. No direct-push bypass for docs.
- **Code PRs validate docs too** — since code changes can affect API docs or generated content, the `docs-validate` job runs on all PRs regardless of path.
- **Trigger model** — CI runs on both `pull_request` (for gating) and `push` to main (post-merge, for badge updates and artifact publishing on the default branch).
- **Docs deploy** — CI validates the docs build (catches broken MDX/links). Actual deployment is PROP-008's responsibility; placeholder step in workflow for now.
- **Codecov is long-term** — even if a self-hosted dashboard (PROP-008 stretch) is built later, Codecov stays for PR diff coverage feedback. They serve different purposes.

- **Coverlet output format** — both Cobertura and OpenCover. Cobertura goes to Codecov, OpenCover is available for ReportGenerator if self-hosted reports are ever needed. No cost to producing both.

## Open Questions

_None remaining — ready to build._

## Prior Art / References

- [Coverlet documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [Codecov GitHub Action](https://github.com/codecov/codecov-action)
- [SonarCloud .NET guide](https://docs.sonarsource.com/sonarcloud/advanced-setup/languages/dotnet/)
- [Allure Report](https://allurereport.org/)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/use-cases-and-examples/building-and-testing/building-and-testing-net)

## Outcome

Implemented 2025-05-26. Core MVP delivered:

- `.github/workflows/ci.yml` — orchestrator with path filtering via `dorny/paths-filter`
- `.github/workflows/_build-and-test.yml` — reusable workflow: restore → build → test → Coverlet (cobertura+opencover) → Codecov coverage + test results upload → test-summary.json artifact
- `.github/workflows/_docs-validate.yml` — reusable workflow: npm ci → Docusaurus build validation
- `.github/ruleset-protect-main.json` — branch protection ruleset (PRs required, status checks enforced)
- `tests/Directory.Build.props` — JunitXml.TestLogger for all test projects
- Dead local NuGet source removed from `nuget.config`

**Stretch goals remaining** (spawn new proposals when pursued):
- SonarCloud integration (phase 2 static analysis)
- PR annotations for coverage gaps
- Coverage badges in README/docs
- Live Developer Portal (benchmarks, dashboards, branch comparison)
