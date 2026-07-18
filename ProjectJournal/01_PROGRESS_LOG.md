# Progress Log

## Current Status

Project phase:
- final-delivery polish pass after the core single-cycle lesson loop was proven end-to-end

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current stable feature set:
- playable guided lesson paths for:
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
- authored phase spaces for:
  - instruction fetch
  - instruction decode
  - execute
  - memory access
  - write-back
  - PC update / lesson conclusion
- lesson-mode groundwork for:
  - `Learning`
  - `Practice`
- full playable practice-mode baseline for:
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
- physical instruction fetch through:
  - `Instruction Module`
  - uploader `Instruction Terminal`
  - downloader `Instruction Terminal`
- authored three-panel lesson UIs for the active phase stations
- authored 32-register MIPS bank with:
  - logical register values
  - authored sample values across all 32 registers
  - local pose-only reset
  - authored-value restore on lesson-mode change
  - scanner-driven decode interaction
- working datapacket flow for:
  - register reads
  - immediates
  - ALU results
  - memory data
  - branch zero result
- dedicated authored stations for:
  - ALU
  - Memory Unit + Data Memory bank
  - Write Back
  - PC Update
- datapacket reset for loose, non-consumed packets
- reworked routed map now established as the active baseline
- gated route progression with authored arrow guidance
- player-facing wrist settings menu support with:
  - current instruction readout
  - current phase readout
  - register reset
  - datapacket reset
  - lesson restart
  - volume control
  - route-guidance toggle
  - password-gated dev-mode unlock
  - current-phase skip for dev testing
  - FPS + frame-time readout
  - quit / restart support
- controller-accessible wrist menu opening through the left controller menu-button path
- spatial-keyboard-backed TMP input fields now work inside the lesson scene
- lightweight onboarding groundwork in-scene through:
  - the imported tutorial/video prefab from `VRTemplateAssets`
  - simple image-based tutorial surfaces
- broad gameplay audio pass covering:
  - gate transitions
  - instruction terminals
  - menu/world buttons
  - phase activation / completion / failure cues
  - lesson completion
  - memory / write-back transfer cues
  - datapacket spawn audio
  - scanner occupied / success / failure cues

Current interaction systems that are already implemented:
- permanent 32-register MIPS bank serialized into `Testing Ground`
- grabbable labeled register tokens with persistent logical values
- register scanners for:
  - decode-stage source-operand validation
  - idle preview/reference use when no lesson is active
- local register reset that restores pose only
- datapacket reset that restores loose packet transforms only
- immediate packet generation and physical sign-extension through the `Immediate Extender`
- ALU execution through:
  - physical `ALUOp` and `ALUSrc` inputs
  - packet-fed ALU input scanners
  - ALU result spawning
  - funct selection for R-type operation selection
- memory interaction through:
  - `Memory Unit`
  - `Data Memory` bank
  - memory read/write control handling
  - addressed-word highlighting and data display
- write-back interaction through:
  - target register validation
  - datapacket validation
  - final register value update
- branch resolution through:
  - `PC Update UI`
  - `PC Update Station`
  - zero-result path
  - branch condition handling for `beq` / `bne`
- scanner feedback through shared occupied / success / failure visuals and audio
- local 3D audio feedback across the main interaction loop
- practice decode flow for the first safe slice now supports:
  - fetch display in hex form
  - decode display in 32-bit binary form
  - staged opcode confirmation
  - staged field validation
  - typed bitfield entry through TMP input fields + the spatial keyboard
  - limited hints
  - limited answer attempts
  - failed-state hold until explicit reset press
- later practice phases now support:
  - per-phase validation-attempt budgets
  - per-phase scanner-attempt budgets
  - per-phase hint budgets
  - held failure states that wait for explicit restart
  - shared failure behavior across `EX`, `MEM`, `WB`, and `PC Update`
- centralized dev-mode support now exists for:
  - unlocking dev mode from the settings menu
  - skipping the currently active phase from the settings menu
  - force-completing `ID`, `EX`, `MEM`, `WB`, and `PC Update`

Current architecture truths:
- lesson UI is scene-authored and code-driven
- core lesson objects are expected to be wired through serialized scene references
- runtime scene lookup glue has been reduced and should not be reintroduced casually
- Unity UI event hookups are now expected to be Inspector-bound instead of wired in code at runtime
- the routed map is part of the lesson design, not just environment decoration
- the current build direction is polish-first, not instruction-count-first
- practice mode is now an active extension path and should be built by safely extending the existing lesson architecture, not replacing it
- the decode panel now uses a clearer split between:
  - scene reference containers
  - learning-mode view logic
  - practice-mode view logic
  - practice decode validation state

Current delivery target:
- next supervisor checkpoint: `2026-07-24`
- official demo deadline: `2026-07-29`
- preferred internal target:
  - finish a stable build a few days early
  - leave the buffer for polish, participant prep, and presentation safety

Current development priority:
- stabilize the practice-mode groundwork without regressing the guided baseline
- stabilize the first practice-mode decode slice before widening instruction coverage inside that mode
- improve readability, onboarding, experiment-mode readiness, and overall presentation quality
- avoid major architecture churn unless it fixes a real blocker
- preserve the now-working lesson loop while polishing around it

## Active TODO Before The Next Supervisor Meeting

These are the live priorities from the current perspective:

1. add the info catalog
2. record the updated tutorial video
3. add background music
4. address decode read-register priority / ordering
5. add test mode
6. add the game intro sequence
7. final polish
8. refactor oversized scripts later if time remains and only if it does not destabilize the current build

Optional only if time remains:
- helper NPC / guide presence
- in-world music player / ambient interaction
- VFX pass using the Unity Asset Store pack already identified but not yet integrated

