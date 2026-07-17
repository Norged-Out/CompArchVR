# Project Snapshot

This file is meant to help me quickly resume work after breaks, session resets, or interrupted work periods.

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
- scene-authored lesson flow built around `Lesson Guide`
- lesson-mode groundwork for:
  - `Learning`
  - `Practice`
- scene-authored world-space UIs for:
  - `Intro UI`
  - `Instruction Decode UI`
  - `ALU UI`
  - `Mem UI`
  - `WB UI`
  - `PC Update UI`
- three-panel lesson layout is now the standard interaction format outside Intro:
  - lesson panel
  - interaction panel
  - hint / info panel
- physical instruction fetch through:
  - `Instruction Module`
  - uploader `Instruction Terminal`
  - downloader `Instruction Terminal`
- scene-authored `Register Zone` with:
  - 32 permanent grabbable MIPS registers
  - authored sample values across all 32 registers
  - decode-stage scanners
  - local pose-only reset
  - authored-value restore on mode change
- datapacket flow for:
  - read data
  - immediate
  - ALU result
  - memory data
  - zero result for branch resolution
- physical `Immediate Extender`
- authored `ALU`, `Memory Unit`, `Data Memory`, `WB`, and `PC Update Station`
- dedicated datapacket reset for loose packets
- routed map with:
  - intro/fetch start space
  - decode room
  - ALU platform
  - memory hall
  - write-back area
  - PC-update conclusion platform
  - gated return toward the end
- moving gate system for phase progression
- route guidance baseline with authored arrows and gate-controlled progression
- player-facing wrist settings menu with:
  - current instruction text
  - current phase text
  - register reset
  - datapacket reset
  - lesson restart
  - volume slider
  - route-guidance toggle
  - FPS / frame-time readout
  - quit support
- left-controller menu-button access layered onto the existing XR hand-menu approach
- onboarding groundwork through:
  - the imported tutorial/video prefab from `VRTemplateAssets`
  - simple image-based tutorial surfaces
- broad local gameplay-audio layer for:
  - gates
  - instruction terminals
  - lesson phases
  - transfer events
  - buttons
  - datapacket spawn feedback
  - scanner feedback
- refactored `CpuLesson` folder organized into:
  - `Flow`
  - `Support`
  - `UI`
  - shared UI helpers under `Assets/MyScripts/Shared/UI`

Current supported instructions:
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

Current build truth:
- these instructions work through the current guided loop
- the project is now in polish/presentation mode more than raw datapath expansion mode
- the next serious system extension in flight is practice mode, built on top of the same lesson foundation

More detailed current-state notes:
- fetch is no longer just UI framing; it uses a physical instruction upload/download handoff
- intro flow is no longer only a single instruction picker; it is now being reshaped into a mode-first entry point
- decode is no longer just register placement; it now includes:
  - instruction field framing
  - opcode selection
  - funct handling where applicable
  - source-operand scanning
  - immediate generation for immediate-bearing instructions
- when no lesson is active, the two decode-stage lesson scanners can now act as simple preview scanners instead of staying uselessly inactive
- execute is currently responsible for:
  - ALU signal interaction
  - operand acceptance
  - operation selection
  - result spawning
- memory is currently responsible for:
  - address validation
  - read vs write behavior
  - memory bank communication
  - output packet spawning for loads
- write-back is currently responsible for:
  - destination validation
  - value-source validation
  - final register update
- PC update is currently responsible for:
  - branch resolution framing
  - next-PC confirmation
  - lesson conclusion

What is current polish work versus already-solved systems:
- already solved enough to build on:
  - core lesson flow
  - instruction fetch embodiment
  - register interaction
  - immediate extension
  - ALU execution
  - memory access for `lw` / `sw`
  - write-back
  - branch resolution
