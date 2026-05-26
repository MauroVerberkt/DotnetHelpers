---
sidebar_position: 8
title: "PROP-008: Docusaurus Hosting"
tags: [docs]
---

# PROP-008: Docusaurus Hosting

**Status:** idea  
**Size:** small  
**Created:** 2025-05-25  
**Blocked by:** PROP-009 (CI pipeline must exist before docs deploy can hook into it)

## Problem / Motivation

The Docusaurus documentation site currently only runs locally during development. To make the documentation accessible to consumers of the NuGet packages (and as a professional portfolio piece), the site needs to be hosted somewhere publicly accessible.

Key considerations:
- Cost (ideally free or near-free for an open-source/personal project)
- Custom domain support
- Automated deployments (CI/CD on push to main)
- SSL/HTTPS out of the box
- Minimal maintenance overhead

## Sketch

### Hosting Options to Investigate

1. **GitHub Pages** — Free, native GitHub Actions integration, custom domain support. Docusaurus has first-class support via `docusaurus deploy`.
2. **Vercel** — Free tier for personal projects, automatic preview deployments on PRs, excellent performance.
3. **Netlify** — Free tier, PR previews, form handling if ever needed, redirect rules.
4. **Cloudflare Pages** — Free tier, global CDN, fast builds, custom domains with Cloudflare DNS.
5. **Azure Static Web Apps** — Free tier, GitHub Actions integration, suits the .NET ecosystem branding.

### Evaluation Criteria

| Criterion | Weight |
|-----------|--------|
| Cost (free tier) | High |
| CI/CD integration | High |
| Custom domain + HTTPS | High |
| PR preview deployments | Medium |
| Build speed | Low |
| Vendor lock-in | Low |

### Deployment Workflow (Generic)

```
push to main → CI builds Docusaurus → deploy to hosting provider
PR opened → CI builds Docusaurus → deploy preview (optional)
```

## Open Questions

- Do we want PR preview deployments, or is local preview sufficient?
- Is a custom domain planned, or is `*.github.io` / `*.vercel.app` acceptable?
- Should the docs site be in the same repo (monorepo) or split out?
- Any preference for a provider that aligns with the .NET ecosystem (e.g., Azure)?

## Stretch Goal: Integrated Project Health Dashboard

Once PROP-009 (CI pipeline) is producing test/coverage data, the docs site can surface that data directly rather than just linking to external tools.

### Option A: Embedded Health Component

A custom React component within the existing Docusaurus site that displays a summary of project health:

- Coverage %, trend sparkline, test count, last green build
- Rendered on the docs landing page or a dedicated "Project Health" page
- "View full report →" link to Codecov/SonarCloud for drill-down

**Data flow:**
```
CI run → publishes test-summary.json artifact
Docusaurus build → fetches artifact at build time (no CORS, no client-side fetch)
CI triggers docs redeploy after test data updates → always in sync
```

This keeps the docs site as the single entry point while leveraging external tools for the heavy lifting. Low effort, high portfolio value.

### Option B: Full Dashboard Page

A dedicated `/dashboard` route in Docusaurus with multiple panels:

- Coverage breakdown per project
- Benchmark history (BenchmarkDotNet trend charts)
- Build health (duration, flaky test detection)
- Branch comparison

Essentially a lightweight Grafana alternative built as Docusaurus pages. More impressive as a portfolio piece but significantly more work — likely a project in itself.

### Shared Dependency

Both options require PROP-009 to publish a machine-readable summary artifact (e.g., `test-summary.json`) as part of CI output. That's PROP-009's interface contract to this stretch goal.

## Prior Art / References

- [Docusaurus Deployment docs](https://docusaurus.io/docs/deployment)
- [GitHub Pages guide for Docusaurus](https://docusaurus.io/docs/deployment#deploying-to-github-pages)
- [Vercel Docusaurus template](https://vercel.com/templates/docusaurus)
- [Netlify Docusaurus guide](https://docs.netlify.com/frameworks/docusaurus/)

## Outcome

_Filled when status changes to done/parked. Link to ADR(s) if applicable._