Interpretation:
- the datapath lesson itself is already broad enough for the checkpoint
- the remaining risk is experience quality, not missing core single-cycle stages
- a baseline settings menu now exists, so the remaining question is expansion/tuning rather than first implementation
- the map revamp / better route baseline should now be treated as established work, with only touch-up tuning left if testing reveals a problem
- the core sound-feedback pass is now in place, so remaining audio work is mostly balancing, ambience, or optional presentation flavor

## Current Working Build Snapshot

What should currently be treated as true:
- `Testing Ground` is the authoritative prototype scene
- lesson UI is scene-authored and code-driven, not runtime-generated
- fetch, decode, execute, memory, write-back, and PC update all exist as real authored interactions
- the routed map is part of the teaching experience, not just scenery
- door gating and arrow guidance are now presentation-layer systems on top of the working lesson loop
- a baseline wrist settings menu now exists for player-facing recovery, status, and guidance control
- a baseline gameplay audio layer now exists across scanning, phase flow, transfer events, and local interaction feedback
- tutorial/onboarding now has real in-scene groundwork rather than being a purely abstract future task
- the improved routed-map layout should now be treated as the active baseline rather than an unfinished first-pass map task
- the current build direction is polish-first, not instruction-count-first
- the next major build change in flight is practice-mode support layered onto the same lesson foundation
- the practice instruction baseline now spans the same ten-instruction set as learning mode
- temporary testing relief now exists through a centralized dev-mode skip path instead of ad-hoc scene edits
- supported working instructions are:
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

## Historical Development Archive

Everything below this heading is intentionally preserved milestone history.
Older entries remain valuable for the dissertation, presentation, and design rationale, but they should not be mistaken for the live current-state summary above.

Entries in this archive are kept in chronological order from oldest to newest.


### 2026-06-15 - Unity Prototype Baseline Established

Completed:
- merged and improved the repo `.gitignore`
- added the Unity project to the repository
- inspected XR Interaction Toolkit starter assets and demo content
- confirmed `Testing Ground` as the active prototype scene
- added a CPU placeholder node layout in the test zone
- adjusted the placeholder node order and labels to match the intended player-facing view
- cloned the preferred mini-label style across the placeholder nodes
- added a physical XR push button in-scene without editing source prefabs directly
- wired the button to progress the node highlight sequence
- added `CpuNodeSequenceController.cs`
- added interaction affordance behavior to match the stock push button feedback
- added first-pass comments to the node sequence script
- drafted `InstructionSystemV1` scripts under `Assets/MyScripts`
- committed the work under:
  - `Initial setup for cpu flow + instruction set`

Notes:
- the commit was later pushed manually during a session cutoff
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
  - on `2026-06-29`, I planned to demo this V1 to the supervisor
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


### 2026-06-28 - Intro/Register UI Layout Stabilized And Decode Continue Added

Completed:
- fixed the lesson-guide panel layout issue by treating `Intro UI` and `Register Setup UI` as authored layout panels whose content is rebuilt after runtime text changes
- updated the lesson guide controller so authored text and action buttons no longer rely on runtime-generated panel content
- confirmed the `Intro UI` panel now works as the real lesson entry point for the current MVP


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
- I had already validated that the present MVP worked at this stage
- the current authored lesson order is now:
  - `Intro UI`
  - `Register Setup UI`
  - `ALU UI`
  - temporary write-back explanation / continue


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


### 2026-06-29 - V1 Closed Out, V2 Design Started

Completed:
- closed out the current V1 lesson slice around `add`
- kept the tested intro -> decode -> execute -> memory checkpoint -> write-back path intact
- removed the now-dead `ControlDecodeController` script from `Assets/MyScripts`
- cleaned the lesson flow so the old write-back confirmation branch is no longer part of the active runtime path
- kept immediate groundwork lightweight by adding a boolean sign-extension state directly to datapackets
- updated ALU validation so immediate-based execution can now distinguish between raw and sign-extended immediate packets

Changed:
- the repo is now transitioning from "stabilize the demoable `add` slice" into "prepare `addi` and `lw` cleanly"
- the lesson/runtime code should now treat:
  - immediate packets as first-class datapath objects
  - sign extension as an explicit state that can later become a physical interaction

Next:
- author the physical sign-extension interaction when ready
- extend decode / ALU / memory / write-back into `addi`
- build the real memory interaction for `lw`

Risks / Notes:
- the sign-extension path is intentionally simple for now
- the next design pass should focus on clarity and reuse, not another rushed architectural rewrite


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


### 2026-06-29 - Dedicated Write-Back Groundwork Started

Completed:
- introduced a dedicated write-back interaction direction instead of relying on the temporary intro-panel confirmation
- added first-pass write-back scripts and wiring groundwork for:
  - register-target scanning
  - datapacket scanning
  - write-back phase gating inside the lesson flow
- updated instruction / lesson definitions so write-back can be treated as its own explicit interaction step
- prepared the lesson guide to recognize a future authored `Mem UI` and authored `WB UI`
- separated a bonus register-inspection scanner from the lesson-controlled decode scanners so it can be used as a value-check station

Changed:
- the lesson is now being steered toward:
  - `Intro UI`
  - `Register Setup UI`
  - `ALU UI`
  - `Mem UI`
  - `WB UI`
- write-back is no longer being treated as just a narrative "continue" step in the architecture

Next:
- finish the dedicated `WB` prefab hookup
- finish `WB UI` validation and execution flow
- keep `Mem UI` explanatory-only for the demo unless `lw` memory logic is stabilized in time
- iron out the exact pedagogical home of `RegDst` and `ALUSrc`

Risks / Notes:
- this pass is groundwork, not the final validated write-back implementation
- the open teaching-design question is still:
  - whether `RegDst` belongs strictly in write-back
  - whether `ALUSrc` should remain purely execution-side
  - or whether either should be surfaced earlier for lesson clarity


