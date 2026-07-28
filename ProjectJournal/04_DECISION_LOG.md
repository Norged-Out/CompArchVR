# Decision Log

This file captures the decisions I want preserved across sessions, even when the surrounding session context is gone.

Everything in this file is historical by design.
Do not read an older decision as the live project state without checking:
- `ProjectJournal/01_PROGRESS_LOG.md`
- `ProjectJournal/03_PROJECT_SNAPSHOT.md`

Use this file for:
- preserved rationale
- design tradeoffs
- scope decisions that still matter later in the dissertation or presentation

Entries below are kept in chronological order from oldest to newest.


## 2026-06-16 - The Existing Instruction Scripts Should Be Refined, Not Scrapped

Decision:
- keep `InstructionSystem` as useful scaffolding

Why:
- the current plan still benefits from instruction definitions, runtime selection, and UI layout data
- the main needed change is moving toward stage-driven validation, not abandoning the architecture entirely

Implication:
- future work should evolve the scripts rather than restart from zero unless a very strong reason emerges


## 2026-06-16 - V1 Must Deliver add, addi, and lw by June 28

Decision:
- the prototype is not forever limited to only `add`, `addi`, and `lw`
- however, those three instructions are the mandatory V1 target for now
- they should be finished by `2026-06-28`
- I intended to demo that V1 to the supervisor on `2026-06-29`

Why:
- this creates a concrete milestone instead of an open-ended prototype
- it preserves scope discipline while still allowing later expansion
- it aligns development with the next supervisor checkpoint

Implication:
- near-term planning should optimize for getting those three lessons stable
- anything beyond them is secondary until the V1 milestone is safe

Status:
- this milestone has now been achieved and should be treated as historical context, not the current live target


## 2026-06-16 - Repo Journal Must Be Maintained After Meaningful Work

Decision:
- after meaningful development sessions, relevant journal files should be updated

Why:
- I wanted persistent project memory inside the repository
- this helps with context loss, interrupted sessions, and long-gap continuity

Implication:
- updating the journal is part of the workflow, not optional housekeeping


## 2026-06-16 - Lesson Runtime Should Stay Split Across Focused Scripts

Decision:
- avoid bloating one controller with scene setup, validation, UI construction, and runtime interaction creation

Why:
- I had already concluded that the old lesson controller was too bloated and hard to trust
- smaller scripts make the prototype easier to reason about and safer to extend into `addi` and `lw`

Implication:
- future lesson work should preserve separation of concerns unless there is a strong reason to merge responsibilities


## 2026-06-16 - Scene Layout Should Be Co-Authored, Not Silently Built In Code

Decision:
- when placement and visual feel matter, prefer scene-authored layout with direct in-editor review
- runtime-generated UI and interaction objects are acceptable only as fallback scaffolding

Why:
- layout and presentation decisions need to be eyeballed directly in Unity
- code-generated layout can be technically functional while still feeling wrong in-scene

Implication:
- future work should favor scene collaboration and authored layout instead of defaulting to procedural construction


## 2026-06-16 - The "Why VR?" Answer Must Be Educational, Not Cosmetic

Decision:
- justify VR through guided spatial reasoning, not novelty

Why:
- the project's motivation comes from earlier teaching experience during four consecutive semesters as a Computer Organisation teaching assistant at the University of Arizona
- the same office-hours problems kept repeating:
  - students struggled to navigate the datapath
  - students struggled to map control signals to instruction behavior
  - students struggled to reason through value flow, stacks, and function-call-related state changes
- students benefited most when I could draw over the datapath on a whiteboard or iPad and guide them through it step by step
- VR should recreate that guided tracing process in an interactive embodied form

Implication:
- every interaction should help explain datapath reasoning more clearly than a flat diagram alone


## 2026-06-16 - Scope Should Be Narrow, Not Grandiose

Decision:
- prioritize one clean learning objective before expanding

Why:
- the supervisor explicitly encouraged a smaller, defensible scope
- 5-6 weeks of development time is limited
- a polished narrow experience is stronger than a large unfinished one

Implication:
- success does not require a giant full-architecture simulator


## 2026-06-16 - Use VR For Meaningful Physical Decisions, UI For Reference

Decision:
- split responsibilities between VR interaction and UI support

Use VR for:
- register selection
- datapath routing/mux decisions
- embodied progression through stages

Use UI for:
- opcode/funct/control lookup
- instruction format reminders
- prompts
- explanations
- feedback text

Why:
- this preserves the strengths of VR without turning the lesson into tedious hardware bookkeeping


## 2026-06-16 - Recommended Instruction Implementation Order

Decision:
- implement in this order if possible:
  1. `add`
  2. `addi`
  3. `lw`

Why:
- it introduces complexity one concept at a time
- it gives the best chance of building reusable lesson structure


## 2026-06-16 - Minimal Animation Is Acceptable

Decision:
- do not depend on heavy animation for project success

Acceptable feedback includes:
- correct highlights
- sound cues
- prompts
- visible state changes
- gating/unlocking

Why:
- I had already decided that minimal animation was acceptable
- this reduces polish pressure and protects scope


## 2026-06-26 - Execution And WriteBack Should Use Physical Register Pedestals

Decision:
- handle `EX` / `ALU` and `WB` interactions through authored pedestal zones rather than through abstract UI-only confirmation

Why:
- I wanted the learner to physically place the correct register onto the appropriate lesson target
- pedestal scanning gives a clean path for instructions that reuse the same logical register in more than one role

