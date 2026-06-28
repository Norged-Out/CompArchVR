# Decision Log

This file captures decisions that should survive across sessions, even when the detailed chat context is gone.

## 2026-06-16 - Scope Should Be Narrow, Not Grandiose

Decision:
- prioritize one clean learning objective before expanding

Why:
- the supervisor explicitly encouraged a smaller, defensible scope
- 5-6 weeks of development time is limited
- a polished narrow experience is stronger than a large unfinished one

Implication:
- success does not require a giant full-architecture simulator

## 2026-06-16 - The "Why VR?" Answer Must Be Educational, Not Cosmetic

Decision:
- justify VR through guided spatial reasoning, not novelty

Why:
- the project's motivation comes from teaching assistant experience
- students benefited from paced one-on-one whiteboard tracing
- VR should recreate that guided tracing process in an interactive embodied form

Implication:
- every interaction should help explain datapath reasoning more clearly than a flat diagram alone

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
- the user explicitly stated that minimal animation is acceptable
- this reduces polish pressure and protects scope

## 2026-06-16 - Recommended Instruction Implementation Order

Decision:
- implement in this order if possible:
  1. `add`
  2. `addi`
  3. `lw`

Why:
- it introduces complexity one concept at a time
- it gives the best chance of building reusable lesson structure

## 2026-06-16 - V1 Must Deliver add, addi, and lw by June 28

Decision:
- the prototype is not forever limited to only `add`, `addi`, and `lw`
- however, those three instructions are the mandatory V1 target for now
- they should be finished by `2026-06-28`
- the user intends to demo that V1 to the supervisor on `2026-06-29`

Why:
- this creates a concrete milestone instead of an open-ended prototype
- it preserves scope discipline while still allowing later expansion
- it aligns development with the next supervisor checkpoint

Implication:
- near-term planning should optimize for getting those three lessons stable
- anything beyond them is secondary until the V1 milestone is safe

## 2026-06-16 - The Existing Instruction Scripts Should Be Refined, Not Scrapped

Decision:
- keep `InstructionSystem` as useful scaffolding

Why:
- the current plan still benefits from instruction definitions, runtime selection, and UI layout data
- the main needed change is moving toward stage-driven validation, not abandoning the architecture entirely

Implication:
- future work should evolve the scripts rather than restart from zero unless a very strong reason emerges

## 2026-06-16 - Repo Journal Must Be Maintained After Meaningful Work

Decision:
- after meaningful development sessions, relevant journal files should be updated

Why:
- the user explicitly requested persistent project memory inside the repository
- this helps with context loss, credit limits, and future assistant continuity

Implication:
- updating the journal is part of the workflow, not optional housekeeping

## 2026-06-16 - Scene Layout Should Be Co-Authored, Not Silently Built In Code

Decision:
- when placement and visual feel matter, prefer scene-authored layout with the user present
- runtime-generated UI and interaction objects are acceptable only as fallback scaffolding

Why:
- the user explicitly wants to eyeball placement and design choices in Unity
- code-generated layout can be technically functional while still feeling wrong in-scene

Implication:
- future assistants should ask for scene objects or layout collaboration instead of defaulting to building everything procedurally

## 2026-06-16 - Lesson Runtime Should Stay Split Across Focused Scripts

Decision:
- avoid bloating one controller with scene setup, validation, UI construction, and runtime interaction creation

Why:
- the user explicitly called out the old lesson controller as too bloated and hard to trust
- smaller scripts make the prototype easier to reason about and safer to extend into `addi` and `lw`

Implication:
- future lesson work should preserve separation of concerns unless there is a strong reason to merge responsibilities

## 2026-06-26 - Register Bank Should Be Scene-Authored

Decision:
- treat the user-created register area in `Testing Ground` as the permanent home for physical MIPS registers

Why:
- the user explicitly dislikes important scene layout being silently created at runtime
- register spacing, readability, and reachability are visual/physical design decisions that need to be eyeballed in-scene