### 2026-06-29 - Supervisor Demo Completed, Next Target Set

Completed:
- demonstrated the current `add` walkthrough to the supervisor
- validated the staged difficulty ramp:
  - `IF` as light introduction
  - `ID` as first physical interaction/scanner training
  - `EX` as a more involved phase with changing UI and physical controls
  - `WB` as the final combined interaction
- kept the choice of not frontloading all signals at the beginning
- refined the authored lesson UIs after the demo and confirmed the scene now works end-to-end again

Supervisor Feedback:
- separate guide / explanation UI from interaction UI where practical
- reduce the need to scroll for information during active interaction
- the current difficulty ramp is a good design choice
- `RegDst` may later be separated again, but only if it does not create too much clutter

Changed:
- the project is now in post-demo refinement mode rather than first-demo scramble mode
- the next milestone is no longer "make `add` demoable"; it is "extend the framework cleanly to `addi` and `lw` by July 6, 2026"

Next:
- add a persistent settings / cheatsheet panel the learner can open at any time
- decide how immediate generation and sign extension should physically appear in `ID`
- design the ALU control sub-step more clearly
- build the real memory interaction for `lw` (and eventually `sw`)

Risks / Notes:
- the main design challenge is now clarity rather than raw MVP existence
- future additions should preserve the staged teaching feel instead of dumping too many controls into one phase

## Chronological Entries


### 2026-07-02 - addi Immediate Path And Secondary Instruction Flow Stabilized

Completed:
- validated `addi` as a real secondary lesson path instead of only keeping `add` working
- fixed the lesson-flow regression that had been skipping ALU / memory and jumping straight to write-back
- re-stabilized the authored phase order so `add` and `addi` both move through:
  - `Intro UI`
  - `Register Setup UI`
  - `ALU UI`
  - `Mem UI`
  - `WB UI`
- added the physical `Immediate Extender` path so immediate packets can be sign-extended before ALU execution
- updated ALU-side packet validation so immediate inputs are rejected cleanly until sign extension is complete
- added clearer ALU-side immediate status messaging so the learner can tell when the wrong immediate state is present
- wired the authored ALU funct dropdown so the secondary instruction path can drive the expected ALU operation more explicitly
- confirmed that the current `add` and `addi` loops are now good enough to stop on for the day

Changed:
- immediate handling is no longer only a quiet boolean in code; it now has a visible physical interaction path through the extender
- the project now has one register-register path (`add`) and one immediate-based path (`addi`) functioning in the same lesson framework
- the next bottleneck is no longer decode / ALU sequencing; it is the actual memory-phase buildout for `lw`


### 2026-07-04 - lw Memory Phase Validated End-To-End

Completed:
- finished the real memory interaction path for `lw`
- validated a real MIPS-style address flow instead of tiny prototype-only offsets
- updated the `lw` lesson setup so:
  - base register holds a data-segment address
  - immediate acts as the byte offset
  - ALU result maps cleanly into the authored 24-word memory bank
- finished the dedicated `Memory Unit` + `Data Memory` bank interaction loop:
  - address input pedestal
  - optional data input pedestal for future store support
  - `MemRead` / `MemWrite` controls
  - central address/value readout
  - addressed-word highlighting
  - memory-data packet spawn for write-back
- fixed the memory-phase UI ownership conflict so the `MemoryUnitController` now owns Mem UI behavior directly
- fixed the pipe animation so it now behaves as:
  - idle
  - one-time waiting sweep
  - transfer success sweep
  - back to idle only when the phase ends
- kept the memory output packet alive after Mem so `lw` can actually complete WB
- confirmed that `add`, `addi`, and `lw` are now all functioning through the current lesson loop

Changed:
- memory addresses are now presented in hex while stored values stay decimal
- hover-preview behavior in the memory bank now defers to the addressed-word preview once a real address has been scanned
- the memory phase is no longer just an explanatory checkpoint; it is now a real interaction step in the lesson

Next:
- build `sw` on top of the now-working memory bank + memory unit structure
- refine memory-bank presentation and reference UI only if it materially helps the next checkpoint
- continue treating guide UI and interaction UI as separate concerns where practical

Risks / Notes:
- the current 24-word memory bank is enough for the present lesson targets, but later scaling may need better authoring utilities
- `lw` now assumes a real data-segment-style base address and should keep doing so unless the whole pedagogy changes on purpose


### 2026-07-05 - sw Added And Memory Phase Closed Out

Completed:
- implemented `sw` on top of the existing memory-unit and data-memory-bank structure
- validated the full `sw` lesson path through:
  - fetch
  - decode with `rs`, `rt`, and immediate offset responsibilities
  - ALU address calculation
  - Memory Access write
  - recap without write-back
- confirmed that memory writes persist inside the shared `DataMemoryBank` instead of resetting on lesson restart
- finished the memory phase as a real two-path interaction:
  - `lw` reads from memory and spawns a `Memory Data` packet
  - `sw` writes store data into the addressed memory word
- confirmed addressed-word highlighting and central readout remain usable during the memory step

Changed:
- the memory phase is no longer just a special case for `lw`; it now cleanly supports both load and store behavior
- `sw` now acts as the first instruction that fully exercises:
  - base register + immediate address formation
  - carried store-data packets
  - memory mutation without register write-back

Next:
- add the remaining arithmetic/logic instructions that can reuse the existing fetch/decode/execute/write-back framework
- clean up datapacket lifetime so packets disappear once a phase has actually consumed them
- keep memory persistence unless a future lesson mode explicitly asks for pristine bank resets

Risks / Notes:
- memory persistence across lesson restarts is currently considered a feature, not a bug
- later lesson modes may still want an optional "reset memory bank" path if testing repeatability becomes more important than continuity
- the next supervisor-facing risk is no longer missing datapath functionality; it is readability, pacing, and presentation polish


