# Master Execution Plan

This file should be read as:
- current strategy first
- preserved historical reasoning second

If an older milestone note conflicts with the present state of the project, the present-tense sections nearer the top of this file and the top of `01_PROGRESS_LOG.md` win.

## Overarching Project Direction

This project is not trying to build a giant all-encompassing computer architecture simulator.

It is trying to build a focused educational VR experience that serves as a supplementary tool for helping learners reason through CPU datapath behavior more clearly than a first pass with static materials often allows.

The strongest justification for VR is:
- students often struggle to mentally trace the datapath from flat diagrams
- one-on-one whiteboard guidance works better because it is paced, interactive, and spatial
- this project aims to recreate that guided tracing experience in an immersive, self-paced form

Personal motivation behind that framing:
- during my Bachelor's at the University of Arizona, I worked as a teaching assistant for Computer Organisation for two years across four consecutive semesters
- during office hours, I repeatedly saw the same pain points:
  - reading datapath diagrams
  - understanding what control signals do for different instructions
  - following data movement through registers, memory, and PC updates
  - reasoning about adjacent topics such as stacks and function calls
- the most effective help was usually not another static explanation, but drawing over the datapath on a whiteboard or iPad and walking a student through it step by step
- this project comes directly from the idea that VR might recreate part of that guided tracing process by letting the learner stand inside the datapath and act through the instruction flow themselves

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

I agreed with my supervisor that the project should:
- achieve one learning objective cleanly first
- only expand if the first objective is already working

This principle should override ambitious expansion pressure.

## Current Delivery Milestone

Active schedule:
- supervisor meeting target on `2026-07-24` is complete
- the project and presentation baseline are complete
- the participant-study period has now been carried through its active session window
- the dissertation, analysis pipeline, and public support materials now exist in submitted final form
- the current build should now be treated as the finished archived project baseline

Current risk:
- not missing datapath coverage
- but introducing late inconsistencies across the archived dissertation, appendices, references, and release/support materials
- plus any last device-only regression that does not appear during editor testing
- and, on the repo side, obscuring the final project state with rushed cleanup or poorly scoped late changes

Historical note:
- the June 29, 2026 V1 checkpoint is complete and should now be treated as project history, not as the live plan

## Design Philosophy

Only make an interaction physical if the physical interaction teaches the concept better than a conventional UI.

Use VR for:
- spatial tracing
- path selection
- register manipulation
- mux/gateway decisions
- embodied progression through datapath stages
- authored route-following cues when the map itself becomes part of the lesson readability

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

## Implemented Instruction Baseline

Working lesson paths now include:
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
- `j`

Historical rollout:
1. `add`
2. `addi`
3. `lw`

That rollout remains useful for explaining project growth, but it is no longer the current delivery plan.

## Historical MVP Logic

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
- this section explains the original shaping logic of the prototype
- the old `add` / `addi` / `lw` V1 target has already been achieved and should now be read as historical context

## Preferred Feature Breakdown

Must have:
- a stable guided lesson loop across the supported instruction set
- instruction selection or controlled instruction presentation
- operand identification, stage progression, correctness gating, and readable feedback
- stable scene layout and presentation-ready authored UI

Strongly desired:
- UI reference support for format/opcode/control help
- mux/gateway interactions
- register-selection mechanics

Nice to have:
- richer audiovisual polish
- jump-family expansion
- optional support systems beyond the current settings baseline

The project has already moved beyond the old three-instruction V1 boundary, so future prioritization should now favor clarity and presentation over raw instruction count.

Current extension note:
- practice mode has already been extended safely from the existing lesson architecture
- test mode now also exists as the harsher assessment layer derived from the same baseline
- the main question is no longer whether the three-mode structure can be built, but how little should be changed now that the study build and dissertation are effectively complete

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

Current authored-panel shape:
- `Intro UI` for mode/instruction selection and fetch framing
- `Register Zone UI` for decode and operand setup
- zone-specific panels for `ALU`, memory, write-back, and PC update
- outside Intro, the preferred format is lesson panel + interaction panel + hint/info panel

Runtime preference:
- keep static instructional text authored in-scene where possible
- keep runtime updates focused on instruction-specific text, live status, feedback, and validation state

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