Implication:
- pedestal objects should become part of the scene-authored interaction set
- each pedestal should validate only when its lesson step is active
- each pedestal should read the placed register token's identity and drive success/failure feedback


## 2026-06-26 - Register Placement Validation May Use A Short Stable-Placement Rule

Decision:
- it is acceptable for a pedestal to require a short stability condition before accepting a register

Examples:
- the token being released
- the token remaining in-zone for roughly 1-2 seconds

Why:
- this reduces accidental brush-triggering while still feeling physical


## 2026-06-26 - Register Bank Should Be Scene-Authored

Decision:
- treat the authored register area in `Testing Ground` as the permanent home for physical MIPS registers

Why:
- important scene layout should not be silently created at runtime
- register spacing, readability, and reachability are visual/physical design decisions that need to be eyeballed in-scene

Implication:
- future work should extend the authored bank instead of replacing it with runtime-generated register UI


## 2026-06-26 - Reuse XR Sample Feel For Register Interaction

Decision:
- build physical registers on top of XR Interaction Toolkit sample interaction feel rather than inventing grab/highlight behavior from scratch

Why:
- the XR sample interactable feel was the preferred interaction baseline
- this reduces risk compared with rebuilding every part of the interaction stack manually

Implication:
- register objects should keep behaving like grabbable sample props, with lesson scripts layered on top


## 2026-06-26 - Register Look Should Stay Close To The Chunky Labeled Design

Decision:
- keep the register tokens visually close to the earlier blocky labeled look that fit the project best

Why:
- the blocky labeled direction proved stronger than the earlier tiny-cylinder pass
- the register bank needs readable labels and obvious affordance, not minimalist placeholder bodies

Implication:
- future register-bank changes should preserve readability, label presence, and visible affordance glow unless a deliberate redesign happens


## 2026-06-27 - Lesson UI Should Be Authored As Zone-Specific World-Space Panels

Decision:
- prefer separate authored lesson panels placed near the relevant interaction zones instead of one giant fixed UI

Why:
- I wanted the learner to physically move through the lesson space
- this keeps prompts close to the action and reduces unnecessary head-turning

Implication:
- `Intro UI` should host lesson start and instruction/decode framing
- the register area should get its own panel for `rs` / `rt` / destination guidance
- later zones can follow the same pattern for `ALU`, memory, and write-back


## 2026-06-27 - Scene Must Be Rescanned After Manual Cleanup

Decision:
- if scene objects are deleted or restructured, treat the current `Testing Ground` hierarchy as the source of truth and rescan it before extending the system

Why:
- several earlier mistakes came from relying on stale assumptions about the scene
- `Testing Ground` is being actively co-authored inside Unity

Implication:
- future work should inspect the live scene state first
- dead code tied to removed scene structures should be pruned rather than preserved automatically


## 2026-06-28 - Current Working MVP Is Intro-To-Register

Decision:
- treat the current working baseline as a minimal intro-to-register `add` lesson

What currently works:
- the lesson starts from `Intro UI`
- the learner is introduced to the instruction there
- the flow then hands off to `Register Setup UI`
- the learner places registers through the authored scanners in the register zone

Why:
- I had already tested the current scene and confirmed that it worked so far
- this is the safest baseline to refine instead of destabilizing it with bigger rewrites

Implication:
- future work should polish and extend this exact path
- `addi` and `lw` should grow from this verified baseline, not replace it


## 2026-06-28 - Authored Lesson Panels Must Use Layout Components, Not Fixed Runtime Text Assumptions

Decision:
- keep `Intro UI` and `Register Setup UI` as authored scroll-panel layouts whose content is resized by Unity layout components
- let code update existing text/button content and force layout rebuilds after changes

Why:
- the earlier issue was not the panel concept itself, but the mismatch between runtime text updates and authored layout sizing
- I had already verified that this scene-authored + code-rebuild approach worked

Implication:
- future lesson panels should follow the same pattern
- if a panel text changes at runtime, code should rebuild the layout instead of assuming the authored size will update itself


## 2026-06-28 - Control Decode Success Should Require One Explicit Continue Press

Decision:
- after the learner sets the correct control signals, the decode step should show success feedback and require one more button press to proceed

Why:
- this is clearer pedagogically than auto-advancing the moment the answer becomes correct
- it gives the learner a visible sense of completion before moving on

Implication:
- future gated lesson steps can reuse this interaction rhythm when immediate auto-progression feels too abrupt


## 2026-06-28 - Register Values Should Be Separate From Physical Reset

Decision:
- register tokens may carry logical values, but the local bank reset button should only reset pose / scanner / visual state

Why:
- I wanted lesson-time register values that could differ from the default zero state
- wiping those values whenever a lost token is returned home would make iteration and teaching flow worse

Implication:
- logical register value reset should stay under lesson/runtime control
- physical prop reset and lesson-value reset remain separate concerns


## 2026-06-28 - ALU_V1 Is The Next Focus Branch

Decision:
- the next dedicated implementation branch should focus on the `ALU` / execution step for the current lesson flow

Why:
- `Intro UI`, `Control Decode UI`, and `Register Setup UI` now form a workable MVP baseline
- the biggest missing instructional piece in the current `add` walkthrough is the execution step itself

Implication:
- the next extension should add an authored `ALU` zone before trying to build out memory behavior
- `Data Memory` should remain unused for `add` while the ALU interaction is being stabilized


