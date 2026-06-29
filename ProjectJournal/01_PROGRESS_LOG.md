# Progress Log

## Current Status

Project phase:
- scene-authored MVP lesson pass for the June 29 V1 check-in

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype focus:
- minimal playable `add` lesson loop in `Testing Ground`
- scene-authored `Intro UI` and `Register Setup UI` under `Lesson Guide`
- authored 32-register MIPS bank with local reset
- per-register logical values now supported in code
- register scanner validation path for decode-stage source operands
- first-pass ALU execution phase now wired with authored ALU UI, physical ALU buttons, and result spawning
- keeping lesson code small and tied to existing scene objects instead of building UI at runtime
- preparing a dedicated write-back phase for the demo instead of confirming the destination register during decode

Current milestone:
- V1 supervisor demo target on `2026-06-29`
- `add`, `addi`, and `lw` should be finished by `2026-06-28` at all costs
- this is the near-term prototype target, not the permanent upper bound of the whole project

## Latest Summary

The project now has:
- a Unity project committed and pushed
- a `Testing Ground` scene being used as the active prototype sandbox
- a scene-authored `Lesson Guide` area hosting world-space lesson UI
- a scene-side `Register Bank` anchor for permanent register authoring
- a permanent 32-register MIPS bank serialized into `Testing Ground`
- grabbable labeled register tokens with working local reset behavior
- a reusable register prefab/material path under `Assets/MyPrefabs` and `Assets/MyMaterials`
- register scanner pedestals for `Read Register 1`, `Read Register 2`, and `Write Register`
- logical register values stored on register tokens, with lesson-time value seeding from instruction assets
- a working MVP lesson flow that starts from `Intro UI`, hands off to `Register Setup UI` for instruction decode, then proceeds into ALU execution
- control decode scene content still exists, but the active lesson path is now being simplified around fetch -> decode -> execute -> write-back
- a smaller lesson architecture centered on focused lesson and register scripts
- cleaned instruction assets for `add`, `addi`, and `lw`
- a working first-pass `ALU` execution loop for `add`:
  - register scanners emit `Read Data 1` / `Read Data 2` packets
  - authored ALU input zones accept the correct packet types through child trigger scanners
  - physical `ALUOp` / `ALUSrc` buttons drive execution setup
  - the authored `ALU UI` validates execution and gates the continue into write-back
  - the ALU emits an `ALU Result` packet with the computed value
- a cleaner instruction-decode model:
  - `add` scans `rs` and `rt`
  - `addi` and `lw` scan `rs` only
  - immediate-based instructions spawn the `Immediate` packet from the second scanner's packet spawn location
  - destination register choice is now intended for write-back rather than register decode

## Chronological Entries

### 2026-06-28 - Control Decode Signal Check Wired Into Authored Scene UI

Completed:
- kept the new `Control Decode UI` scene-authored under `Lesson Guide`
- kept the 8 control-signal buttons scene-authored under `Control Unit`
- split the lesson UI and control-decode responsibilities more cleanly:
  - `LessonGuideController` now drives panel visibility and lesson handoff
  - `ControlDecodeController` now owns the decode-phase button interaction and validation
- wired the authored lesson objects through serialized Inspector references instead of runtime scene-object discovery
- made the active lesson flow explicit:
  - `Intro UI`
  - `Control Decode UI`
  - `Register Setup UI`
- kept the control-signal interaction scene-authored:
  - physical control buttons cycle signal values
  - `Control Decode UI` mirrors each current signal state in its authored text fields
  - the panel action button checks the full control-signal combination and gates progression
- derived first-pass expected control signals directly from the active instruction for:
  - `add`
  - `addi`
  - `lw`
- serialized the lesson's `RegisterBank` reference into `CpuLessonFlow` instead of relying on runtime lookup

Changed:
- control decode is now a real decode-phase gate in the authored lesson flow instead of a vague later add-on
- the current implementation is aligned with the scene-authored direction:
  - no runtime-built lesson UI
  - no runtime-created control buttons
  - no runtime scene searches for the main lesson objects