### 2026-07-05 - Remaining ALU Instructions Added And Packet Lifetime Cleaned Up

Completed:
- added `sub`, `and`, `or`, and `slt` as real instruction-definition-driven lesson paths
- validated that the existing fetch -> decode -> execute -> write-back structure can now support:
  - multiple R-type ALU operations
  - immediate execution through the extender path
  - memory-bearing instructions through the shared Mem phase
- updated lesson flow so non-memory instructions skip Mem cleanly while still telling the learner what the next phase will be
- updated lesson flow so `sw` skips write-back and ends with recap instead
- moved immediate packet spawning to the `Immediate Extender` instead of piggybacking on the second register scanner
- cleaned datapacket lifetime so packets are consumed only when their owning phase has actually used them
- kept the current working set validated across:
  - `add`
  - `addi`
  - `lw`
  - `sw`
  - `sub`
  - `and`
  - `or`
  - `slt`

Changed:
- immediate-based decode now finishes by prompting the learner to press Continue, which spawns the immediate packet at the authored extender
- ALU and memory feedback now more clearly announce whether the next stage is Memory Access, Write Back, or recap
- the current prototype is no longer only a three-instruction V1 slice; it is now a broader, still-guided single-cycle datapath lesson set

Next:
- post-demo polish and cleanup
- decide the final pedagogical home of `RegDst` and `ALUSrc`
- design the next meaningful extension rather than inflating complexity for its own sake

Risks / Notes:
- the lesson framework is now broad enough that wording clarity matters more than raw phase existence
- future work should continue favoring reusable instruction definitions over one-off phase hacks


### 2026-07-07 - Multi-Panel UI Refactor Started, With IF Embodiment Planned Next

Completed:
- began refactoring the authored lesson UIs away from single overloaded panels and toward a cleaner three-panel pattern:
  - lesson panel for concept + task framing
  - interaction panel for the active action + progression control
  - hint panel for optional lookup/reference support
- rewired `Instruction Decode` around that structure first
- then rewired the same runtime pattern for:
  - `EX`
  - `Mem`
  - `WB`
- moved more static lesson wording out of code and into scene-authored UI blocks
- kept runtime text focused on:
  - instruction-specific values
  - live status
  - feedback
  - phase progression
- confirmed the code side still works with the new authored split

Changed:
- the UI architecture is now moving toward "editor-authored first, runtime fills in only the changing pieces"
- hint/reference content is now intended to be panelized and optional instead of crammed into one scrolling panel
- the next major teaching/presentation improvement is no longer another datapath phase, but making `IF` physically meaningful

Next:
- finish the three-panel split across the remaining lesson UIs
- add a datapacket reset path
- improve the map / environment presentation
- build the planned `Instruction Module` + `Instruction Terminal` flow so:
  - lesson start uploads the selected instruction into a physical module
  - the learner carries that module to the decode zone
  - instruction fetch becomes a visible physical handoff instead of only UI framing

Risks / Notes:
- the new UI split should improve readability, but it still needs a final pass on wording and scene layout
- the instruction-module flow should remain scoped:
  - enough to make `IF` meaningful
  - not so much that it delays the presentation polish pass


### 2026-07-07 - Instruction Fetch Terminal Flow Implemented

Completed:
- added a physical instruction-fetch handoff through:
  - `Instruction Module`
  - uploader `Instruction Terminal`
  - downloader `Instruction Terminal`
- wired lesson startup/reset so a fresh blank module is spawned at the uploader terminal
- wired the selected instruction to upload into that module before the learner carries it away
- gated decode so it only begins once the module is delivered to the decode terminal
- removed the earlier experimental terminal rise/lower animation after it proved unstable
- kept terminal VFX restrained to short upload/download bursts only

Changed:
- `IF` is no longer just explanatory UI framing; it now has a real embodied transport step
- fetch no longer expects the learner to return and manually close the phase after delivery
- the current fetch/decode handoff is now:
  - start lesson
  - module spawns and receives the instruction
  - learner carries it to decode
  - decode unlocks automatically

Next:
- finish the three-panel lesson UI conversion across the remaining zones
- add the datapacket reset path
- move into environment/map work for presentation quality

Risks / Notes:
- terminal motion polish was intentionally removed in favor of stability
- future fetch polish should stay conservative unless it clearly improves readability


### 2026-07-09 - Datapacket Reset Added And Cleanup Checkpointed

Completed:
- added a dedicated `Reset Data Packets` path to complement the existing register reset button
- datapacket reset now restores only loose, non-latched packets to their authored spawn transforms
- kept datapacket value, sign-extension state, and other lesson data untouched during reset
- confirmed that the full gameplay loop still works after the datapacket reset addition
- checkpointed the current lesson-script cleanup pass, including the in-progress split of oversized lesson files into smaller focused parts

Changed:
- datapacket recovery is now separate from lesson reset and separate from register reset
- packet reset deliberately ignores packets that are already latched/validated by active scanners

Next:
- move attention onto map/environment building for the next polish pass
- revisit deeper lesson-code decoupling after the supervisor checkpoint rather than folding it into the current presentation push

Risks / Notes:
- this reset path is intentionally narrow and should stay transform-only unless a later usability issue proves it needs broader behavior
- the lesson-code cleanup is in a safer place now, but the larger architectural cleanup is still deferred


### 2026-07-09 - PC Update, Branch Resolution, And Final Flow Polish Added

Completed:
- added a dedicated `PC Update UI` and `PC Update Station` as the final lesson step instead of routing the learner back through the old intro-panel ending
- implemented branch-resolution support for:
  - `beq`
  - `bne`
- added a `Zero` datapacket result path from the ALU for branch comparisons
- wired the branch route so:
  - ALU compares register operands
  - branch immediate is sign-extended
  - branch immediate is shift-left-2 handled through the PC-update step
  - `PCSrc` is calculated and validated there