Implication:
- future assistants should extend the authored bank instead of replacing it with runtime-generated register UI

## 2026-06-26 - Reuse XR Sample Feel For Register Interaction

Decision:
- build physical registers on top of XR Interaction Toolkit sample interaction feel rather than inventing grab/highlight behavior from scratch

Why:
- the user specifically pointed at the sample interactable feel they liked
- this reduces risk compared with rebuilding every part of the interaction stack manually

Implication:
- register objects should keep behaving like grabbable sample props, with lesson scripts layered on top

## 2026-06-26 - Register Look Should Stay Close To The Chunky Labeled Design

Decision:
- keep the register tokens visually close to the earlier blocky labeled look that the user liked

Why:
- the user explicitly approved that visual direction and rejected the tiny-cylinder pass
- the register bank needs readable labels and obvious affordance, not minimalist placeholder bodies

Implication:
- future register-bank changes should preserve readability, label presence, and visible affordance glow unless the user asks otherwise

## 2026-06-26 - Register Reset Should Be Local To The Bank

Decision:
- the register area should have its own reset control whose only job is returning lost pieces to home poses

Why:
- the user wants a quick recovery path if a register is dropped away from the bank
- resetting the whole lesson just to recover props would be annoying during iteration and playtesting

Implication:
- lesson reset and prop reset are separate concerns and should remain separate unless a future reason emerges to merge them

## 2026-06-26 - Execution And WriteBack Should Use Physical Register Pedestals

Decision:
- handle `EX` / `ALU` and `WB` interactions through authored pedestal zones rather than through abstract UI-only confirmation

Why:
- the user wants the learner to physically place the correct register onto the appropriate lesson target
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
- it gives the learner a clear "place and confirm" rhythm during later datapath phases

Implication:
- pedestal validation should be designed around placement confirmation, not instant overlap detection alone

## 2026-06-27 - Lesson UI Should Be Authored As Zone-Specific World-Space Panels

Decision:
- prefer separate authored lesson panels placed near the relevant interaction zones instead of one giant fixed UI

Why:
- the user wants the learner to physically move through the lesson space
- this keeps prompts close to the action and reduces unnecessary head-turning

Implication:
- `Intro UI` should host lesson start and instruction/decode framing
- the register area should get its own panel for `rs` / `rt` / destination guidance
- later zones can follow the same pattern for `ALU`, memory, and write-back

## 2026-06-27 - Scene Must Be Rescanned After Manual Cleanup

Decision:
- if the user deletes or restructures scene objects, treat the current `Testing Ground` hierarchy as the source of truth and rescan it before extending the system

Why:
- several earlier mistakes came from relying on stale assumptions about the scene
- the user is actively co-authoring `Testing Ground` inside Unity

Implication:
- future work should inspect the live scene state first
- dead code tied to removed scene structures should be pruned rather than preserved automatically

## 2026-06-28 - Lesson UI Must Be Authored In Edit Mode

Decision:
- lesson UI should exist in the scene ahead of play
- code should drive existing scene UI rather than spawning lesson panels or text objects at runtime

Why:
- the user explicitly prefers to author and eyeball the world-space panels in Unity
- runtime-built UI caused layout, overlap, and readability problems during the MVP pass
- the current verified MVP works better when the scene owns the panel layout

Implication:
- future lesson panels should be created in edit mode under `Testing Ground`
- lesson code should mostly:
  - toggle authored panels
  - update authored text
  - respond to authored buttons
- future assistants should not default back to runtime-created lesson UI

## 2026-06-28 - Core Lesson Objects Should Be Inspector-Wired, Not Found At Runtime

Decision:
- core lesson scene objects should be assigned through serialized Inspector references whenever practical

Why:
- the user explicitly wants scene objects bound in edit mode
- runtime scene searches made the system less trustworthy and less aligned with the authored-scene workflow
- the current lesson flow is easier to reason about when the scene wiring is visible in the Inspector

