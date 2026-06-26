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
- keep `InstructionSystemV1` as useful scaffolding

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

## 2026-06-26 - Register Bank Should Be Scene-Authored, Not Runtime-Spun Up

Decision:
- treat the user-created `Register Bank` object in `Testing Ground` as the permanent home for physical MIPS registers
- prefer editor-time authoring under that object over creating the bank from scratch when play mode starts

Why:
- the user explicitly dislikes important scene layout being silently created at runtime
- register spacing, readability, and reachability are visual/physical design decisions that need to be eyeballed in-scene
- the register bank is intended to become reusable later for ALU, memory, and placement-zone interactions

Implication:
- future assistants should extend the authored bank instead of replacing it with another runtime-generated register UI
- if the register bank is regenerated procedurally, it should be done in editor-time authoring or with the user's scene collaboration, not as invisible play-mode setup

## 2026-06-26 - Reuse XR Sample Interactables For Register Feel

Decision:
- build physical registers from XR Interaction Toolkit sample interactables rather than inventing grab/highlight behavior from scratch

Why:
- the user specifically pointed at the sample interactable feel they liked
- this preserves the stock highlight affordance and grab tuning from known-good XR samples
- it reduces risk compared with manually rebuilding every part of the interaction stack

Implication:
- register objects should keep behaving like grabbable sample props, with lesson scripts layered on top instead of replacing the XR basics

## 2026-06-26 - Register Look Should Stay Close To The Earlier Chunky Labeled Design

Decision:
- keep the register tokens visually close to the earlier blocky labeled look that the user liked
- only the interaction mode should change from button-like activation to grabbable object behavior

Why:
- the user explicitly approved the earlier visual direction and rejected the later tiny-cylinder pass
- the register bank needs readable labels and obvious affordance, not minimalist placeholder bodies

Implication:
- future register-bank changes should preserve readability, label presence, and visible affordance glow unless the user asks otherwise

## 2026-06-26 - Register Reset Should Be Local To The Bank

Decision:
- the register area should have its own reset control whose only job is returning lost pieces to home poses

Why:
- the user wants a quick recovery path if a register is dropped or thrown away from the bank
- resetting the whole lesson just to recover props would be annoying during iteration and playtesting

Implication:
- lesson reset and prop reset are separate concerns and should remain separate unless a future reason emerges to merge them

## 2026-06-26 - Execution And WriteBack Should Use Physical Register Pedestals

Decision:
- handle `EX` / `ALU` and `WB` interactions through authored pedestal zones rather than through abstract UI-only confirmation

Why:
- the user wants the learner to physically place the correct register onto the appropriate lesson target
- this keeps VR interaction meaningful instead of reducing later phases to button presses
- pedestal scanning also gives a clean path for instructions that reuse the same register in more than one logical role

Implication:
- pedestal objects should become part of the scene-authored interaction set
- each pedestal should validate only when its lesson step is active
- a pedestal should read the placed register token's identity, hold that scanned result, and then drive success/failure feedback
- future duplicate-register instructions should rely on scanned logical identity rather than requiring duplicate physical register props

## 2026-06-26 - Register Placement Validation May Use A Short Stable-Placement Rule

Decision:
- it is acceptable for a pedestal to require a short stability condition before accepting a register, such as:
  - the token being released
  - or the token remaining in-zone for roughly 1-2 seconds

Why:
- this reduces accidental brush-triggering while still feeling physical
- it gives the learner a clear "place and confirm" rhythm during later datapath phases

Implication:
- pedestal validation should be designed around placement confirmation, not instant overlap detection alone
- the exact acceptance rule can still be tuned once the first pedestal exists in-scene

## Open Questions

- which instruction should become the first fully playable lesson
- how much instruction decoding should be interactive in v1
- how explicit ALU/control-signal manipulation should be in the first prototype
- whether instruction selection is user-driven, random, or both
- how closely the prototype should model exact datapath semantics vs pedagogically simplified interactions