- stabilized branch/invalidation behavior across the later datapath zones so signal changes now properly release invalid locked packets instead of silently trapping them
- fixed multiple cross-phase regressions that had appeared during the branching pass:
  - ALU input 2 revalidation
  - memory pedestal refresh loops
  - immediate-extender reset edge cases
  - write-back and PC-update completion handoff
- added the decode-side `funct` substep for R-type instructions so decode now supports:
  - opcode confirmation first
  - funct confirmation second when needed
  - then operand setup
- confirmed the current working instruction set now includes:
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

Changed:
- lesson conclusion now belongs to the PC-update step instead of the earlier recap-only ending
- branch instructions are now modeled as real lesson paths rather than only future design ideas
- the instruction set is now broad enough that remaining risk is mostly presentation, readability, and UX polish rather than missing core datapath routes

Next:
- continue the three-panel UI cleanup and authored-text split
- finish map / environment work for presentation quality
- add remaining quality-of-life items such as datapacket reset

Risks / Notes:
- branch flow currently relies on UI-side `PCSrc` reasoning instead of a more physical branch-control interaction, which is acceptable for now
- the project now has enough instruction coverage for the meeting/demo path, so future work should resist turning into uncontrolled scope growth


Next:
- build the real memory interaction for `lw`
- decide how much of the write-up/reference content should move into a persistent cheatsheet or settings panel
- revisit any remaining UI clarity issues only if they materially affect the next checkpoint

Risks / Notes:
- sign extension is still represented simply in the data model even though the learner now interacts with a physical extender
- `lw` remains the largest unfinished checkpoint item because memory scanning, output, and write-back routing still need real interaction logic


### 2026-07-09 - Post-Meeting Direction Reset Toward Final Polish

Completed:
- completed the routed-map / branch-resolution checkpoint with the supervisor
- validated the broader routed environment as a real lesson path rather than only a sandbox arrangement
- confirmed that the current single-cycle guided build now covers:
  - fetch
  - decode
  - execute
  - memory when needed
  - write-back when needed
  - PC update for branch resolution and lesson conclusion
- received positive supervisor feedback on:
  - the newer map
  - the broader instruction coverage
  - the three-panel lesson UI layout

Changed:
- the main project risk is no longer "can the datapath work?"
- the main project risk is now:
  - can the learner tell where to go next
  - can the learner understand the lesson comfortably
  - can the build feel finished enough for the next checkpoint and the final demo

Next:
- add door gating tied to phase progression
- add directional arrows tied to the active lesson phase
- add tutorial/onboarding support
- improve lighting and route readability
- add audio cues as guidance support
- plan and, if safe, introduce experiment mode before the next meeting

Risks / Notes:
- further large core-system changes should now be treated cautiously
- multicycle/pipeline expansion remains valuable, but should stay documented as future work unless extra time appears


### 2026-07-10 - CPULesson Refactor Stabilized

Completed:
- finished the full `CpuLesson` refactor into a proper foldered structure instead of the older oversized / split-file arrangement
- replaced the earlier lesson layout with focused files under:
  - `Flow`
  - `Support`
  - `UI`
  - shared UI helpers under `Assets/MyScripts/Shared/UI`
- kept the validated gameplay loop working while the refactor was happening instead of treating it as a separate throwaway experiment
- updated the shared lesson panel base so authored UIs can now bind one or several panel roots / scroll views cleanly

Changed:
- lesson lifecycle, fetch/decode progression, decode text/hint helpers, and panel presentation are now easier to reason about file-by-file
- the lesson system is now better aligned with the scene-authored workflow instead of older runtime-heavy assumptions

Next:
- keep further polish work building on top of this refactored lesson baseline
- avoid reintroducing oversized lesson root scripts unless a future rewrite is intentional

Risks / Notes:
- the refactor is now considered the stable baseline
- future cleanup should be incremental rather than another large disruptive reshape right before delivery


### 2026-07-11 - End-Of-Day Map Planning Locked In

Completed:
- reviewed the current routed map after the post-meeting refactor pass
- explored a new grid-based map revision concept to improve pacing between intro, decode, ALU, memory, write-back, and PC update
- stopped diagram iteration once the high-level route direction was clear enough to build directly in-scene

Changed:
- tomorrow's focus is now explicitly environmental/presentation work rather than lesson-logic expansion
- the next short-term target is to tighten navigation and readability before adding anything ambitious

Next:
- map retouch
- UI cleanup
- gate + arrow refinement

Risks / Notes:
- the map-layout discussion was useful for direction, but not precise enough to preserve as a final spatial spec yet
- tomorrow should bias toward practical in-scene iteration rather than more abstract diagram work


### 2026-07-12 - Gated Routing And Guidance Prototype

Completed:
- added lesson-gated door progression tied to lesson phases
- validated moving gates instead of simple active/inactive blockers
- prototyped route arrows as authored scene objects with pulsing guidance behavior
- updated the route prototype so arrows can stay hidden when not needed
- updated the route prototype so arrows in a group can pulse sequentially instead of all in sync

Changed:
- navigation is now starting to become part of the lesson readability strategy, not only a scene-polish extra
- the project can now communicate both:
  - where the learner is allowed to go
  - where the learner should probably go next

Next:
- tune the arrow visual language
- decide how many arrows each route truly needs
- add sound cues only after the visual route language feels clear enough


### 2026-07-14 - Tutorial Direction Narrowed To Video And/Or Coaching Cards

Completed:
- clarified that tutorial/onboarding should now be treated as an active presentation task rather than a vague future polish category
- established that a baseline tutorial/video prefab from `VRTemplateAssets` is already in the scene as onboarding groundwork
- narrowed the likely tutorial directions to two serious options:
  - a recorded custom control/tutorial video
  - a coaching-card style image-and-text walkthrough based on the sample spatial panel setup