Implication:
- `LessonGuideController`, `ControlDecodeController`, and `CpuLessonFlow` should prefer serialized refs for authored panels, buttons, and banks
- future assistants should not default to `Find*` or name-based hookup for the main lesson path

## 2026-06-28 - Control Decode Is A Real Gated Phase Between Intro And Registers

Decision:
- the active lesson order is now:
  1. `Intro UI`
  2. `Control Decode UI`
  3. `Register Setup UI`

Why:
- the user explicitly defined this order in the scene
- the decode step is part of the intended instruction-decode phase, not a side experiment
- treating the flow explicitly keeps future work grounded and prevents circular redesign

Implication:
- future lesson extensions should preserve this authored handoff unless the user changes it
- later zones such as `ALU`, memory, and write-back should grow after this baseline rather than replace it

## 2026-06-28 - Current Working MVP Is Intro-To-Register

Decision:
- treat the current working baseline as a minimal intro-to-register `add` lesson

What currently works:
- the lesson starts from `Intro UI`
- the learner is introduced to the instruction there
- the flow then hands off to `Register Setup UI`
- the learner places registers through the authored scanners in the register zone

Why:
- the user has tested the current scene and confirmed that it works so far
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
- the user has now verified that this scene-authored + code-rebuild approach works

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

## 2026-06-28 - ALU_V1 Is The Next Focus Branch

Decision:
- the next dedicated implementation branch should focus on the `ALU` / execution step for the current lesson flow

Why:
- `Intro UI`, `Control Decode UI`, and `Register Setup UI` now form a workable MVP baseline
- the biggest missing instructional piece in the current `add` walkthrough is the execution step itself

Implication:
- the next extension should add an authored `ALU` zone before trying to build out memory behavior
- `Data Memory` should remain unused for `add` while the ALU interaction is being stabilized

## 2026-06-29 - Control Decode Stops Before ALU-Specific Signals

Decision:
- `Control Decode UI` should validate only the 6 non-ALU signals for now
- `ALUOp` and `ALUSrc` belong to the execution phase

Why:
- the user explicitly moved those responsibilities onto the ALU itself
- this keeps decode focused and lets execution teach ALU-specific configuration in the right place

Implication:
- `ControlDecodeController` should not gate on `ALUOp` / `ALUSrc`
- `AluExecutionController` and the authored `ALU UI` now own those checks

## 2026-06-29 - Local Register Reset Must Be Pose-Only

Decision:
- the register-bank reset button should only restore register piece positions

Why:
- the user explicitly wants it to recover moved props without disturbing lesson state
- clearing packets or successful scans made the interaction feel wrong mid-lesson

Implication:
- local register reset should not:
  - destroy data packets
  - clear successful scanner colors
  - deactivate scanners
  - wipe logical values

## 2026-06-28 - Register Values Should Be Separate From Physical Reset

Decision:
- register tokens may carry logical values, but the local bank reset button should only reset pose / scanner / visual state

Why:
- the user explicitly wants lesson-time register values that can differ from the default zero state
- wiping those values whenever a lost token is returned home would make iteration and teaching flow worse

Implication:
- logical register value reset should stay under lesson/runtime control
- physical prop reset and lesson-value reset remain separate concerns

## 2026-06-28 - Scanner Value Display Is Preferred Over Always-On Register Value Display

Decision:
- prefer showing the active value at the scanner / datapath stage rather than forcing every register token to permanently foreground its value

Why:
- the register pieces are easier to read when they primarily communicate identity
- the value becomes pedagogically meaningful at the moment the datapath reads it
- this fits the user's ALU plan better, where scanned values turn into carried data packets

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

## Open Questions

- how much instruction decoding should be interactive in V1
- how explicit ALU/control-signal manipulation should be in the first prototype
- whether instruction selection is user-driven, random, or both
- how closely the prototype should model exact datapath semantics vs pedagogically simplified interactions
