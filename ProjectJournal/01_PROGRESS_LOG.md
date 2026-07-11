# Progress Log

## Current Status

Project phase:
- post-meeting polish and final-delivery pass after the routed-map / branch-resolution checkpoint

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype focus:
- playable `add`, `addi`, `lw`, `sw`, `sub`, `and`, `or`, `slt`, `beq`, and `bne` lesson loops in `Testing Ground`
- scene-authored `Intro UI`, `Instruction Decode UI`, `ALU UI`, `Mem UI`, and `WB UI` under `Lesson Guide`
- scene-authored route gating and first-pass arrow guidance now prototyped for wayfinding
- authored 32-register MIPS bank with local reset
- per-register logical values now supported in code
- register scanner validation path for decode-stage source operands
- first-pass ALU execution phase now wired with authored ALU UI, physical ALU buttons, and result spawning
- immediate packet generation and sign-extension path now working for `addi`
- ALU funct-selection path now working through the authored dropdown for the secondary instruction path
- dedicated write-back phase now present through a `WB` prefab and authored `WB UI`
- dedicated memory phase now present through a `Memory Unit`, `Data Memory` bank, and authored `Mem UI`
- memory phase is now concluded as a real interaction step for both `lw` and `sw`
- datapackets are now cleaned up when the phase that consumed them actually finishes
- physical instruction fetch now exists through:
  - `Instruction Module`
  - uploader `Instruction Terminal`
  - downloader `Instruction Terminal`
  - decode gating on successful delivery
- authored `PC Update UI` and first-pass `PC Update Station` now exist for branch resolution and lesson conclusion
- dedicated datapacket reset support now exists for loose, non-consumed packets
- keeping lesson code tied to existing scene objects instead of building UI at runtime
- treating the new routed map as part of the lesson experience instead of only scenery
- preparing navigation, tutorial, audio, and presentation polish on top of the now-working instruction set and branch flow
- user is taking the next day off for recovery, so the current checkpoint should preserve enough context to resume directly into polish work

Current milestone:
- June 29, 2026 supervisor demo completed
- routed-map / branch-resolution checkpoint completed
- next supervisor checkpoint target: `2026-07-24`
- official demo deadline: `2026-07-29`
- preferred internal finish target:
  - complete the build a few days before the official deadline
  - use the remaining time for participant prep and presentation safety
- current checkpoint status:
  - `add`, `addi`, `lw`, `sw`, `sub`, `and`, `or`, `slt`, `beq`, and `bne` are working
  - current focus has shifted from instruction coverage to:
    - wayfinding
    - tutorial quality
    - UI readability
    - map polish
    - sound/design polish
    - experiment-mode planning

## Current TODO Cutoff Before The Next Supervisor Meeting

The user-defined cutoff for the next meeting is:

1. Door gating + arrow system
2. Add at least some jump instructions if the existing system can absorb them safely
3. UI polishing
4. Add proper lighting to the map, and tighten the path if needed
5. Proper tutorial UI, and possibly a settings / cheatsheet UI
6. Proper sound pass
7. Final polish
8. Experiment mode without handholding before the meeting if feasible
9. Optional helper NPC / guide character if time remains
10. Optional in-scene music player / ambient music interaction if it strengthens presentation

Interpretation:
- the core instructional loop is already present
- the next meeting should emphasize usability, wayfinding, onboarding, and presentation quality rather than another major systems rewrite
- jump work is stretch scope, not the first priority
- unless a blocking bug appears, polish/navigation/tutorial work should win over new datapath invention

## Post-Meeting Backlog (Ordered Recommendation)

Recommended order after the current meeting cutoff:

1. settings menu + cheatsheet panel
   Why:
   - directly supports the supervisor feedback about separating explanation from interaction
   - gives the learner a persistent reference surface without forcing guide panels to stay overloaded

2. proper intro + controls tutorial
   Why:
   - improves onboarding immediately
   - reduces friction before adding more instruction complexity

3. proper exit option / cleaner end-state flow
   Why:
   - makes the build feel more complete and presentation-ready

4. additional immediate-family instructions:
   - `slti`
   - `andi`
   - `ori`
   Why:
   - relatively natural extensions once the immediate path is stable
   - may expose the need to distinguish sign extension from zero extension more clearly

5. `j` / `jal` / `jr`
   Why:
   - the system is already partway toward control-flow variety
   - but these should stay secondary to the polish pass

6. `lui`
   Why:
   - possible, but more specialized

7. deeper audio / VFX polish
   Why:
   - worthwhile once guidance and clarity are already safe

8. future multicycle / pipeline extension
   Why:
   - this is now the clearest long-term upgrade path
   - it should be documented and discussed even if not implemented before the final demo

Rationale:
- polish and onboarding now come before raw scope growth
- the instruction additions expand scope only after the learner experience is easier to follow
- multicycle/pipeline belongs to future-work framing unless time opens unexpectedly

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
- a working lesson flow that now runs through fetch -> decode -> execute -> memory when needed -> write-back when needed
- a routed environment that now physically separates the lesson into fetch, decode, execute, memory, write-back, and PC-update spaces
- a smaller lesson architecture centered on focused lesson and register scripts
- cleaned instruction assets for `add`, `addi`, and `lw`
- cleaned instruction assets for `sw`, `sub`, `and`, `or`, and `slt`
- a working first-pass `ALU` execution loop for `add`:
  - register scanners emit `Read Data 1` / `Read Data 2` packets
  - authored ALU input zones accept the correct packet types through child trigger scanners
  - physical `ALUOp` / `ALUSrc` buttons drive execution setup
  - the authored `ALU UI` validates execution and gates the continue into write-back
  - the ALU emits an `ALU Result` packet with the computed value
- a working first-pass write-back loop for `add`:
  - authored `WB` prefab with separate register and datapacket inputs
  - authored `WB UI` for control-signal and input-status teaching
  - bonus loose register scanner for value inspection outside the lesson-gated decode flow
  - final register value update happens through the write-back phase instead of being implied
- an explanatory `Mem UI` checkpoint now exists between execution and write-back
- a cleaner instruction-decode model:
  - `add` scans `rs` and `rt`
  - `addi` and `lw` scan `rs` only
  - immediate-based instructions spawn the `Immediate` packet from the second scanner's packet spawn location
  - destination register choice is now intended for write-back rather than register decode
- a simple sign-extension placeholder path in code:
  - immediate packets now carry a boolean that later phases can validate
  - this keeps `addi` / `lw` unblocked before a physical sign-extension interaction is authored
- datapacket lifetime is now better matched to datapath use:
  - ALU consumes its accepted input packets once the ALU result is produced
  - Memory consumes address/store packets once the memory interaction has completed
  - write-back still owns the final register value update
- branch resolution now finishes through a real `PC Update` step instead of bouncing back to an earlier generic panel
- lesson progression can now physically gate map traversal through authored doors/gates
- first-pass route arrows now exist to support navigation across the larger map

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

1. Add door gating + arrow-guidance support.
   Focus on:
   - unlocking the next valid route by phase progression
   - reducing confusion about where to head next

2. Add tutorial/onboarding support.
   Focus on:
   - controls
   - lesson rhythm
   - recovery expectations

3. Polish the authored UIs and environment together.
   Focus on:
   - readability
   - lighting
   - path clarity
   - sound-guided feedback
   - experiment-mode feasibility before the next checkpoint

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
- gate + arrow introduction

Risks / Notes:
- the map-layout discussion was useful for direction, but not precise enough to preserve as a final spatial spec yet
- tomorrow should bias toward practical in-scene iteration rather than more abstract diagram work