Changed:
- the map revamp itself should now be considered done enough that it no longer deserves to sit at the top of the task list as if it were unfinished
- settings should now be treated as baseline-complete, with only optional refinement later
- a separate dedicated cheatsheet is no longer the preferred plan; the existing hint panels are expected to carry most reference/help text

Next:
- choose whether tutorial V1 should be:
  - recorded video
  - coaching cards
  - both if it still stays cheap enough
- move into the sound pass next, since it now looks like the clearest low-risk presentation improvement
- return to experiment mode after the next few polish tasks rather than forcing it immediately

Risks / Notes:
- a video is likely the fastest "good enough" onboarding route, but is less flexible to revise late
- coaching cards are more editable and potentially more reusable later for hints/experiment mode support
- doing both can still be valid, but should only stay on the table if it does not quietly turn onboarding into its own side project

### 2026-07-14 - Wrist Settings Menu Baseline Added

Completed:
- added a real player-facing wrist settings menu into the current prototype flow instead of leaving settings/cheatsheet access as a deferred TODO
- switched the scene-side menu setup onto the `Button Hand Menu` path and validated the existing XR hand-menu system as a usable base
- added controller-side menu-button opening support on top of the existing hand-menu behavior without replacing the package system itself
- added menu readouts for:
  - current instruction
  - current routed phase
- added menu actions for:
  - register reset
  - datapacket reset
  - lesson restart
  - quit
- added menu controls for:
  - master volume
  - route-guidance enable/disable
- added lightweight diagnostics output for:
  - FPS
  - frame time

Changed:
- settings/cheatsheet support is no longer a missing feature category; it now exists as a baseline system that can be expanded later
- the wrist menu now acts as the main in-session quality-of-life hub for:
  - recovery
  - status checking
  - guidance preferences
  - fast presentation testing
- existing world interactions were preserved by exposing shared script entry points instead of replacing the original reset-button behavior

Next:
- decide how much additional lesson-reference content belongs in the wrist menu without overcrowding it
- decide whether experiment-mode toggles should live here later or elsewhere
- keep onboarding and route readability ahead of deeper menu expansion unless a presentation blocker appears

Risks / Notes:
- the base hand-menu anchoring/pose behavior is still fundamentally driven by Unity's existing wrist-menu system, so future comfort tweaks should build on that rather than assuming the helper script owns the whole behavior
- the new menu solved practical access needs, but its content density should stay controlled so it does not become another overloaded lesson panel


### 2026-07-14 - Guidance Polish + Intro Offset Utility

Completed:
- refined the phase arrow guidance scripts into a cleaner reusable route-pulse setup
- added a reusable `AuthoredOffsetLerp` component for future intro/environment settle-in beats
- made the offset utility opt-in so it can sit on scene-authored objects without affecting them until explicitly enabled

Changed:
- the current visual polish pass now includes two lightweight support systems:
  - phase-authored route arrows
  - a dormant intro-offset helper for walls / ceilings / platforms
- this pass stayed intentionally narrow and did not reopen lesson architecture

Next:
- use the arrow system to keep tightening route readability in-scene
- decide later whether the offset helper is worth using in the final opening sequence

Risks / Notes:
- Unity XR play-mode startup was inconsistent during this pass, but the issue looked editor/runtime-side rather than caused by the new offset helper
- keep the offset helper disabled by default unless it is being actively trialed in-scene


### 2026-07-16 - Presentation Support Pass Consolidated

Completed:
- treated the reworked routed map as the live baseline instead of an unfinished navigation task
- kept the settings menu as an established player-support system rather than a deferred feature category
- kept lightweight tutorial/onboarding support in-scene through:
  - the imported tutorial/video prefab
  - simple image-based tutorial surfaces
- completed a broad gameplay audio pass across:
  - gate transitions
  - instruction terminal upload/download events
  - world/menu button interaction
  - phase activation / completion / failure cues
  - lesson completion
  - memory and write-back transfer events
  - datapacket spawning
  - scanner occupied / success / failure feedback
- normalized scanner feedback so incorrect scans now follow a consistent:
  - occupied
  - failure
  visual/audio rhythm instead of skipping straight to failure

Changed:
- the map, settings menu, and first-pass audio layer should now be treated as established presentation support systems
- onboarding is no longer purely hypothetical, but it still remains open to refinement and possible replacement
- remaining polish risk is now less about missing baseline support systems and more about final clarity, comfort, and presentation feel

Next:
- decide how far to push tutorial V1 beyond the existing lightweight setup
- build experiment mode once the guided baseline feels safe enough
- treat further audio work as balancing / ambience / optional flavor rather than missing core feedback

Risks / Notes:
- no background music has been locked in yet, so ambience should still be treated as optional flavor work
- the current audio pass intentionally prioritizes local interaction clarity over theatrical presentation


### 2026-07-17 - Practice-Mode Groundwork And Register-Bank Overhaul

Completed:
- created a real practice-mode groundwork path on the dedicated `practice-mode` branch instead of trying to layer the mode in as an unsafe one-off hack
- extended the lesson architecture so mode selection can sit above instruction selection without replacing the existing guided lesson flow
- updated the intro flow so it can support:
  - lesson mode selection
  - instruction selection scoped by the active mode
  - a cleaner return-to-idle path once `IF` has started