## 2026-06-28 - Scanner Value Display Is Preferred Over Always-On Register Value Display

Decision:
- prefer showing the active value at the scanner / datapath stage rather than forcing every register token to permanently foreground its value

Why:
- the register pieces are easier to read when they primarily communicate identity
- the value becomes pedagogically meaningful at the moment the datapath reads it
- this fit the ALU plan better, where scanned values turn into carried data packets

Implication:
- future scenes can still show values on tokens if needed, but the main teaching emphasis should stay on phase-specific value exposure


## 2026-06-28 - ALU Should Consume Data Packets Rather Than Raw Register Tokens

Decision:
- after successful register scanning, later phases should work with emitted datapath value packets instead of continuing to move the original register token through every stage

Why:
- this more closely matches the datapath mental model:
  - choose register
  - read value
  - carry value into ALU / memory / write-back logic
- it keeps register identity selection separate from value flow
- it makes later `addi` and `lw` extensions cleaner

Implication:
- ALU and later memory interactions should be built around packet scanners / result tokens
- the existing register bank remains the source of operand identity, not the universal prop for every later phase


## 2026-06-28 - Control Decode Is A Real Gated Phase Between Intro And Registers

Decision:
- the active lesson order is now:
  1. `Intro UI`
  2. `Control Decode UI`
  3. `Register Setup UI`

Why:
- this order was already defined directly in the scene
- the decode step is part of the intended instruction-decode phase, not a side experiment
- treating the flow explicitly keeps future work grounded and prevents circular redesign

Implication:
- future lesson extensions should preserve this authored handoff unless the lesson structure is deliberately changed
- later zones such as `ALU`, memory, and write-back should grow after this baseline rather than replace it


## 2026-06-28 - Core Lesson Objects Should Be Inspector-Wired, Not Found At Runtime

Decision:
- core lesson scene objects should be assigned through serialized Inspector references whenever practical

Why:
- scene objects should be bound in edit mode
- runtime scene searches made the system less trustworthy and less aligned with the authored-scene workflow
- the current lesson flow is easier to reason about when the scene wiring is visible in the Inspector

Implication:
- `LessonGuideController`, `ControlDecodeController`, and `CpuLessonFlow` should prefer serialized refs for authored panels, buttons, and banks
- future work should not default to `Find*` or name-based hookup for the main lesson path


## 2026-06-29 - July 6, 2026 Checkpoint Should Prioritize addi And lw

Decision:
- the next concrete target is the July 6, 2026 checkpoint
- the desired deliverables for that checkpoint are:
  - `addi`
  - `lw`

Why:
- `add` is now demonstrable
- the next value comes from extending the framework to one immediate instruction and one memory instruction

Implication:
- near-term work should prioritize:
  - immediate packet generation + sign extension
  - memory-phase design and implementation
  - minimal ALU-control refinement only as needed to support those two instructions


## 2026-06-29 - Local Register Reset Must Be Pose-Only

Decision:
- the register-bank reset button should only restore register piece positions

Why:
- I wanted it to recover moved props without disturbing lesson state
- clearing packets or successful scans made the interaction feel wrong mid-lesson

Implication:
- local register reset should not:
  - destroy data packets
  - clear successful scanner colors
  - deactivate scanners
  - wipe logical values


## 2026-06-29 - Immediate Sign Extension Can Stay Boolean For Now

Decision:
- represent immediate sign extension with a simple boolean flag on the datapacket for now

Why:
- the next checkpoint needs `addi` and `lw` functioning more than it needs a fully physical sign-extension contraption
- this keeps the code path ready while leaving room for a later authored interaction

Implication:
- immediate packets can be validated by later phases as either sign-extended or not
- a future physical sign-extension station can update that same packet state instead of forcing a data-model rewrite


## 2026-06-29 - Control Decode Stops Before ALU-Specific Signals

Decision:
- `Control Decode UI` should validate only the 6 non-ALU signals for now
- `ALUOp` and `ALUSrc` belong to the execution phase

Why:
- those responsibilities had already been moved onto the ALU itself
- this keeps decode focused and lets execution teach ALU-specific configuration in the right place

Implication:
- `ControlDecodeController` should not gate on `ALUOp` / `ALUSrc`
- `AluExecutionController` and the authored `ALU UI` now own those checks


## 2026-06-29 - Instruction Decode Should Gather Only The Operands Actually Read

Decision:
- treat `ID` as source-operand preparation, not destination confirmation

For the current teaching flow:
- `add` decode should scan `rs` and `rt`
- `addi` decode should scan `rs` and spawn an immediate packet
- `lw` decode should scan `rs` and spawn an immediate offset packet
- destination register choice should be deferred to write-back

Why:
- this matches the register-file read ports more cleanly
- it keeps decode from doing write-back work too early
- it makes the later write-back interaction more meaningful

Implication:
- the register phase should no longer require `rd`
- immediate packets may be spawned from the second scanner's authored spawn location
- write-back should own the final target-register confirmation


## 2026-06-29 - RegDst And ALUSrc Placement Still Needs Pedagogical Review

Decision:
- leave the current flow stable for the demo, and revisit exact control-signal ownership after write-back is complete

Open tension:
- `ALUSrc` is the clean signal for deciding register operand 2 vs immediate operand
- `RegDst` is the clean signal for deciding write-back target register
- but the most understandable lesson placement for these may not match the earliest point they are derived in hardware

