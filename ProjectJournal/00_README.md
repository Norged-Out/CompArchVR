# Project Journal

This folder is the repo-level source of truth for the dissertation project.

Its purpose is to preserve:
- project intent
- current scope
- progress history
- decisions made with the supervisor
- repo-specific working rules for future chat sessions

This should make the project resilient to:
- chat context loss
- daily credit limits
- future assistant handoffs
- "what were we doing again?" moments after a break

## File Map

- `01_PROGRESS_LOG.md`
  Chronological diary of work completed, current status, and next steps.

- `02_MASTER_EXECUTION_PLAN.md`
  The ideal end-to-end plan, target learning objectives, scope boundaries, and phased execution strategy.

- `03_GUIDELINES.md`
  Repo-specific instructions for future assistants working on this project.

- `04_PROJECT_SNAPSHOT.md`
  Human-facing snapshot for resuming work, orienting a new assistant, and checking the current project state.

- `05_DECISION_LOG.md`
  Key decisions, rationale, tradeoffs, and open questions that should survive across sessions.

## Update Policy

After any meaningful development or planning milestone, update these files when relevant:

- Always update `01_PROGRESS_LOG.md`
  Add what changed, what the current state is, and what should happen next.

- Update `05_DECISION_LOG.md` if scope, rationale, or major direction changed.

- Update `02_MASTER_EXECUTION_PLAN.md` only if the long-term plan or success criteria changed.

- Update `04_PROJECT_SNAPSHOT.md` if the most important resume information changed.

## Minimum Read Order For Future Sessions

Before doing meaningful work in a future chat, the assistant should read:

1. `ProjectJournal/00_README.md`
2. `ProjectJournal/02_MASTER_EXECUTION_PLAN.md`
3. `ProjectJournal/03_GUIDELINES.md`
4. `ProjectJournal/01_PROGRESS_LOG.md`
5. `ProjectJournal/05_DECISION_LOG.md`

## Working Rule Added On 2026-06-16

The user requested that after meaningful development sessions, the relevant journal files should be updated before wrapping up whenever practical.

This rule is now part of the repository working process.

## Current Resume Note

As of `2026-06-27`, the project has shifted further toward:
- scene-authored world-space UI under `Lesson Guide`
- a permanent scene-authored `Register Zone`
- a minimal reusable lesson loop that should be wired into authored scene objects instead of silently spawning layout in code