- refactored Unity UI event handling so main menu/dropdown/button events are now intended to be bound through the Inspector instead of added in code at runtime
- kept dropdown population in code while moving event ownership out to scene bindings
- formalized the register bank into a proper reusable `Register Zone` prefab with authored sample values across all 32 MIPS registers
- assigned real data-memory-backed address values to the saved-register range so memory-related lessons can use believable address-bearing registers instead of placeholder repetition
- changed the register-value lifecycle so ordinary lesson starts/resets no longer wipe the whole bank back to fresh values every time
- added a safer authored-value restore path that happens when the lesson mode changes, so moving between `Learning` and `Practice` can re-baseline the bank cleanly
- broadened the current instruction-definition assets so they no longer overuse only the same small handful of registers
- polished multiple phase readouts so accepted values are displayed more cleanly:
  - `ALU` accepted input text now shows just the relevant value
  - `Mem` text no longer repeats redundant packet-role suffixes
  - `WB` now exposes the target register's current stored value for better context
- updated lesson decode scanners so, when no lesson is active, they can act like ordinary preview scanners instead of sitting uselessly inactive
- kept the lesson-only scanner behavior intact once a lesson actually starts

Changed:
- the project is no longer assuming that register values should be freshly scripted per lesson run as the main baseline
- the register bank now behaves more like the data-memory bank in spirit:
  - authored defaults exist
  - local interaction can change runtime state
  - mode changes can safely restore the authored baseline
- practice mode is now a real architecture concern, not just an idea in the backlog
- the intro panel has started shifting from "single guided lesson picker" toward "mode-first lesson entry point"
- scene bindings are now the preferred source of truth for UI event hookup, while code remains responsible for supplying dropdown content and runtime state

Next:
- test the current practice-mode groundwork in-scene and verify the guided mode still behaves exactly as expected after the refactor
- continue the practice-mode implementation through decode-specific behavior rather than mixing it loosely into the learning flow
- keep updating learning-mode instruction assets so the broader register bank is used more intentionally across instruction families
- decide the exact first playable slice of practice mode before widening its decode/input burden too aggressively

Risks / Notes:
- the current practice-mode work is still foundation work, not the finished mode itself
- the biggest near-term risk is accidental regression of the stable guided flow while practice-mode pieces are being added
- the new register-value persistence direction is intentionally more realistic/useful for replay, but it increases the importance of keeping authored reset boundaries explicit


### 2026-07-18 - Decode Practice Slice Refined And Refactored

Completed:
- finished the first real practice-mode slice around `add` instead of leaving practice mode as only intro/setup groundwork
- kept `IF` and the shared lesson flow architecture intact while layering the new decode behavior onto the existing system
- updated practice decode so it now supports:
  - opcode confirmation first
  - staged `rs`, `rt`, `rd`, and `funct` validation for the `add` path
  - limited hint usage
  - limited answer attempts
  - held failure state that waits for an explicit reset press instead of auto-resetting immediately
- updated the practice decode presentation so the binary instruction display now has clearer runtime framing text
- moved decode-panel serialized reference containers out of the main decode controller into their own helper file
- split repeated practice-decode field behavior into focused helper classes instead of leaving the main practice view to manually manage every dropdown/toggle combination
- changed practice decode validation to read from a captured input-state object instead of repeatedly pulling individual values from the panel controller
- removed a duplicated lesson-guide initialization path by collapsing shared `Awake`/`OnEnable` setup into one helper path

Changed:
- practice mode is no longer only "groundwork"; it now has a real first decode interaction slice for one instruction
- the decode architecture is now cleaner than the first implementation pass:
  - scene refs are separate from controller logic
  - practice field widgets own more of their own behavior
  - practice validation reads one captured state instead of scattered panel calls
- this was intentionally a refactor-in-place, not a second parallel decode stack

Next:
- widen practice mode carefully beyond `add` only after the current decode slice has been judged safe enough
- decide how much of decode guidance should live in lesson text versus hint text before broadening practice coverage
- continue protecting the guided baseline while practice-specific decode logic grows

Risks / Notes:
- the current practice mode is still only a first slice, not the full intended mode
- decode remains the highest-risk area for architecture sprawl, so future additions should keep honoring the split-responsibility direction established here


### 2026-07-18 - Practice Mode Extended Through The Full add Lesson Path

Completed:
- extended the first practice-mode slice beyond decode so the `add` instruction can now be exercised through the later authored lesson phases too
- carried the practice-mode split into:
  - `ALU`
  - `Memory`
  - `WB`
  - `PC Update`
- added per-phase practice budgets for:
  - validation attempts
  - scanner attempts
  - hint usage
- updated practice failure behavior so later phases now hold on a failure end-state until the learner explicitly presses restart
- created shared support helpers and panel-ref containers so the practice additions for later phases did not have to be duplicated ad hoc across every controller
- changed the practice runtime-instruction path so practice definitions can clone/override learning definitions at runtime instead of depending on a second fully duplicated instruction-asset stack

Changed:
- practice mode is no longer just "decode with extra steps"; it now has a real first end-to-end lesson path for `add`
- the later phase controllers now share more of the same practice-mode language:
  - budgets
  - hint flow
  - held failure state
  - restart rhythm
- runtime practice instructions are now treated as lightweight overlays on top of learning-mode instruction definitions rather than separate hand-maintained full lesson copies

Next:
- test new practice instructions such as `lw` and `beq`
- decide whether decode register scanning should remain order-sensitive or become order-free
- continue widening practice mode only after the first complete path remains stable under testing

Risks / Notes:
- the first full practice path exists, but it is still the start of the mode rather than the final shape of it
- later instruction families may still expose differences in how immediate, memory, and branch behavior should be surfaced during practice mode


### 2026-07-18 - Centralized Dev Mode Added For Testing And Recovery

Completed:
- added a centralized dev-mode toggle to the wrist settings menu
- added a single settings-menu button that can skip the currently active lesson phase during testing
- implemented force-complete helpers for:
  - decode
  - execute
  - memory
  - write-back
  - PC update
- patched the first decode-skip implementation after it exposed a same-frame advancement freeze
- updated the decode skip path so it now emits the expected datapackets instead of only stamping internal lesson state
- kept dev tooling anchored to the settings menu rather than scattering separate skip controls onto every phase UI