Why:
- the write-back phase is the most important missing piece before the demo
- destabilizing phase ownership now is not worth the risk

Implication:
- future work should explicitly decide whether:
  - `ALUSrc` stays purely in `EX`
  - `RegDst` stays purely in `WB`
  - or either one is exposed earlier for teaching clarity


## 2026-06-29 - Write-Back Should Become Its Own Physical Phase

Decision:
- move away from the temporary "write-back explanation on Intro UI" approach and build a dedicated write-back interaction with its own prefab and authored UI

Why:
- the learner should explicitly confirm both:
  - which register is being written
  - which datapath value is being written
- this keeps the final state change legible and gives `addi` / `lw` a reusable end-stage pattern

Implication:
- `WB` should own:
  - `RegWrite`
  - target-register confirmation
  - final value-source confirmation
  - the actual register-value update
- `Mem UI` can remain explanatory-first for the demo while write-back becomes the real next interaction milestone


## 2026-06-29 - Keep The Difficulty Ramp Progressive

Decision:
- keep the current phased ramp where interaction complexity increases as the learner moves forward through the datapath

Current shape:
- `IF` = light introduction
- `ID` = first meaningful register/scanner interaction
- `EX` = richer interaction with physical controls and changing UI
- `WB` = final combined interaction phase

Why:
- the supervisor explicitly approved this gradual ramp
- it makes the lesson feel more teachable than dumping every control signal on the learner at once

Implication:
- future work on `addi` and `lw` should preserve this pacing
- adding more signals should be justified by the teaching sequence, not by hardware purity alone


## 2026-07-02 - The Next Checkpoint Focus Is Memory, Not More Refactoring

Decision:
- treat `lw` memory-phase implementation as the next main work item

Why:
- `add` and `addi` are now both functioning through the current authored lesson loop
- the biggest remaining checkpoint risk is not lesson sequencing anymore; it is the lack of a real memory interaction

Implication:
- the next session should prioritize memory scanning, memory output, and `lw` handoff into write-back
- further cleanup or polish should stay secondary unless it directly blocks the memory lesson


## 2026-07-02 - Immediate Sign Extension Should Be Taught Through A Physical Extender, But Still Stored As A Simple Packet State

Decision:
- keep the datapacket data model simple with a sign-extension boolean
- teach the learner the concept through a dedicated physical `Immediate Extender`

Why:
- this preserves a clean implementation path for `addi` and later `lw`
- it gives the learner an explicit interaction for sign extension instead of hiding the concept entirely in code
- it avoids another data-model rewrite while still making the phase visible and teachable

Implication:
- ALU-side validation should reject an immediate packet until the extender has marked it sign-extended
- later memory/branch work can reuse the same packet state instead of inventing a second immediate representation


## 2026-07-04 - lw Uses Real Data-Segment Addresses

Decision:
- keep `lw` aligned with real MIPS-style addressing instead of simplifying memory addresses down to tiny local offsets

Why:
- I wanted the memory bank to resemble the MARS / MIPS mental model
- the authored 24-word memory bank already maps cleanly to a contiguous data-segment slice
- this makes ALU-result packets and memory lookup behavior easier to explain alongside outside tools

Implication:
- base registers used by memory instructions should hold real data-segment addresses such as `0x10010000`
- immediates remain offsets
- the ALU result should be a full mapped address before memory lookup occurs


## 2026-07-04 - Memory Unit Owns Mem UI Directly

Decision:
- let `MemoryUnitController` be the only script that owns the authored `Mem UI` interaction state

Why:
- sharing Mem UI responsibility between the lesson-guide layer and the memory-phase controller caused button visibility and phase-state conflicts
- the memory phase now has enough real interaction logic that it should manage its own UI state directly

Implication:
- `LessonGuideController` should only decide whether the Mem panel is shown
- `MemoryUnitController` should own:
  - Mem body/status/feedback text
  - Mem action button state
  - Mem phase progression handoff


## 2026-07-05 - If More Instructions Are Added Next, Favor Immediate And Branch Paths Before Jump Paths

Decision:
- if instruction expansion resumes soon, prefer:
  - `slti`, `andi`, `ori`
  - then `beq`, `bne`
  - then `j`, `jal`, `jr`
  - with `lui` as optional/low priority

Why:
- immediate-family instructions reuse more of the current datapath and lesson scaffolding
- branch instructions are pedagogically important and closer to the existing single-cycle story than jump-specific control-flow work
- jump instructions are worthwhile but likely require a more deliberate rethink of PC/control-flow presentation

Implication:
- future expansion should continue reusing the current framework where possible instead of jumping too early into a bigger control-flow redesign


## 2026-07-05 - The Next Supervisor-Facing Cutoff Should Prioritize Presentation Over New Core Systems

Decision:
- before the next supervisor meeting, focus on:
  - splitting lesson UI into clearer guide / interaction surfaces
  - adding a dedicated datapacket reset path
  - improving the environment / map presentation

Why:
- the existing lesson agenda is now largely accounted for functionally
- the bigger risk for the next meeting is readability, cleanliness, and perceived completeness

Implication:
- treat new instruction families and larger pedagogical redesigns as post-cutoff work unless a blocker appears


## 2026-07-05 - Immediate Packets Should Spawn From The Immediate Extender

Decision:
- move immediate packet spawning away from the second register scanner and onto the authored `Immediate Extender`

Why:
- this separates source-register scanning from immediate-path interaction more cleanly
- it avoids overlap/confusion when immediate-bearing instructions also need other packet behavior during decode

