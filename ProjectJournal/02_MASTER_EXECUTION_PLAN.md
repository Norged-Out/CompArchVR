# Master Execution Plan

This file should be read as:
- current strategy first
- preserved historical reasoning second

If an older milestone note conflicts with the present state of the project, the present-tense sections nearer the top of this file and the top of `01_PROGRESS_LOG.md` win.

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

I agreed with my supervisor that the project should:
- achieve one learning objective cleanly first
- only expand if the first objective is already working

This principle should override ambitious expansion pressure.

## Current Delivery Milestone

Active schedule:
- next supervisor meeting target: `2026-07-24`
- official project/demo deadline: `2026-07-29`
- preferred internal target:
  - finish a stable build a few days early
  - keep the final buffer for polish, participant prep, and presentation safety

Current risk:
- not missing datapath coverage
- but safely extending the build into practice mode without breaking the guided baseline
- plus onboarding clarity, experiment-mode readiness, and overall delivery quality

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

### Must Have

- a stable guided lesson loop across the current supported instruction set
- instruction selection or controlled instruction presentation
- operand identification
- stage-by-stage progression
- correctness gating
- feedback for wrong choices
- stable scene layout for key datapath zones
- presentation-ready readability across authored UI and environment

### Strongly Desired

- UI reference boards for format/opcode/control help
- mux/gateway interactions
- register selection mechanics

### Nice To Have

- richer audiovisual polish
- jump-family instruction support
- experiment mode without guided handholding
- expanded cheatsheet/settings support beyond the current baseline menu
- optional helper NPC / guide presence
- optional in-world music / ambience interaction if it improves presentation feel without becoming distracting

The project has already moved beyond the old three-instruction V1 boundary, so future prioritization should now favor clarity and presentation over raw instruction count.

Current extension note:
- practice mode is now the main near-term systems extension, and it should be built by extending the existing lesson architecture rather than forking an unrelated second flow

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
   - show the fetch framing
2. `Register Zone UI`
   - show the instruction field breakdown for decode
   - remind the learner which fields map to the currently needed source operands
   - keep prompts physically near the register bank and scanners
3. later zone-specific panels
   - `ALU`
   - `Data Memory`
   - `WriteBack`

Current refinement direction:
- each lesson zone UI should move toward three authored panels side by side:
  - lesson panel
  - interaction panel
  - hint / info panel
- static instructional text should live in edit mode where possible
- runtime should primarily update:
  - current instruction-specific text
  - live statuses
  - feedback
  - validation state
- instruction fetch now also has a physical embodiment path through:
  - `Instruction Module`
  - uploader terminal
  - downloader terminal
- control-flow conclusion now also has a dedicated end-stage path through:
  - `PC Update UI`
  - `PC Update Station`
  - branch-resolution logic for `beq` / `bne`

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
- for the current polish phase, keep the existing interaction split stable unless a clearer teaching need emerges

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
- the next anchor is the `2026-07-24` supervisor meeting
- the official project/demo target remains `2026-07-29`

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

## Current Pre-Meeting Cutoff

Before the next supervisor meeting on `2026-07-24`, prioritize:

1. practice mode foundation and first safe playable slice
2. tutorial / onboarding refinement
3. experiment mode without handholding
4. optional settings refinement only where it clearly improves usability
5. jump-family instruction additions only if the above is already safe
6. final presentation polish

Already partly established and now moving into refinement instead of first implementation:
- routed map revamp / improved route baseline
- gated progression
- physical route guidance
- multi-panel authored lesson UI
- baseline wrist settings menu support
- baseline tutorial/onboarding setup through the imported sample tutorial/video prefab and lightweight in-scene image surfaces
- broad gameplay audio feedback across the main lesson loop

Reason:
- the functional lesson loop is already broad enough
- the main remaining risk is whether the experience feels readable, guided, polished, and complete

Practical interpretation of this cutoff:
- do not let practice-mode work silently regress the stable guided lesson loop
- do not reopen core datapath architecture unless a blocker appears
- do not treat instruction-count growth as the main success metric anymore
- treat the next supervisor meeting as a usability/readability checkpoint, not a "prove the datapath exists" checkpoint

## Recommended Backlog After The Meeting

Recommended order once the `2026-07-24` checkpoint build is safe:

1. expanded settings menu / replay affordances if still useful
2. cleaner exit / restart / free-roam transitions
3. deeper audio balancing / ambience if still useful
4. immediate-family expansion:
   - `slti`
   - `andi`
   - `ori`
5. jump instructions:
   - `j`
   - `jal`
   - `jr`
6. `lui`
7. deeper VFX polish

Rationale:
- the learner experience should be presentation-ready first
- post-checkpoint scope can expand only if the final guided build is already stable
- instruction additions are now secondary to clarity and delivery

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

The more disciplined the scope, the easier the paper will be to write.