- still active polish work:
  - practice mode
  - tutorial/onboarding
  - experiment mode
  - optional background music / ambient tone
  - light route/gate readability tuning only if testing exposes issues
  - optional settings refinement

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
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support\PracticeInstructionDefinition.cs`
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

## Current Milestone

- next supervisor checkpoint: `2026-07-24`
- official demo deadline: `2026-07-29`
- current focus is presentation readiness:
  - practice-mode foundation
  - tutorial / onboarding
  - navigation clarity
  - UI readability
  - experiment-mode planning
  - optional ambience / background music
  - only small settings-menu refinements if they clearly help

## If I Resume After A Break

Key files to reread:
- `ProjectJournal/02_MASTER_EXECUTION_PLAN.md`
- `ProjectJournal/01_PROGRESS_LOG.md`
- `ProjectJournal/04_DECISION_LOG.md`
- `ProjectJournal/03_PROJECT_SNAPSHOT.md`

## If I Need To Stop Mid-Session

Before stopping:
- make sure `01_PROGRESS_LOG.md` reflects the current state
- make sure any major scope shift is written into `04_DECISION_LOG.md`

## Current Open Questions

- what the safest first playable slice of practice mode should be before deeper decode complexity is added
- how strong the door + arrow + audio guidance should be without becoming visually noisy
- whether onboarding V1 should ship as:
  - a recorded custom tutorial video
  - a coaching-card image/text walkthrough
  - both, if still cheap enough
- what the minimum viable experiment mode should hide, skip, or ungate
- how much of the current UI wording should move entirely into authored scene text versus stay runtime-driven
- whether instruction choice should remain manually selected, become randomized, or support both
- whether jump-family instructions are worth fitting in before the July 24 checkpoint
- how much onboarding belongs in a dedicated tutorial versus just-in-time prompts in the map
- how much post-meeting time should be reserved for participant prep instead of feature work
- whether a helper NPC / guide would actually improve onboarding or only add scope/noise
- whether an optional music player / ambient scene audio source would help presentation tone without distracting from the lesson

## Current Pre-Meeting Priorities

Ordered cutoff list before the next supervisor meeting:

1. tutorial UI / onboarding decision and first pass
2. practice mode before the checkpoint if feasible
3. experiment mode before the checkpoint if feasible
4. optional settings refinement only if it materially helps usability
5. jump-family evaluation / inclusion only if safe
6. final polish
7. optional helper NPC / guide if time remains
8. optional in-world music player / ambience pass if time remains
9. optional VFX pass if the unused Unity Asset Store effects pack proves presentation-helpful and cheap to integrate

Everything else should be treated as future work unless a blocking regression appears.

## Best Resume Point For The Next Development Session

The cleanest next work item is:
- continue the practice-mode foundation while preserving the current guided flow
- choose and build the first real onboarding path
- likely keep both tutorial options on the table for now:
  - recorded custom video
  - coaching-card walkthrough
- add experiment mode only after the guided version feels presentation-safe
- keep map changes to touch-up work unless testing exposes a real routing/readability issue

Do not resume from an older assumption that gating or route guidance still needs first implementation; those systems now exist and should be refined, not reinvented.

## Latest Visual Polish Note

- the route-arrow guidance layer is now in a usable refinement state rather than first-pass prototype state
- a reusable `AuthoredOffsetLerp` helper exists for optional opening-sequence/environment settle-in work
- that helper is intentionally dormant by default and should only be enabled on specific authored scene pieces when actively testing an intro beat
- recent Unity headset play-mode delays looked inconsistent and editor-side, so they should not be treated as evidence that the offset helper itself is unsafe
- the project also now has a baseline wrist settings menu, so player-facing support is no longer limited to the phase-local lesson panels alone
- the project now also has a broad gameplay-audio feedback layer, so any remaining sound work should be treated as balancing or ambience rather than missing core interaction support
- no dedicated separate cheatsheet is currently planned, since the hint panels are expected to cover most reference needs

## Personal Reminder

If the project feels too large, that is not a sign the concept is bad.
It is usually a sign that the lesson objective needs to be narrowed again.