Next:
- polish the authored scroll-panel layout for `Intro UI`, `Control Decode UI`, and `Register Setup UI`
- keep the control decode step lightweight and readable rather than turning it into a giant standalone minigame
- extend the same authored flow into `addi` and `lw` more formally

Risks / Notes:
- this pass uses instruction-derived expected control signals in code instead of a larger custom control-definition asset system
- that is intentional scope control for the V1 deadline
- the current codebase should now be treated as scene-wired first; future work should keep adding serialized refs rather than slipping back into lookup-heavy patterns

### 2026-06-15 - Unity Prototype Baseline Established

Completed:
- merged and improved the repo `.gitignore`
- added the Unity project to the repository
- inspected XR Interaction Toolkit starter assets and demo content
- confirmed `Testing Ground` as the active prototype scene
- added a CPU placeholder node layout in the test zone
- adjusted the placeholder node order and labels to match the intended user-facing view
- cloned the user's preferred mini-label style across the placeholder nodes
- added a physical XR push button in-scene without editing source prefabs directly
- wired the button to progress the node highlight sequence
- added `CpuNodeSequenceController.cs`
- added interaction affordance behavior to match the stock push button feedback
- added first-pass comments to the node sequence script
- drafted `InstructionSystemV1` scripts under `Assets/MyScripts`
- committed the work under:
  - `Initial setup for cpu flow + instruction set`

Notes:
- the user later pushed the commit manually due to credit limits
- `Testing Ground` became the stable sandbox scene for near-term CPU datapath prototyping

### 2026-06-16 - Planning Day After Supervisor Discussion

Completed:
- reviewed the current dissertation scope through the lens of the "why VR?" question
- narrowed the project toward a small number of meaningful learning objectives
- aligned around a safer strategy:
  - first achieve one learning objective well
  - only then extend to additional instruction families or mechanics
- clarified the likely educational framing:
  - recreate the effectiveness of paced one-on-one whiteboard guidance
  - transform static datapath tracing into a spatial, interactive, guided reasoning experience
- identified a strong design principle:
  - use VR for meaningful spatial decisions
  - use UI for lookup/reference/explanation
- outlined an interaction concept for instruction walkthroughs, especially around MIPS datapath tracing
- created this `ProjectJournal` folder and repository-level persistence system
- clarified the near-term V1 delivery target:
  - by `2026-06-28`, the prototype should support `add`, `addi`, and `lw`
  - on `2026-06-29`, the user intends to demo this V1 to the supervisor
  - this does not mean later expansion is off the table

### 2026-06-26 - Register Bank Visual And Interaction Direction Locked In

Completed:
- moved register-related scripts into `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers`
- added a reusable custom register prefab under `D:\CompArchVR\ThePrototype\Assets\MyPrefabs`
- settled on the chunky labeled register look instead of the bad tiny-cylinder pass
- kept the registers grabbable with XR grab behavior instead of button-only behavior
- added a local register-bank reset path
- serialized a permanent 32-register MIPS bank into `Testing Ground`
- confirmed the reset button returns registers to their home poses
- introduced authored register scanners for:
  - `Read Register 1`
  - `Read Register 2`
  - `Write Register`

Changed:
- the register area is now scene-authored first
- the lesson no longer depends on runtime-generated register buttons
- later `EX` and `WB` interactions are expected to grow from physical placement / scanning rather than abstract UI-only confirmation

Next:
- continue using the authored bank and scanners as the baseline for lesson progression
- extend the same physical interaction pattern into later datapath zones

Risks / Notes:
- register color / feedback behavior may still need polish later, but the current baseline is functional

### 2026-06-27 - Scene-Authored UI Direction Locked In

Completed:
- confirmed that `Intro UI` in `Testing Ground` is a real world-space panel under `Lesson Guide`
- stopped treating the scene as if the old placeholder-node/runtime-UI setup were still authoritative
- accepted that future lesson guidance should be represented by authored scene panels

