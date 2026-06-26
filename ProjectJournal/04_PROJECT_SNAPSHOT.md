# Project Snapshot

This file is meant to help the project owner quickly resume work after breaks, chat resets, or low-credit days.

## What This Project Is Trying To Be

A focused VR learning experience for computer architecture, centered on helping learners reason through single-cycle MIPS datapath execution more effectively than static diagrams alone.

## Current Agreed Direction

The project should:
- answer the "why VR?" question through guided spatial reasoning
- focus on one learning objective first
- use VR for meaningful physical/spatial choices
- use UI for lookup tables, prompts, and explanations

## Current Active Prototype Area

Scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype features:
- lesson framework present, but not intended to auto-start during ordinary scene iteration
- explicit node highlighting by datapath id
- refactored lesson runtime split across small helper scripts
- runtime world-space lesson UI fallback
- scene-authored `Register Bank` with 32 permanent grabbable MIPS registers
- reusable custom register prefab and register materials under `Assets/MyPrefabs` and `Assets/MyMaterials`
- local register-bank reset button path separate from lesson reset
- planned `Execution` / `WriteBack` pedestal-zones for physical register placement validation
- draft instruction assets for `add`, `addi`, and `lw`

## Most Important Scripts Right Now

- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuNodeSequenceController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\CpuLessonFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\NodeMap.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonSetup.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonUI.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBank.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankAuthoring.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankResetButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonChecks.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefaults.cs`

## Recommended Development Order

1. `add`
2. `addi`
3. `lw`

Why:
- `add` teaches the cleanest register-register path
- `addi` adds immediate handling without memory complexity
- `lw` adds address calculation and memory read/write-back behavior

## Current Non-Negotiable Milestone

- by `2026-06-28`, `add`, `addi`, and `lw` should be working
- on `2026-06-29`, this should be ready to show the supervisor as a V1 checkpoint
- this is the current deadline target, not a promise that the whole dissertation will stop at those three instructions

## If Starting A New Chat

Useful prompt:

`Please read ProjectJournal/00_README.md, 02_MASTER_EXECUTION_PLAN.md, 03_GUIDELINES.md, 01_PROGRESS_LOG.md, and 05_DECISION_LOG.md before suggesting or making changes.`

## If Credits Are Running Low

Before stopping:
- ask for the relevant journal files to be updated
- make sure `01_PROGRESS_LOG.md` reflects the current state
- make sure any major scope shift is written into `05_DECISION_LOG.md`

## Current Open Questions

- how readable and comfortable the current lesson UI and register bank feel in-headset
- which remaining elements should become scene-authored instead of runtime-generated
- how much of instruction decode should be physical vs UI-driven
- how explicit the control-signal interactions should be in the first version
- whether instruction choice is user-selected, randomized, or both
- the exact acceptance rule for pedestal validation:
  - immediate on release
  - after 1-2 seconds stable in-zone
  - or some combination of both
- how pedestal feedback should distinguish:
  - wrong register
  - right register
  - right register but wrong phase

## Best Resume Point For The Next Development Session

The cleanest next work item is:
- co-author any remaining spacing or readability changes to the saved register bank in-scene with the user
- validate and polish the playable `add` lesson in-headset
- author pedestal zones for the `ALU` / `Execution` phase and later `WriteBack`
- make each pedestal scan the placed register token and validate it only for the active lesson step
- then extend the same framework into `addi`

## Personal Reminder

If the project feels too large, that is not a sign the concept is bad.
It is usually a sign that the lesson objective needs to be narrowed again.