Implication:
- immediate-based decode should finish by telling the learner to press Continue
- that Continue action should spawn the immediate packet at the extender, where sign extension can then become a visible physical sub-step


## 2026-07-05 - Datapackets Should Be Consumed By The Phase That Actually Uses Them

Decision:
- once a datapacket has served its purpose in a phase, that phase should consume it

Why:
- leaving spent packets behind caused clutter and confusing leftovers between instruction runs
- datapath packets are easier to reason about when they exist only while they are still logically "in flight"

Implication:
- ALU should consume accepted input packets after producing the ALU result
- Memory should consume accepted address/store-data packets after finishing its transfer
- write-back remains responsible for the final register-value change at the end of the flow


## 2026-07-05 - Only lw And sw Should Visit The Interactive Memory Phase

Decision:
- keep the authored Memory phase only for instructions that actually touch data memory

Why:
- `add`, `addi`, `sub`, `and`, `or`, and `slt` become cleaner when they move straight from `EX` to `WB`
- the learner still needs the lesson text to explain the next stage, but does not need a fake Memory interaction when the datapath skips it

Implication:
- only `lw` and `sw` should open the real `Mem UI` / `Memory Unit`
- `sw` should end with recap after Mem rather than visiting write-back


## 2026-07-07 - Lesson UIs Should Move To A Three-Panel Pattern

Decision:
- each active lesson zone should move toward three authored panels:
  - lesson
  - interaction
  - hint / info

Why:
- the single-panel approach worked, but it stayed too crowded
- the supervisor specifically wanted explanation separated from interaction more clearly
- this structure keeps the guide content visible without making the active interaction area too busy

Implication:
- static explanatory text should be authored in-scene where practical
- runtime should mostly update the changing pieces instead of rebuilding whole text walls


## 2026-07-07 - Stability Beats Fancy Terminal Animation

Decision:
- the attempted raise/lower terminal animation should remain removed unless it is later rebuilt in a clearly stable way

Why:
- the movement introduced bugs around module parenting, spawning, and terminal state
- presentation value was not worth the interaction instability

Implication:
- the current baseline is static terminals plus clear material/VFX state changes
- future polish should not reintroduce terminal movement casually


## 2026-07-07 - Decode Should Unlock Automatically After Successful Module Delivery

Decision:
- once the learner docks the fetched instruction module at decode, the lesson should hand off directly into Instruction Decode

Why:
- forcing the learner to return to fetch just to close UI is unnecessary friction
- the physical carry itself is already the meaningful fetch interaction

Implication:
- fetch acts as a transport gate, not a second manual confirmation checkpoint
- decode UI should become the next active authored surface as soon as the module is accepted


## 2026-07-07 - Instruction Fetch Should Become A Physical Module Handoff

Decision:
- the next meaningful `IF` upgrade should use a physical `Instruction Module` and `Instruction Terminal`

Why:
- the current fetch step is conceptually fine, but physically thin
- carrying a module from fetch into decode gives `IF` an actual embodied purpose
- it also creates a natural gate before `ID` begins

Implication:
- the platform should spawn or host a module
- lesson start should upload the selected instruction into that module
- decode should unlock only after the learner brings the module into place
- later extension into additional phase-gating is optional, not mandatory


## 2026-07-07 - Fetch Terminal VFX Should Stay Restrained

Decision:
- keep fetch-terminal particles short and event-based only

Why:
- constant or repeated VFX quickly became annoying during testing
- a calmer presentation was preferable to spectacle

Implication:
- play a short burst on upload
- play a short burst on download
- avoid ambient or looping terminal effects unless explicitly re-requested later


## 2026-07-09 - Multicycle / Pipelining Should Be Documented As The Main Future Extension Path

Decision:
- do not force multicycle/pipeline implementation into the current pre-demo build
- do document it as the clearest future upgrade path

Why:
- I already had a plausible concept for extending the instruction-module system across later phases
- it would be valuable to discuss in the dissertation and presentation as the natural "what comes next"
- it is likely too large and risky to wedge into the current polish window

Implication:
- the post-demo or dissertation-facing roadmap should frame:
  - additional download terminals per phase
  - multiple instructions in flight
  - stall / hazard explanation
as the next major evolution of the project


## 2026-07-09 - Signal Changes Should Release Invalid Latched Inputs

Decision:
- when a control signal changes in a way that invalidates an accepted datapath input, that input should be released rather than silently remain latched

Why:
- keeping the old locked value in place hides mistakes instead of teaching the learner to correct them
- I preferred correction-through-reinteraction over handholding
- this behavior is more robust across ALU, memory, write-back, and branch-facing interactions

Implication:
- later phase controllers should keep respecting signal-driven validity
- accepted datapackets should not remain locked if the controlling signal mode no longer matches them


## 2026-07-09 - Datapacket Reset Must Stay Separate And Conservative

Decision:
- keep datapacket reset as a local quality-of-life action that only restores free packet transforms

Why:
- I wanted a datapacket equivalent to register reset, not another lesson-state reset
- validated / latched packets should stay untouched so active phase logic is not accidentally broken
- packet values and sign-extension state should survive reset because the reset is about recovery, not reinitialization

Implication:
- datapacket reset should:
  - affect only loose packets
  - restore only pose / parent / motion state
  - avoid changing value, sign extension, or lesson progress
- it should support recovery without becoming another lesson-state reset


