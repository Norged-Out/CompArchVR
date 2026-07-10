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
- scene-authored `Instruction Decode UI` near the register area
- lesson UIs now use a three-panel structure:
  - lesson panel
  - interaction panel
  - hint / info panel
- guide panels have now been proven useful, but post-demo feedback suggests separating guide/explanation surfaces from active interaction surfaces more clearly
- scene-authored `Register Zone` with 32 permanent grabbable MIPS registers
- register scanners for `Read Register 1`, `Read Register 2`, and `Write Register`
- register tokens now support persistent logical values in code
- reusable custom register prefab and register materials under `Assets/MyPrefabs` and `Assets/MyMaterials`
- local register-bank reset button path separate from lesson reset
- local register-bank reset now restores only register poses and does not clear lesson progress, scanner success state, or emitted packets
- dedicated datapacket reset path now exists for loose, non-consumed packets
- authored register placement validation in the register zone
- authored lesson panels are now expected to be wired through serialized scene references, not found dynamically at runtime
- working MVP path:
  - start from `Intro UI`
  - present instruction / fetch framing
  - hand off to `Instruction Decode UI`
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
- authored memory/write-back path now exists:
  - a dedicated `Mem UI`
  - a dedicated `Memory Unit` prefab
  - a dedicated `Data Memory` bank prefab with 24 authored words
  - a dedicated `WB` prefab
  - a dedicated `WB UI`
  - a bonus loose register scanner for value inspection / confirmation
  - lesson-flow wiring for a real write-back phase instead of only a temporary intro-panel explanation
- authored lesson panel layout is now stabilized around edit-mode content plus code-triggered layout rebuilds
- `Instruction Decode`, `EX`, `Mem`, `WB`, and `PC Update` now follow the authored multi-panel direction, with code updated to support more scene-authored static/toggle text
- physical instruction fetch now exists through:
  - `Instruction Module`
  - `Instruction Terminal` in uploader mode
  - `Instruction Terminal` in downloader mode
  - decode staying locked until the uploaded module is delivered
- authored `PC Update UI` now exists as the final control-flow / lesson-conclusion surface
- first-pass `PC Update Station` support now exists for branch-resolution teaching
- the routed environment/map is now largely authored and validated by the supervisor:
  - elevated intro/fetch start space
  - decode room
  - ALU platform
  - memory hall
  - write-back area
  - PC-update conclusion platform
  - return path toward the ending gate
- first-pass value pipeline groundwork now exists in code:
  - register scanners can emit data packets
  - decode can now emit immediate packets from the second scanner spawn point for immediate-based instructions
  - immediate packets can now pass through a physical `Immediate Extender` before ALU use
  - ALU input scanners can accept those packets
  - ALU execution can compute `add` and the currently tested `addi` path in code
- authored ALU funct selection now exists through the UI dropdown used for the secondary instruction path
- instruction definitions now explicitly support:
  - initial register values
  - expected immediate values
  - write-back target resolution (`rd` vs `rt`)
  - immediate packets carrying a simple sign-extension boolean for now
- the full guided zone set is now present for:
  - `Instruction Fetch`
  - `Instruction Decode`
  - `EX`
  - `Mem`
  - `WB`
  - `PC Update`
- instruction assets now exist for:
  - `add`
  - `addi`
  - `lw`
  - `sw`
  - `sub`
  - `and`
  - `or`
  - `slt`
- `beq`
- `bne`
- `add`, `addi`, `lw`, `sw`, `sub`, `and`, `or`, `slt`, `beq`, and `bne` have now all been tested successfully through the current loop
- datapackets can now be manually recovered to their authored spawn points without disturbing active latched interactions
- `lw` now uses a real MIPS-style memory address path:
  - base register contains a data-segment address
  - immediate acts as offset
  - ALU result maps into the authored data-memory bank
- the `CpuLesson` area has now been fully refactored into:
  - `Flow`
  - `Support`
  - `UI`
  - shared UI helpers under `Assets/MyScripts/Shared/UI`
