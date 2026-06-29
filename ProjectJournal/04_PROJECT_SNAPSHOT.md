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
- register tokens now support persistent logical values in code
- reusable custom register prefab and register materials under `Assets/MyPrefabs` and `Assets/MyMaterials`
- local register-bank reset button path separate from lesson reset
- local register-bank reset now restores only register poses and does not clear lesson progress, scanner success state, or emitted packets
- authored register placement validation in the register zone
- authored lesson panels are now expected to be wired through serialized scene references, not found dynamically at runtime
- working MVP path:
  - start from `Intro UI`
  - present instruction / fetch framing
  - hand off to `Register Setup UI`
  - show instruction breakdown during decode
  - validate source-register placement through the authored scanners
  - spawn data packets from decode for later execution
- authored `ALU` execution pass now exists:
  - physical `ALUOp` and `ALUSrc` buttons on the ALU prefab
  - authored `ALU UI` for execution validation
  - ALU input trigger zones that accept datapackets
  - input 2 role switching based on `ALUSrc`
  - result packet spawning with role `ALU Result`
  - one extra continue click after success before write-back
- authored lesson panel layout for `Intro UI` and `Register Setup UI` has now been stabilized around edit-mode content plus code-triggered layout rebuilds
- first-pass value pipeline groundwork now exists in code:
  - register scanners can emit data packets
  - decode can now emit immediate packets from the second scanner spawn point for immediate-based instructions
  - ALU input scanners can accept those packets
  - ALU execution can compute an `add` result in code
- instruction definitions now explicitly support:
  - initial register values
  - expected immediate values
  - write-back target resolution (`rd` vs `rt`)
- planned later zone-specific lesson panels for `ALU`, control/decode, `Data Memory`, and `WriteBack`
- draft instruction assets for `add`, `addi`, and `lw`
- slim lesson scripts now reduced to:
  - `CpuLessonFlow`
  - `LessonGuideController`
  - `LessonChecks`
  - register bank / token / scanner scripts
  - ALU packet / scanner / controller scripts
- `Assets/MyScripts` has now had an additional pre-demo cleanup pass to remove obvious leftovers and improve code comments without changing the validated MVP flow

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
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionEnums.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionDefaults.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionRuntimeSelection.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluExecutionController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluInputScanner.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluInputScannerZone.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluPacketTypes.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\DataPacketToken.cs`

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
- exactly where `RegDst` and `ALUSrc` should live pedagogically:
  - both in their strict hardware-derivation sense
  - and in the simplified lesson flow sense
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
- finish the dedicated write-back phase on top of the current mainline snapshot:
- keep `Intro UI` and `Register Setup UI` authored in-scene
- keep the flow order fixed as intro -> instruction decode -> ALU -> write-back unless the user explicitly changes it
- keep the current lesson UI layout approach:
  - authored in scene
  - updated by code
  - not generated at runtime
- author the next physical interaction layer for `WriteBack`
- reuse the scanner / register / lesson state pattern for `addi` and `lw`

## Personal Reminder

If the project feels too large, that is not a sign the concept is bad.
It is usually a sign that the lesson objective needs to be narrowed again.