Changed:
- rapid in-editor / in-headset testing now has a sanctioned path instead of requiring repeated full lesson replays for every late-phase check
- dev support is now a player-facing utility inside the existing settings menu instead of a hidden scene-only workaround

Next:
- use dev mode to accelerate validation of new practice instructions and future scanner tweaks
- decide later whether any extra dev-only utilities are truly necessary beyond phase skipping

Risks / Notes:
- dev mode is intentionally a testing aid, not part of the learner-facing intended experience
- any future expansion of dev tooling should stay centralized and restrained so it does not pollute the actual lesson UX


### 2026-07-18 - Scanner Failure Behavior Stabilized Across Practice Phases

Completed:
- normalized scanner-attempt failure behavior across the practice-enabled authored phases
- ensured later-phase practice failures now shut scanners down when the failure end-state is reached instead of leaving them half-alive and spam-triggerable
- fixed repeated scanner-failure budget draining in decode by deduplicating repeated charges for the same stable wrong placement
- aligned decode more closely with the already safer scanner-failure behavior seen in later phases
- preserved decode idle-preview utility behavior while still restoring lesson-owned scanner strictness once a lesson starts

Changed:
- scanner failure now behaves more like a true phase-end failure condition instead of an endlessly repeatable punishment loop
- the difference between "wrong but still trying" and "failure state reached" is now cleaner for testing and presentation

Next:
- decide whether decode scanner validation should remain ordered by `rs` then `rt`, or become order-free for usability
- keep an eye on scanner behavior when new practice instructions are introduced

Risks / Notes:
- decode still has the most unusual scanner behavior because it mixes operand collection, packet spawning, and lesson progression in one phase
- scanner behavior may still need one more usability pass once `lw` and `beq` practice variants are exercised


### 2026-07-18 - Spatial Keyboard And Input Fields Entered The Lesson Flow

Completed:
- imported the XR Interaction Toolkit spatial keyboard sample into the project workflow
- verified world-space TMP input fields can now accept text correctly inside the authored lesson scene
- identified and corrected the character-limit setup issue that was blocking keyboard text entry
- replaced the settings-menu dev-mode toggle path with a password-backed input submission path
- moved Practice decode away from binary dropdown selection and onto authored input fields for direct bit entry
- removed the temporary one-off input-field debug helper after keyboard setup was confirmed working

Changed:
- Practice decode now expects typed bitfields instead of dropdown picking for:
  - opcode
  - `rs`
  - `rt`
  - optional `rd`
  - optional immediate
  - optional funct
- dev-mode unlocking is now an intentional keyboard submission instead of a visible toggle in the settings menu
- the lesson scene now has a validated path for future text-entry interactions

Next:
- widen the input-field-based Practice path beyond the first instruction slice
- decide how much future practice / test content should rely on typed decode versus constrained authored choices
- reuse the same keyboard-backed flow anywhere later text entry is genuinely worth the friction

Risks / Notes:
- spatial keyboard setup is sensitive to field configuration, especially character-limit handling and submission events


### 2026-07-18 - Practice Instruction Coverage Expanded Across The Full Current Baseline

Completed:
- expanded Practice mode beyond the original `add`-only slice so authored practice instruction assets now exist for:
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
- kept those Practice assets on top of the shared learning-mode lesson flow instead of creating separate full lesson copies
- diversified Practice operands so the mode no longer mirrors the learning-mode register usage too closely
- preserved register-value persistence across ordinary instruction runs so Practice sequences can meaningfully observe changed register state between lessons
- updated the settings-menu instruction readout so Practice mode no longer leaks decoded assembly text during `IF` / `ID`
- treated the register bank and instruction bank as authored reference data worth documenting directly in the journal instead of repeatedly re-deriving them from prefabs and assets

Changed:
- Practice mode should now be treated as a real playable baseline for the current ten-instruction set, not as an `add`-only prototype path
- the next Practice-mode work no longer needs to be "make Practice exist"; it should be refinement, stress-testing, and deciding how much further a later `Test` mode is worth pushing before the deadline
- wrist-menu ergonomics still matter a lot when pairing keyboard use with the existing hand-menu presentation


## Current Working Baseline

### Scene / Interaction Baseline

- `Testing Ground` is the sandbox scene
- the lesson framework is currently driven from `Lesson Guide`
- `Intro UI` is the current lesson start point
- `Instruction Decode UI` is the current instruction-decode and operand-selection panel
- the preferred register path is the authored `Register Bank` with 32 permanent register tokens
- the preferred register validation path is the authored scanner pedestals
- minimal visual feedback is acceptable; heavy animation is not required
- physical instruction fetch now depends on:
  - uploader `Instruction Terminal`
  - `Instruction Module`
  - downloader `Instruction Terminal`
- physical route progression now matters because the lesson space is no longer flat:
  - intro/fetch start
  - decode room
  - ALU platform
  - memory hall
  - write-back area
  - PC-update conclusion platform
- phase routing now also includes:
  - moving gates
  - prototype phase-specific arrow guidance

### Architecture / Script Baseline

Current relevant scripts:
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

1. Add tutorial/onboarding support.
   Focus on:
   - controls
   - lesson rhythm
   - recovery expectations

2. Refine the new settings support only if it materially helps usability.
   Focus on:
   - keeping wording concise enough to fit the wrist format cleanly
   - adding only small quality-of-life options if they clearly help testing or presentation
   - not re-scoping it into a full separate cheatsheet system, since the hint panels are currently expected to cover most reference needs

3. Polish the authored UIs and environment together.
   Focus on:
   - readability
   - lighting
   - path clarity
   - sound-guided feedback
   - experiment-mode feasibility before the next checkpoint

4. Refine the already-built navigation layer.
   Focus on:
   - route arrow tuning
   - gate readability
   - environmental guidance without visual clutter

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
