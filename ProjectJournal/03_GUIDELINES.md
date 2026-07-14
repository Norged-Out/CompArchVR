# Guidelines

These instructions are for future assistants working on this repository.

## Read First

Before doing meaningful work, read:

1. `ProjectJournal/00_README.md`
2. `ProjectJournal/02_MASTER_EXECUTION_PLAN.md`
3. `ProjectJournal/01_PROGRESS_LOG.md`
4. `ProjectJournal/05_DECISION_LOG.md`

Do not assume the current chat alone contains enough project context.

## Project Intent

This is a dissertation project about teaching computer architecture through VR.

The project is not meant to become a giant universal simulator.
It should prioritize one focused learning objective done well.

## Current Prototype Scene Rules

Active prototype scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Important scene rule:
- prototype work belongs in `Testing Ground` unless the user explicitly redirects elsewhere

## Unity / Asset Rules

Do not directly modify source prefabs if avoidable.

Acceptable:
- place prefab instances into the scene
- unpack scene instances if they need scene-specific edits

Preferred script location:
- `D:\CompArchVR\ThePrototype\Assets\MyScripts`

Reserved project folders currently in use:
- `Assets/MyScripts`
- `Assets/MyPrefabs`

## Scene Collaboration Rule

If scene placement, readability, spacing, or general visual feel matters:
- do not silently solve it by generating everything in code
- tell the user what needs to exist in-scene
- let the user help place and eyeball it inside Unity
- if the user has recently edited the scene, rescan `Testing Ground` before assuming older hierarchy or components still exist

Runtime-generated scene objects are acceptable only as:
- temporary fallback scaffolding
- emergency prototyping glue when Unity is not open

They are not the preferred final workflow for layout-heavy scene elements.

For the current lesson framework, also prefer:
- serialized Inspector references to authored scene objects
- explicit scene wiring for buttons, panels, scanners, and banks

Avoid:
- `Find*`-style scene searches for core lesson objects
- name-based runtime hookup when the user can wire the scene directly in edit mode

## Design Rules

Prefer:
- meaningful learner decisions
- spatial reasoning
- guided stage progression
- local correctness feedback
- stable, explainable interactions

Avoid:
- turning every low-level hardware detail into manual busywork
- overloading one lesson with too many simultaneous learning goals
- making polish/animation a prerequisite for understanding

Use VR for:
- selecting registers
- choosing datapath routes
- operating mux-like gateways
- embodied stage progression

Use UI for:
- lookup tables
- instruction format reminders
- prompts and explanations
- control references

## Architecture Rules

The current `InstructionSystem` scripts are scaffolding, not final dogma.

Do not scrap them casually.
Instead:
- preserve useful data model ideas
- refactor toward a stage-driven lesson system
- introduce new data structures when the lesson design clearly needs them

Do not allow one script to become a dumping ground for:
- lesson state
- scene setup
- UI creation
- validation logic
- runtime button construction

Prefer several focused scripts over one bloated controller.

For the current `Testing Ground` lesson path:
- `LessonGuideController` should stay focused on authored panel flow
- `CpuLessonFlow` should stay focused on lesson-step state and validation
- phase-specific controllers should stay focused on their own station logic:
  - ALU
  - memory
  - write-back
  - PC update

Also:
- if older scene assumptions are no longer true, refactor or delete the dead code instead of layering more hacks on top

## Documentation Update Rule

After meaningful work, update documentation when relevant:

- always update `ProjectJournal/01_PROGRESS_LOG.md`
- update `ProjectJournal/05_DECISION_LOG.md` if direction or rationale changed
- update `ProjectJournal/02_MASTER_EXECUTION_PLAN.md` only if the long-term plan changed
- update `ProjectJournal/04_PROJECT_SNAPSHOT.md` if resume-critical information changed

This rule exists because the user explicitly wants persistent project memory inside the repo.

When updating journals:
- keep old milestone information if it is still useful for the dissertation or presentation
- but rewrite top-level "current state" sections so they reflect the live project, not an older checkpoint
- clearly separate:
  - current authoritative status
  - historical milestone/archive notes
- if a system moved from TODO to working baseline, update the active priority lists to reflect that instead of leaving stale TODO wording behind

## Current Deadline Rule

The active deadline framing is now:
- next supervisor meeting target: `2026-07-24`
- official project/demo deadline: `2026-07-29`
- preferred internal finish target:
  - complete the build a few days early
  - use the remaining time for polish, participant prep, and presentation safety

Current short-term priorities:
- tutorial / onboarding flow
- sound-guided navigation / feedback
- experiment mode before the meeting if safely possible
- settings refinement only if it clearly helps usability/presentation
- jump-family work only if the polish path is already safe
- optional helper NPC / in-world music only if clearly beneficial and low-risk

Current live-state reminder:
- a baseline player settings menu now exists
- future journal updates should describe further menu work as refinement/expansion unless that baseline is intentionally removed or replaced
- the map revamp / better route baseline should not keep being written up as an unfinished headline task unless new testing exposes a real readability problem
- a separate cheatsheet is no longer the preferred assumption; current planning expects the hint panels to carry most of that explanatory burden
- tutorial planning may legitimately discuss both:
  - a custom recorded video path
  - a coaching-card / image-and-text path

Do not casually re-scope away from presentation readiness unless the user explicitly changes direction.

## Commit Style Rule

The user prefers commit messages that sound like natural completed actions.

Examples:
- `Added project`
- `Updated gitignore`
- `Initial setup for cpu flow + instruction set`

Avoid terse imperative styles like:
- `Update ...`
- `Add ...`

unless the user explicitly asks otherwise.

## Communication Rule

Be honest about scope and risk.
Do not oversell grand plans if the current implementation path is too ambitious.

The user responds well to:
- concrete reasoning
- narrow scoping
- stable architecture
- strong memory of earlier repo constraints

## If Context Is Thin

When unsure what to do next:

1. inspect `ProjectJournal/01_PROGRESS_LOG.md`
2. inspect `ProjectJournal/05_DECISION_LOG.md`
3. identify the next smallest meaningful lesson milestone
4. avoid speculative refactors with no immediate payoff