Changed:
- the UI direction is now explicitly scene-authored first
- the scene hierarchy, not older runtime UI experiments, is the source of truth

Next:
- keep lesson panels authored in `Testing Ground`
- let code update those panels instead of creating them

Risks / Notes:
- older lesson experiments still exist in repo history and should not be mistaken for the intended path

### 2026-06-28 - Scene-Authored Lesson MVP Verified

Completed:
- verified in-editor that the current lesson path works
- kept `Lesson Guide` as the lesson host for the current MVP
- kept `Intro UI` as the start / instruction / decode-facing panel
- kept `Register Setup UI` as the register-zone-facing panel
- confirmed the register zone path still works with:
  - physical register tokens
  - `Read Register 1`, `Read Register 2`, and `Write Register` scanners
  - local register reset
- retained a minimal `CpuLessonFlow` + `LessonGuideController` setup instead of returning to the older oversized controller pattern

Changed:
- the project should now be treated as scene-authored first:
  - UI elements are expected to exist in edit mode
  - code should update or toggle existing scene objects instead of spawning lesson UI at runtime
- the current MVP baseline is:
  - `Intro UI` exists in-scene
  - `Register Setup UI` exists in-scene
  - lesson code drives those authored panels

Next:
- polish `Intro UI` wording and layout
- polish `Register Setup UI` wording and layout
- extend the same scene-authored panel pattern into later lesson zones such as `ALU`, control/decode, and memory
- continue toward `addi` and `lw` using the same authored-panel + physical-interaction structure

Risks / Notes:
- older journal entries before this point are historical and should not override the current scene-authored baseline
- the user has explicitly validated that the present MVP works so far
- the current authored lesson order is now:
  - `Intro UI`
  - `Register Setup UI`
  - `ALU UI`
  - temporary write-back explanation / continue

### 2026-06-28 - Intro/Register UI Layout Stabilized And Decode Continue Added

Completed:
- fixed the lesson-guide panel layout issue by treating `Intro UI` and `Register Setup UI` as authored layout panels whose content is rebuilt after runtime text changes
- updated the lesson guide controller so authored text and action buttons no longer rely on runtime-generated panel content
- confirmed the `Intro UI` panel now works as the real lesson entry point for the current MVP

### 2026-06-29 - Pre-Demo Script Cleanup And Comment Pass

Completed:
- removed leftover empty `MyScripts` folders that were no longer part of the scene-authored lesson path
- removed clearly dead compatibility methods from the register bank cleanup path
- kept the tested MVP behavior intact while tightening comments across:
  - lesson flow
  - lesson guide
  - control decode
  - ALU execution
  - ALU input scanners
  - register scanners
- preserved the current working `Testing Ground` scene and lesson behavior after cleanup

Notes:
- this pass was intentionally conservative: obvious dead code was removed, but potentially reusable fallback paths were left alone if they still fit future lessons
- confirmed the `Register Setup UI` panel follows the same authored layout pattern
- updated control decode so a correct control-signal setup no longer advances immediately
- added the intended decode rhythm:
  - first press validates signals
  - success feedback appears
  - a final press continues to the next phase
- verified that the current flow works through:
  - `Intro UI`
  - `Control Decode UI`
  - `Register Setup UI`
  - immediate transition into the next lesson phases already scaffolded in code

Changed:
- lesson panel text is now expected to be resized through authored Unity layout components plus forced rebuilds in code
- `Control Decode UI` now behaves more clearly as a gated teaching step instead of auto-jumping the moment the answer is correct

Next:
- create branch `ALU_V1` from `main`
- build the first authored `ALU` execution step for `add`
- keep `Data Memory` out of the `add` route while making the ALU system reusable for `addi` and later `lw`