## 2026-07-09 - The Next Major Push Should Prioritize Guidance And Polish Over More Core Datapath Systems

Decision:
- before the `2026-07-24` supervisor checkpoint, prioritize:
  - door gating
  - path arrows
  - tutorial/onboarding
  - UI polish
  - sound cues
  - experiment mode if feasible

Why:
- the guided single-cycle lesson is now broad enough to demonstrate the core concept set
- the remaining risk is not missing datapath phases, but whether the learner knows where to go, what to do, and how to recover when confused

Implication:
- new instruction/system work should be secondary unless it is already half-built and low risk
- navigation clarity is now a first-class requirement


## 2026-07-09 - The Map Is Now Part Of The Teaching Experience, Not Just Scenery

Decision:
- treat the routed environment as part of the lesson design itself

Why:
- the supervisor responded positively to the newer multi-room / multi-platform spatial layout
- the path through fetch, decode, execute, memory, write-back, and PC update now reinforces datapath progression physically
- the map is now doing instructional work, not only decorative work

Implication:
- future polish should improve route readability, spatial pacing, and visibility between major lesson zones
- environment changes that make the lesson harder to read should be treated as regressions


## 2026-07-09 - Branch Resolution Belongs To A Dedicated PC Update Step

Decision:
- treat `PC Update` as its own authored final lesson phase, especially for branch instructions

Why:
- it gives the learner a concrete place to reason about `PC + 4`, branch offset use, and `PCSrc`
- it avoids overloading earlier phases with too much control-flow explanation at once
- it provides a cleaner lesson conclusion than bouncing the learner back to the old intro panel

Implication:
- `beq` and `bne` should finish through `PC Update`
- branch resolution can stay partly UI-driven for now as long as the reasoning remains clear
- future control-flow expansion should build on this end-stage instead of scattering PC logic across unrelated phases


## 2026-07-09 - 3D Audio Is A Guidance Tool, Not Just Cosmetic Polish

Decision:
- treat sound as part of player guidance, not only as presentation polish

Why:
- directional audio emerged as a way to attract the learner toward the next active zone
- the supervisor liked the idea of better wayfinding
- spatial audio can support the new map without adding too much visual clutter

Implication:
- future sound work should prioritize:
  - phase completion cues
  - gate unlock cues
  - next-destination cues
- keep ambience secondary to instructional clarity


## 2026-07-12 - Navigation Guidance Should Be Physical, Gated, And Minimal

Decision:
- use authored doors/gates plus authored arrow routes to guide the learner through the map

Why:
- the routed scene is now large enough that phase order needs environmental support
- the supervisor specifically wanted clearer indication of where the learner should head next
- visual clutter should stay low, so inactive arrows should disappear instead of remaining onscreen

Implication:
- route arrows should activate only for the currently relevant path
- sequential pulsing is preferred over every arrow flashing in sync, because it reads more like a route than a beacon wall


## 2026-07-14 - Tutorial Planning Should Keep Both Video And Coaching-Card Paths Alive For Now

Decision:
- keep two onboarding/tutorial directions active for now:
  - a custom recorded tutorial video
  - a coaching-card style image/text walkthrough based on the sample spatial-panel setup

Why:
- both options currently look cheap enough to remain plausible without forcing an immediate hard lock-in
- the video path may be the fastest route to a presentable onboarding pass
- the coaching-card path is easier to revise late and may be more reusable later for hints or experiment-mode support

Implication:
- either onboarding path can be tried first
- combining both is acceptable only if it stays small
- pure text remains a fallback, not the preferred presentation target


## 2026-07-14 - Opening Sequence Polish Should Stay Opt-In And Scene-Led

Decision:
- keep the new opening/environment offset helper disabled by default and treat it as an authored polish utility rather than a globally active scene effect

Why:
- I wanted a tiny visual polish pass, not a system that quietly changed the whole scene
- pre-attaching a dormant helper keeps iteration easy while preserving authored placement until a specific object is intentionally opted in
- this fits the broader project preference for scene-authored control over runtime scene-wide behavior

Implication:
- `AuthoredOffsetLerp` should remain safe to leave on objects in a dormant state
- intro-sequence experimentation can happen object-by-object without committing the whole map to the effect


## 2026-07-14 - Settings Menu Should Stay A Lightweight Baseline Support Layer

Decision:
- keep the settings menu as a lightweight support layer built on top of Unity's existing XR wrist/hand-menu system
- reuse existing world actions where practical instead of building separate UI-only logic paths
- treat the menu as an established baseline system, not a missing feature category
- do not plan a separate standalone cheatsheet system unless the hint panels later prove insufficient

Why:
- the existing XR hand-menu prefabs already provide the anchored wrist-menu behavior the project can reuse safely
- reusing validated reset/status behaviors keeps the menu aligned with the authored world interactions
- the wrist menu is already good enough to count as a baseline support system for recovery, status, and quick presentation control
- another parallel reference surface would add scope during the final polish window without clearly improving the lesson

Implication:
- future menu work should focus on refinement rather than replacement
- settings work should stay optional unless a usability blocker appears
- hint panels remain the main reference surface unless later testing proves otherwise


## 2026-07-17 - Practice Mode Should Extend The Existing Lesson Architecture, Not Fork It

Decision:
- build practice mode on top of the same lesson foundation as learning mode
- do not create a second unrelated runtime flow just because the mode has different decode expectations