Current near-term note:
- `RegDst` vs `ALUSrc` lesson placement is still an open pedagogical question
- for the final archived baseline, keep the existing interaction split stable unless a clearer teaching need emerges

## Architecture Direction

The current `InstructionSystem` scripts are useful scaffolding and should be evolved rather than discarded.

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
- the `2026-07-24` supervisor meeting is now complete
- the project/demo target is complete and now belongs to project history

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

## Remaining Near-Term Posture

At this point:
- the project is effectively complete for the presentation, research sessions, and dissertation submission
- the current build should be preserved, not actively reshaped
- any further work should be treated as optional continuation rather than required project delivery

If anything still changes, it should be small and low-risk:
1. deployment sanity / device-only fixes
2. questionnaire / study-material alignment
3. presentation / demonstration support
4. anything larger should move to future work instead

## Future Work Beyond The Final Demo

The clearest post-demo extension path is:
- multicycle datapath extension
- then real pipelining concepts
- then stalls / hazards as a higher-difficulty teaching mode

Current concept:
- place additional instruction download terminals at later phases
- physically move instruction modules between phases
- allow multiple instructions to exist in-flight
- use that structure to discuss overlap, sequencing, and pipeline disruption

This is intentionally future work:
- valuable for the dissertation and presentation discussion
- probably too large to force into the current pre-demo build without destabilizing it

## Paper Preparation Implication

The implementation should collect enough clarity for the later paper to discuss:
- educational motivation
- why VR was chosen
- what learning objective was targeted
- how the interaction design supported that objective
- what was implemented
- what limitations remained

Current dissertation-structure note:
- the active paper order is now:
  1. Introduction
  2. Related Work
  3. Methodology
  4. Design
  5. Implementation
  6. Results
  7. Discussion
  8. Conclusion and Future Work
- this means the paper should establish the research framing, learning outcomes, bounded hypotheses, study design, and analysis plan before turning to the design and implementation chapters
- the design chapter should therefore read as a response to the methodological and educational framing, and the implementation chapter should read as the concrete realization of that design
- the research question and hypotheses should be revised where needed so they describe the project honestly as a supplementary educational tool rather than as a broad classroom-versus-traditional-methods comparison
- the results chapter should commit the findings cleanly without interpretation, and the discussion chapter should handle interpretation, contribution, and comparison with the literature
- the implementation chapter should emphasize what was built and how the traditional datapath was translated into VR, with figures and component-to-datapath connections doing more work than internal class inventories
- the final report now also includes the appendix set, front matter, and results assets needed to support submission and later inspection cleanly

Current note:
- the project is now at the stage where presentation and paper framing should describe the implemented build faithfully instead of arguing for speculative expansion
- the cleanest framing is now:
  - TA-grounded educational motivation
  - guided spatial reasoning as the "why VR?" answer
  - methodology and learning outcomes made explicit before artifact chapters
  - research question and hypotheses aligned to the real supplementary-tool study scope
  - the finalized learning outcomes supported by the built three-mode lesson flow
  - future work kept clearly separate from the finished participant-study baseline
  - repetition cut down across the draft so major points are stated once in their strongest location and only referred back to when needed
  - reproducible quantitative processing and clear appendix support for the study materials
  - a final repo/release pass that exposes the build and report cleanly without reopening the project scope

The more disciplined the scope, the easier the paper will be to write.

Thesis-side authority note:
- the original research-plan PDF remains useful as a historical record of the approved project direction
- for the current August 2026 dissertation rewrite, the live paper direction
  should be treated as:
  - a supplementary-tool framing rather than a broad
    classroom-versus-traditional-methods claim
  - revised research-question and hypothesis wording that matches the actual
    study scope
  - a clear split between `Results`, `Discussion`, and `Conclusion and Future Work`
- if older plan wording overclaims, revise the thesis framing toward the
  supplementary-tool direction rather than trying to defend the older wording

## Final Archived Posture

Current posture:
- keep the dissertation, appendices, and references internally consistent as the submitted record
- preserve the reproducible study-data/SPSS baseline already prepared under `SPSS`
- keep the public-facing repository notes and build links aligned with the finished project state
- treat any future expansion as personal-interest continuation rather than as part of the formal dissertation obligation
