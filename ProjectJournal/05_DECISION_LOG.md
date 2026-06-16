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

## Open Questions

- which instruction should become the first fully playable lesson
- how much instruction decoding should be interactive in v1
- how explicit ALU/control-signal manipulation should be in the first prototype
- whether instruction selection is user-driven, random, or both
- how closely the prototype should model exact datapath semantics vs pedagogically simplified interactions