Why:
- the guided lesson flow already works and is too valuable to destabilize casually
- a shared architecture makes it easier to preserve existing scene wiring, progression, and phase ownership
- this keeps future maintenance saner than trying to support two separate systems that happen to touch the same authored scene

Implication:
- mode selection should happen as part of the existing intro/lesson entry flow
- shared systems should only split where behavior truly differs by mode
- future practice-mode work should prefer safe extensions and focused refactors over parallel controller stacks


## 2026-07-17 - Unity UI Events Should Be Inspector-Bound, While Option Population Stays In Code

Decision:
- stop relying on runtime listener wiring for the main lesson UI events
- keep dropdown content population in code, but bind UI event callbacks through the Inspector

Why:
- scene-bound UI behavior is easier to inspect, reason about, and correct in Unity
- this aligns better with the broader authored-scene direction of the project
- it also avoids quietly scattering UI behavior setup across code in ways that are harder to verify at a glance

Implication:
- buttons and dropdowns should expose clear public handlers for scene binding
- future UI refactors should preserve authored event wiring unless there is a strong reason to change it
- code should still own dynamic option generation where hand-authoring all entries would be wasteful


## 2026-07-17 - Register Values Should Persist During Ordinary Lesson Use, But Rebaseline On Mode Change

Decision:
- stop resetting the entire register bank back to fresh scripted values on every ordinary lesson start/reset
- preserve register values during ordinary interaction
- restore the authored baseline when the active lesson mode changes

Why:
- this matches the earlier preference for letting stateful systems feel more like real authored parts of the environment instead of disposable per-lesson props
- it also makes the register bank behave more coherently alongside the already-stateful data-memory behavior
- mode changes are the cleaner boundary for a deliberate authored reset than every single lesson restart

Implication:
- local register reset remains pose-only
- lesson flow should not casually wipe the whole register bank during ordinary retries
- future practice-mode work can rely on authored register defaults still existing as a safe reset boundary when changing modes


## 2026-07-17 - Idle Lesson Decode Scanners May Revert To Utility Preview Scanners

Decision:
- when no lesson is active, the two decode-stage lesson scanners may behave like ordinary preview scanners instead of remaining inactive

Why:
- there was already a third utility scanner in the scene proving that simple preview behavior is useful outside the structured lesson flow
- letting the lesson scanners fall back to preview use makes the register zone feel more useful between lesson runs
- this adds convenience without meaningfully changing the actual in-lesson decode validation rules

Implication:
- while idle, lesson decode scanners may preview register values and show simple success-style feedback
- once a lesson starts, they should return to their stricter lesson-owned validation behavior
- this fallback should be treated as a utility behavior, not as a second lesson-mode path


## 2026-07-18 - Practice Decode Must Extend The Existing Decode Panel Through Focused Helpers, Not Through A Second Controller Stack

Decision:
- keep one decode panel/controller entry point
- split scene refs, learning presentation, practice presentation, and practice validation into focused helper types instead of bloating the main controller

Why:
- the first practice-mode pass already showed how quickly decode can become too crowded if every new field and rule is handled directly in one place
- the existing decode scene wiring is still valuable and should not be replaced with a parallel practice-only panel controller stack
- a helper split keeps the mode extension real without turning the decode controller into another oversized file

Implication:
- future practice-mode decode growth should keep reusing:
  - dedicated scene-ref containers
  - focused practice field/view helpers
  - captured input-state validation
- additional practice instructions should extend the staged decode logic carefully rather than reopening the panel architecture each time


## 2026-07-18 - Later Practice Phases Should Share A Common Budgeted Failure Rhythm

Decision:
- extend practice mode through the existing later phase stations with a shared budget/failure pattern instead of inventing a different punishment/reset rule per phase

Why:
- the first decode slice already proved that practice mode benefits from:
  - limited answer attempts
  - limited hints
  - explicit held failure states
- repeating a different failure grammar in every later phase would make the mode feel inconsistent and harder to test

Implication:
- `EX`, `MEM`, `WB`, and `PC Update` should all use the same broad practice expectations:
  - validation attempts are limited
  - scanner attempts are limited
  - hint uses are limited
  - failure should hold until an explicit restart press


## 2026-07-18 - Dev Utilities Should Stay Centralized In The Settings Menu

Decision:
- put dev/testing relief in the settings menu instead of scattering separate skip/debug controls onto each phase UI

Why:
- testing the growing lesson repeatedly was already becoming a time sink
- a centralized dev path is easier to find, easier to disable mentally, and less likely to pollute the actual learner-facing panels
- per-phase debug buttons would add scene clutter and risk becoming accidental long-term UI baggage

Implication:
- if a dev-only phase skip exists, it should live behind a settings-menu dev gate
- future testing helpers should prefer the same centralized location unless there is a very strong reason not to


## 2026-07-18 - Practice Instructions Should Overlay Learning Instructions, Not Duplicate Them Whole

Decision:
- let practice instruction assets derive their runtime lesson definition by overriding a learning-mode base instruction instead of keeping a second full instruction-definition copy for every practice variant

Why:
- the project is already moving toward more practice instructions and may later add even more test-mode permutations
- fully duplicating every instruction lesson asset would make maintenance worse and increase drift risk
- the real differences for practice mode are mostly:
  - encoded presentation
  - decode expectations
  - register/runtime operand overrides
not an entirely different phase script flow

Implication:
- future practice instructions should stay lightweight where possible
- learning-mode instruction definitions remain the main authoritative lesson path
- practice-mode assets should override only what genuinely differs