Risks / Notes:
- during register selection, success text currently stays minimal while failure text is more explicit; this is acceptable for now
- after the third correct register, later-step auto-satisfaction is still possible if a reused scanner is already holding the expected register; this should be cleaned up in the ALU/write-back pass

### 2026-06-28 - Register Values And ALU Groundwork Added

Completed:
- added logical integer values to register tokens
- added register-bank helpers to:
  - read register values
  - set register values
  - reset only logical values separately from physical reset
- kept the local register reset button behavior limited to pose / scanner / visual reset so it does not accidentally wipe lesson values
- updated lesson startup/reset flow so instruction assets can seed runtime register values
- cleaned instruction definition assets under `Assets/MyData/Resources/InstructionDefinitions`
- removed stale instruction-definition fields that no longer match the current authored-scene workflow
- renamed `InstructionSystemV1` to `InstructionSystem`
- added first-pass ALU-side scripts for:
  - data packets emitted from successful register scanners
  - ALU input scanners that accept data packets
  - a lightweight ALU execution controller that computes the first-pass `add` result in code

Changed:
- the project now distinguishes between:
  - physical register identity
  - logical register value
  - datapath value packets carried into later stages
- the instruction data model now owns initial register values instead of old curated bank-choice / UI-layout fields

Next:
- author a `Data Packet` prefab using the pyramid model
- author ALU input scanners using the torus model
- hook the new ALU scripts into `Testing Ground`
- decide whether value display should live on the packet, the scanner, or both for each ALU-side interaction

Risks / Notes:
- the ALU groundwork is code-first right now; the Unity scene hookup is still required
- `Testing Ground.unity` and the register scanner prefab already had live scene/prefab edits during this pass and should be treated carefully in follow-up work

### 2026-06-29 - ALU Execution Phase Wired For add

Completed:
- moved `ALUOp` and `ALUSrc` responsibility out of control decode and into the ALU phase
- kept register data packets alive after the register-selection step so execution can consume them
- wired the ALU execution loop around authored scene objects:
  - authored `ALU UI`
  - physical `ALUOp` button
  - physical `ALUSrc` button
  - authored ALU input trigger zones
  - authored ALU result spawn
- made `ALUSrc` control what `Input 2` accepts:
  - `0` -> `Read Data 2`
  - `1` -> `Immediate`
- made the ALU operation label use plain operation names:
  - `Add`
  - `Sub`
  - `And`
  - `Or`
  - `Slt`
- made the ALU emit a result packet of role `ALU Result`
- changed the ALU success flow so the UI shows the resulting value and requires one extra continue press before handing off to write-back
- changed the local register reset button so it now only restores moved register pieces to their home poses

Changed:
- the current `add` walkthrough order is now:
  - `Intro UI`
  - `Control Decode UI`
  - `Register Setup UI`
  - `ALU UI`
  - temporary `Intro UI` write-back explanation
- local register reset no longer:
  - destroys data packets
  - resets successful scanner colors
  - turns active scanners inactive

Next:
- build the actual ALU control sub-step later if desired
- extend the same execution pattern into `addi`
- then build the memory-path variation for `lw`

Risks / Notes:
- the current write-back is still a temporary explanation/continue step, not yet its own physical datapath interaction
- `Write Register` / `rd` is confirmed during the register phase and does not emit a data packet

### 2026-06-29 - IF/ID Cleanup Prepared For Write-Back

Completed:
- removed the old control-decode panel from the active `LessonGuideController` lesson path
- simplified the lesson path toward:
  - `Intro UI` for lesson intro and instruction fetch framing
  - `Register Setup UI` for instruction decode and operand preparation
  - `ALU UI` for execution
  - dedicated write-back to be authored next
- updated instruction definitions so decode no longer expects `rd` placement for `add`
- updated draft `addi` and `lw` definitions so decode expects:
  - `rs` scan
  - immediate handling
  - destination register deferred to write-back
