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
- lesson framework re-wired around `Lesson Guide`
- scene-authored `Lesson Guide` with a real `Intro UI` world-space panel
- scene-authored `Control Decode UI` used as the decode-phase gate between intro and registers
- scene-authored `Register Setup UI` near the register area
- scene-authored `Register Zone` with 32 permanent grabbable MIPS registers
- scene-authored `Control Unit` signal buttons for:
  - `RegDst`
  - `Branch`
  - `MemRead`
  - `MemtoReg`
  - `ALUOp`
  - `MemWrite`
  - `ALUSrc`
  - `RegWrite`
- register scanners for `Read Register 1`, `Read Register 2`, and `Write Register`
- reusable custom register prefab and register materials under `Assets/MyPrefabs` and `Assets/MyMaterials`
- local register-bank reset button path separate from lesson reset
- authored register placement validation in the register zone
- first-pass control decode validation now lives in `ControlDecodeController`:
  - button presses cycle signal values
  - `Control Decode UI` mirrors the current signal states
  - the panel action button checks the full signal combination against the active instruction
- authored lesson panels are now expected to be wired through serialized scene references, not found dynamically at runtime
- working MVP path:
  - start from `Intro UI`
  - present instruction / fetch framing
  - use `Control Decode UI` to set/check control signals
  - use one final continue press after a correct decode result
  - hand off to `Register Setup UI`
  - validate register placement through the authored scanners
- authored lesson panel layout for `Intro UI` and `Register Setup UI` has now been stabilized around edit-mode content plus code-triggered layout rebuilds
- planned later zone-specific lesson panels for `ALU`, control/decode, `Data Memory`, and `WriteBack`
- draft instruction assets for `add`, `addi`, and `lw`
- slim lesson scripts now reduced to:
  - `CpuLessonFlow`
  - `LessonGuideController`
  - `LessonChecks`
  - register bank / token / scanner scripts

## Most Important Scripts Right Now

- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\CpuLessonFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonGuideController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\ControlDecodeController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonChecks.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBank.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankResetButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterScanner.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterScannerZone.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionEnums.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefaults.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionRuntimeSelection.cs`

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
- how much of the intro/decode text should stay on `Intro UI` before handoff to later zone panels
- how much instruction decoding should be physical vs UI-driven
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
- branch from `main` into `ALU_V1`
- extend the now-working intro-to-register MVP:
- keep `Intro UI` and `Register Setup UI` authored in-scene
- keep `Control Decode UI` and the authored `Control Unit` buttons as the decode-phase baseline
- keep the flow order fixed as intro -> control decode -> register setup unless the user explicitly changes it
- keep the current lesson UI layout approach:
  - authored in scene
  - updated by code
  - not generated at runtime
- add the next authored lesson panel / interaction zone for `ALU`
- reuse the scanner / register / lesson state pattern for `addi` and `lw`

## Personal Reminder

If the project feels too large, that is not a sign the concept is bad.
It is usually a sign that the lesson objective needs to be narrowed again.
