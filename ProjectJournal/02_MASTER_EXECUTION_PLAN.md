# Master Execution Plan

## Overarching Project Direction

This project is not trying to build a giant all-encompassing computer architecture simulator.

It is trying to build a focused educational VR experience that helps learners reason through CPU datapath behavior more effectively than a static schematic alone.

The strongest justification for VR is:
- students often struggle to mentally trace the datapath from flat diagrams
- one-on-one whiteboard guidance works better because it is paced, interactive, and spatial
- this project aims to recreate that guided tracing experience in an immersive, self-paced form

## Primary Learning Objective

After completing the experience, a learner should be able to:

- identify the relevant operands of a selected MIPS instruction
- trace the instruction through the required major datapath components of a single-cycle CPU
- explain why each major component is used or skipped

## Secondary Learning Objective

If time allows, the learner should also be able to:

- compare how different instruction types activate different parts of the datapath

Examples:
- `add` does not access data memory
- `addi` uses the immediate path
- `lw` uses sign extension, address calculation, memory read, and memory-to-register write-back

## Scope Principle

The user and supervisor agreed that the project should:
- achieve one learning objective cleanly first
- only expand if the first objective is already working

This principle should override ambitious expansion pressure.

## Current Concrete Milestone

Near-term deadline:
- finish `add`, `addi`, and `lw` by `2026-06-28`
- present that build to the supervisor on `2026-06-29` as a V1 checkpoint

Important clarification:
- this does not permanently limit the whole project to only those three instructions
- it defines the minimum instruction set that must be working for the first serious prototype review

## Design Philosophy

Only make an interaction physical if the physical interaction teaches the concept better than a conventional UI.

Use VR for:
- spatial tracing
- path selection
- register manipulation
- mux/gateway decisions
- embodied progression through datapath stages

Use UI for:
- lookup tables
- instruction format reminders
- opcode/function/control references
- short prompts
- explanation text
- correctness feedback

Animation is not mandatory.
Acceptable feedback mechanisms include:
- highlights
- sound cues
- prompts
- gated progression
- visual state changes

## Recommended Instruction Rollout

Preferred implementation order:

1. `add`
   Teaches pure register-to-register datapath flow.

2. `addi`
   Adds immediate handling and ALUSrc reasoning.

3. `lw`
   Adds memory access, address calculation, and write-back from memory.

This sequence is strong because each instruction introduces one major new concept at a time.

For the current V1 milestone, this sequence is also the committed delivery target, not just a suggestion.

## Recommended MVP

The minimum viable playable lesson should include:
- one instruction
- a guided step sequence
- meaningful learner decisions
- correctness gating
- explanation prompts tied to each major stage
- scene-authored world-space panels at the places the learner actually visits

The first instruction does not need to be the most complex one.
It needs to be the one most likely to produce a clean, working lesson loop.

Note:
- the original MVP logic still applies architecturally
- however, the current supervisor-facing V1 target is more ambitious: a stable `add` / `addi` / `lw` prototype by `2026-06-28`

## Preferred Feature Breakdown

### Must Have

- three working instruction walkthroughs for the current V1 target:
  - `add`
  - `addi`
  - `lw`
- instruction selection or controlled instruction presentation
- operand identification
- stage-by-stage progression
- correctness gating
- feedback for wrong choices
- stable scene layout for key datapath zones

### Strongly Desired

- UI reference boards for format/opcode/control help
- mux/gateway interactions
- register selection mechanics

### Nice To Have

- random instruction generation
- advanced control-signal detail
- richer audiovisual polish
- extra instruction families like branches or jumps

Anything beyond `add`, `addi`, and `lw` should be treated as post-V1 expansion unless the core loop is already stable.

## Non-Goals

The following are explicitly not required for success:

- a full MIPS implementation
- multi-cycle datapath coverage
- pipeline simulation
- exhaustive bit-level reconstruction of every instruction
- heavy animation or cinematic polish
- support for every control signal as a standalone physical minigame

## Proposed Experience Shape

The ideal lesson loop is:

1. present or select an instruction
2. identify the relevant fields/operands
3. move through the major datapath stages
4. make meaningful choices at key decision points
5. block incorrect progression locally
6. explain what the current stage is doing
7. finish with a recap of the instruction flow

Current preferred authored-panel shape:

1. `Intro UI`
   - start lesson
   - show current instruction
   - show the fetch/decode framing
2. `Register Zone UI`
   - remind the learner which fields map to `rs`, `rt`, and destination
   - keep prompts physically near the register bank and scanners
3. later zone-specific panels
   - `ALU`
   - `Data Memory`
   - `WriteBack`

## Interaction Heuristic

Make the learner do the meaningful reasoning.
Do not make the learner do all the mechanical overhead.

Good candidate interactions:
- selecting the correct registers
- choosing the correct mux path
- choosing the correct destination/write-back route
- deciding whether memory is used
- deciding whether an immediate is used

Poor candidate interactions for a first version:
- manually building every instruction bit pattern
- manually computing sign extension bit-by-bit
- manually replaying every wire-level movement
- overloading one lesson with too many simultaneous control decisions

## Architecture Direction

The current `InstructionSystemV1` scripts are useful scaffolding and should be evolved rather than discarded.

Likely long-term shape:
- `InstructionDefinition`
  High-level asset describing one instruction.
- `InstructionStageDefinition`
  One stage of a lesson.
- `InstructionInteractionRequirement`
  What the learner must do correctly in a stage.
- `InstructionRuntimeSelection`
  What the learner currently selected.
- `InstructionLessonController`
  Validates and advances the lesson flow.

This structure aligns better with the emerging design than a simple "advance highlight" controller alone.

## 5-6 Week Development Strategy

### Phase 1 - Lesson Framework

- lock the first instruction choice
- stabilize the datapath zone layout
- define lesson stages and validation requirements
- convert the current prototype from raw highlighting to gated stage progression

### Phase 2 - First Fully Playable Instruction

- implement one instruction end-to-end
- include prompts, correctness checks, and recap
- keep interactions minimal but meaningful

### Phase 3 - Expand To A Second Instruction

- add one instruction that introduces one new concept
- reuse the same lesson framework
- compare path differences

### Phase 4 - Expand To A Third Instruction If Time Allows

- likely `lw` if not already used
- emphasize memory path differences clearly

### Phase 5 - Polish For Demo

- improve clarity, pacing, and feedback
- simplify confusing interactions
- remove unnecessary friction
- make the demo stable and understandable to a first-time viewer

Current deadline anchor:
- this phase should culminate in the `2026-06-29` supervisor demo

## Fallback Plan

If development slips, the project should still be considered successful if it delivers:

- one polished instruction lesson
- one clearly articulated learning objective
- a strong explanation of why VR helped the lesson design

That is preferable to three half-working instruction lessons.

## Demo Success Criteria

A strong demo should show that a learner can:
- recognize the selected instruction format
- select the correct key operands
- move through the correct datapath route
- explain why memory, immediate, or write-back paths were or were not used

## Paper Preparation Implication

The implementation should collect enough clarity for the later paper to discuss:
- educational motivation
- why VR was chosen
- what learning objective was targeted
- how the interaction design supported that objective
- what was implemented
- what limitations remained

The more disciplined the scope, the easier the paper will be to write.
