---
name: superstorage-progress
description: "Track implementation progress for the SuperStorage project. Use whenever Codex makes direct code changes, adds or removes project files, changes implementation-related documentation, finishes a feature slice, or needs to resume work from the current project state. The skill requires reading the progress tracker before edits, keeping status docs aligned after work, and asking whether the feature is Done, Partial, or Needs Fix before moving on."
---

# SuperStorage Progress

## Purpose

Keep SuperStorage's implementation state explicit and recoverable across sessions.

Use this skill as the project memory layer whenever making code changes or implementation-impacting documentation changes.

## Core Files

- `Doc/10-progress-tracker.md`: source of truth for current progress, decisions, feature status, next steps, and open questions.
- `Doc/09-piano-sviluppo.md`: roadmap and long-range development plan.
- `Doc/`: architecture, security, database, UI, test, and operations documentation.

## Workflow

1. Read `Doc/10-progress-tracker.md` before making direct code changes.
2. Read only the relevant nearby docs for the requested area, such as auth, EF Core, architecture, UI, or tests.
3. Implement the user request using the normal project skills and coding standards.
4. Run the narrowest useful verification for the change, usually `dotnet build Dev.slnx` for cross-project changes or focused tests when available.
5. Update `Doc/10-progress-tracker.md` when the change affects feature status, project decisions, next steps, or known gaps.
6. If the roadmap changed materially, update `Doc/09-piano-sviluppo.md` too.
7. In the final response, state whether the tracker was updated and ask the feature-status question.

## Feature Status Question

When a feature slice has been implemented or materially advanced, end with:

```text
Consideriamo questa feature Done, Partial o Needs Fix?
Manca qualcosa prima di passare alla prossima?
```

Do not ask this for pure explanations, quick terminal checks, or documentation-only clarifications unless the user is explicitly closing a feature.

## Status Vocabulary

Use these statuses consistently:

- `Done`: implemented, verified, and smoke-tested enough for the current phase.
- `Partial`: useful progress exists, but UI, API, tests, hardening, docs, or edge cases remain.
- `Needs Fix`: implementation exists but has a known defect, design concern, or failing verification.
- `Planned`: not started.

## Tracker Update Rules

Update the tracker when:

- a new feature, endpoint, aggregate, behavior, UI page, migration, or integration is added;
- a feature moves between `Planned`, `Partial`, `Needs Fix`, and `Done`;
- a new architectural decision is made;
- a known bug, test gap, or production-hardening task is discovered;
- the recommended next step changes.

Keep updates concise. Prefer changing the relevant row or bullet instead of adding long diary entries.

## Final Response Checklist

Mention:

- files or areas changed;
- verification performed, or why it was not run;
- whether `Doc/10-progress-tracker.md` was updated;
- the feature-status question when appropriate.

## Project Standards Reminder

Honor the existing SuperStorage conventions:

- prefer `is null` and `is not null` for object null checks;
- use async/await where possible;
- forward `CancellationToken`;
- keep code comments in English;
- preserve Clean Architecture boundaries;
- avoid reverting unrelated user changes.