## 2026-07-18 - Practice Decode Should Use Typed Bitfields Instead Of Binary Dropdowns

Decision:
- move Practice decode entry from dropdown-selected binary fields to TMP input fields backed by the spatial keyboard flow

Why:
- the point of Practice mode is to break guided-selection muscle memory and make instruction decoding more deliberate
- typed bitfields better fit the intended difficulty increase than selecting from tiny authored binary menus
- the same keyboard-backed path can later support broader typed interactions if they are worth keeping

Implication:
- Practice decode should validate submitted bitstrings after normalization rather than rely on dropdown option values
- future Practice / Test additions should reuse the same input normalization and field-wrapping helpers


## 2026-07-18 - Practice Mode Should Hide Decoded Assembly Until Decode Is Out Of The Way

Decision:
- when Practice mode is active, player-facing status readouts should keep showing the encoded instruction during `IF` and `ID`
- the readable assembly form should only reappear once decode is behind the learner

Why:
- the whole point of Practice mode is to make the learner decode the instruction rather than receive the answer for free from a convenience status label
- leaving the readable assembly exposed in the settings menu undermines the intended difficulty and turns the rest of decode into busywork

Implication:
- any future player-facing Practice readout should be checked for this same leak pattern before it is treated as safe
- settings-menu dev access can safely follow the same text-submission pattern when a typed gate is more appropriate than a toggle


## 2026-07-19 - Instruction And Info Bookkeeping Should Live In Authored Catalog Assets

Decision:
- keep instruction-selection bookkeeping and phase-info dropdown bookkeeping in authored `ScriptableObject` assets

Why:
- folder-wide runtime discovery stopped being the right shape once the project had multiple lesson modes, a larger instruction bank, and future `Test`-mode expectations
- authored assets make ordering, expansion, and inspection clearer than implicit path-based loading
- this keeps bookkeeping project-wide instead of tying it to one scene component

Implication:
- instruction selection should read from the authored lesson catalog asset
- phase-info dropdown population should read from the authored info catalog asset
- future expansion of either system should extend those assets rather than reintroduce path sweeps or scattered hardcoded lists


## 2026-07-19 - Decode Register Scanning Should Not Force rs Before rt

Decision:
- decode should still validate scanner-role correctness, but it should not penalize the learner just for scanning the correct required source registers in the opposite order

Why:
- the old sequential rule added friction without improving the teaching value of decode
- it was especially wasteful in Practice mode, where the user could lose an attempt even after decoding the right register pair
- the meaningful check is which register belongs on which scanner, not whether the learner happened to place `Read Register 1` before `Read Register 2`

Implication:
- required decode source-register scans can now complete in either order
- scanner-role validation still matters
- future decode text should describe remaining targets instead of implying a single mandatory next scanner unless a later lesson truly requires that behavior


## 2026-07-19 - Test Mode Should Be A Harsh Practice-Derived Assessment Mode

Decision:
- `Test` mode should reuse the Practice-style downstream mechanics, but strip away support and learner choice

Locked behavior:
- selecting `Test` on the intro panel should hide the instruction dropdown
- the intro body text should explain that the learner will receive a random instruction and that support is minimal
- pressing the intro action button should randomly choose one instruction from the Test pool and only then begin `IF`
- once `Test` begins, only the interaction panel should remain active
- the lesson panel should be hidden
- the hint panel should be hidden
- there should be no hints
- each phase should allow only:
  - `1` validation mistake
  - `1` scanner mistake
- any failure should end the full run immediately

Why:
- this keeps `Test` mode clearly distinct from `Practice`
- removing the lesson and hint panels avoids fake support surfaces that exist but do nothing
- the mode becomes a clean assessment path rather than a slightly stricter Practice clone

Instruction-pool note:
- it is acceptable for `Test` to reference or reuse Practice-definition assets where the mechanics are identical

Implication:
- `Test` should be a presentation/selection change plus stricter budget rules, not a third unrelated lesson system


## 2026-07-26 - The Current Android XR Build Should Be Treated As The Study Baseline

Decision:
- the current Android XR build is now the authoritative participant-study baseline
- further code changes should be treated as high-risk unless they fix a real blocker

Why:
- the project now covers the intended single-cycle lesson loop, the reduced-handholding practice path, and the strict assessment path
- the recent work shifted from feature building to build stability and presentation safety
- late-stage churn now poses more risk than value

Implication:
- remaining changes before real study use should stay narrow, deliberate, and easy to justify
- anything larger should move to future work after the presentation and participant sessions


## 2026-07-27 - Presentation And Thesis Framing Should Stay Anchored To The Built Learning Outcomes

Decision:
- presentation and thesis framing should be anchored to the implemented learning outcomes and the finished single-cycle study build, not to older broader ambitions

Why:
- the project has now reached participant-study use, so overclaiming is a bigger risk than underselling optional future work
- the strongest motivation comes from repeated teaching-assistant experience with datapath reasoning difficulties, not from VR novelty
- the final three-mode structure now supports a clean learning progression:
  - guided walkthrough
  - reduced-support practice
  - strict independent test

Implication:
- research framing should emphasize what the current build actually teaches:
  - decode
  - trace
  - explain
  - select
  - predict
  - compare
  - complete with reduced or no guidance
- future-work topics such as wider jump support, multicycle coverage, and pipelining should stay clearly separated from the baseline claims