- the lesson system no longer depends on the older oversized `CpuLessonFlow.*` / `LessonGuideController.*` structure
- current lesson code is now explicitly organized around scene-authored bindings and smaller focused classes
- `Assets/MyScripts` has now had an additional pre-demo cleanup pass to remove obvious leftovers and improve code comments without changing the validated MVP flow

## Most Important Scripts Right Now

- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\CpuLessonFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\LessonLifecycle.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\LessonState.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\LessonStepActions.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\FetchFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\DecodeFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow\FlowProgress.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\LessonChecks.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\LessonPhaseRouter.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\InstructionCatalog.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\DecodeTextBuilder.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\DecodeHintBuilder.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\DecodeGuideFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\DecodeDropdownView.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\UI\LessonGuideController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\UI\LessonGuideView.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\UI\IntroPanelController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\UI\DecodePanelController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Shared\UI\LessonPanelBase.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBank.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankResetButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterScanner.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterScannerZone.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionEnums.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionDefaults.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionRuntimeSelection.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionFetch\InstructionModule.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionFetch\InstructionTerminal.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluExecutionController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluInputScanner.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluInputScannerZone.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluPacketTypes.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\DataPacketToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\DataPacketResetButton.cs`

## Historical Initial Rollout Order

1. `add`
2. `addi`
3. `lw`

Why:
- `add` teaches the cleanest register-register path
- `addi` adds immediate handling without memory complexity
- `lw` adds address calculation and memory read/write-back behavior

## Current Non-Negotiable Milestone

- the June 29, 2026 supervisor demo has been completed
- the post-map / post-branch supervisor checkpoint is now `2026-07-24`
- the official demo deadline is `2026-07-29`
- the current focus is no longer instruction coverage, but:
  - navigation clarity
  - door / gate progression
  - path guidance
  - tutorial/onboarding
  - UI readability
  - map lighting / route polish
  - sound cues
  - experiment-mode planning
  - overall presentation readiness

## If Starting A New Chat

Useful prompt:

`Please read ProjectJournal/00_README.md, 02_MASTER_EXECUTION_PLAN.md, 03_GUIDELINES.md, 01_PROGRESS_LOG.md, and 05_DECISION_LOG.md before suggesting or making changes.`

## If Credits Are Running Low

Before stopping:
- ask for the relevant journal files to be updated
- make sure `01_PROGRESS_LOG.md` reflects the current state
- make sure any major scope shift is written into `05_DECISION_LOG.md`

## Current Open Questions

- how strong the door + arrow + audio guidance should be without becoming visually noisy
- whether a persistent settings / cheatsheet panel should mirror all closed lesson information
- what the minimum viable experiment mode should hide, skip, or ungate
- how much of the current UI wording should move entirely into authored scene text versus stay runtime-driven
- whether instruction choice should remain user-selected, become randomized, or support both
- whether jump-family instructions are worth fitting in before the July 24 checkpoint
- how much onboarding belongs in a dedicated tutorial versus just-in-time prompts in the map
- how much post-meeting time should be reserved for participant prep instead of feature work

## Current Pre-Meeting Priorities

Ordered cutoff list before the next supervisor meeting:

1. door gating + arrow system
2. jump-family evaluation / inclusion only if safe
3. UI polish
4. map lighting and path tightening
5. proper tutorial UI, and possibly settings/cheatsheet support
6. sound pass
7. final polish
8. experiment mode before the checkpoint if feasible

Everything else should be treated as future work unless a blocking regression appears.

## Best Resume Point For The Next Development Session

The cleanest next work item is:
- finish the presentation/polish pass on top of the now-working instruction set
- keep the flow order fixed as:
  - fetch
  - decode
  - execute
  - memory when needed
  - write-back when needed
  - PC update when needed
- keep the branch / PC-update finish stable as the current lesson conclusion
- keep the current lesson UI layout approach:
  - authored in scene
  - updated by code
  - not generated at runtime
- add navigation clarity through:
  - physical doors/gates
  - arrow guidance
  - audio cues
- improve the environment without sacrificing lesson readability
- add experiment mode before the checkpoint if it can be done safely

## Personal Reminder

If the project feels too large, that is not a sign the concept is bad.
It is usually a sign that the lesson objective needs to be narrowed again.