- updated lesson validation helpers so decode-stage required register roles depend on the instruction instead of assuming `rs`, `rt`, and `rd`
- added immediate-value support directly to `InstructionDefinition`
- added write-back target resolution to `InstructionDefinition` so:
  - R-type destinations resolve to `rd`
  - immediate/load destinations resolve to `rt`
- updated register scanners and register-bank helpers so scanner output roles can be reassigned by lesson flow
- added decode-side immediate packet spawning through the second scanner's authored packet spawn anchor

Changed:
- the intended phase semantics are now cleaner:
  - `IF` tells the learner what instruction is being fetched
  - `ID` gathers only the source operands actually needed
  - destination selection is deferred to write-back
- `Register Setup UI` is now the intended home for both:
  - instruction field breakdown
  - source register scanning

Next:
- finish the dedicated write-back prefab and UI
- replace the temporary write-back explanation with:
  - WB control-signal configuration
  - target-register scan
  - final data-packet scan
  - register value update on success
- then add a minimal memory phase panel/prefab for `lw`

Risks / Notes:
- one open design question remains:
  - whether `RegDst` should stay in write-back only
  - or whether `ALUSrc` / `RegDst` responsibilities should be exposed earlier in the teaching flow
- current recommendation is to leave the flow as-is for the demo and finish write-back first

## Current Working Baseline

### Scene / Interaction Baseline

- `Testing Ground` is the sandbox scene
- the lesson framework is currently driven from `Lesson Guide`
- `Intro UI` is the current lesson start point
- `Register Setup UI` is the current instruction-decode and operand-selection panel
- the preferred register path is the authored `Register Bank` with 32 permanent register tokens
- the preferred register validation path is the authored scanner pedestals
- minimal visual feedback is acceptable; heavy animation is not required

### Architecture / Script Baseline

Current relevant scripts:
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\CpuLessonFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonGuideController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonChecks.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluExecutionController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluInputScanner.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluInputScannerZone.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\AluPacketTypes.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU\DataPacketToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBank.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankResetButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterScanner.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterScannerZone.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionEnums.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionDefaults.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem\InstructionRuntimeSelection.cs`

Interpretation:
- these are the scripts that currently matter to the working MVP
- older experiments outside this set should be treated cautiously and removed if they stop matching the scene
- the next work should refine this scene-driven path rather than reintroduce runtime-built lesson UI or runtime scene lookup glue

## Immediate Next Steps

Recommended next development priorities:

1. Polish the current `add` MVP without changing its authored-scene direction.
   Focus on:
   - cleaner `Intro UI` layout
   - cleaner `Register Setup UI` layout
   - clearer feedback text
   - simpler inspector wiring where possible
   - finish the dedicated write-back phase

2. Treat `add`, `addi`, and `lw` as the non-negotiable V1 set for the `2026-06-29` demo target.
   Recommended implementation order:
   - `addi`
   - `lw`

3. Build the next interaction layer around placement pedestals and authored lesson panels.
   Focus on:
   - `WriteBack` pedestals that confirm the correct destination register
   - `WriteBack` packet validation for the final value source
   - success / failure colors tied to active lesson-step validation
   - storing scanned register identity so reused registers can be handled cleanly

4. Extend the same framework into `addi`.
   Reuse:
   - instruction assets
   - scene-authored lesson panels
   - curated register bank workflow
   - pedestal validation workflow

5. Keep the post-V1 door open.
   After the June 29 supervisor demo:
   - expand to more instructions only if the V1 loop is stable
   - preserve the architecture so later instructions can reuse it

## Risks To Watch

- scope creep into "simulate all of MIPS"
- too many manual low-level interactions turning the lesson into a tedious puzzle
- over-investment in animation/polish before the educational loop works
- confusing "accurate hardware behavior" with "best pedagogical interaction"

## Update Template For Future Entries

Use this format when appending future entries:

### YYYY-MM-DD - Title

Completed:
- item
- item

Changed:
- item
- item

Next:
- item
- item

Risks / Notes:
- item
