# Progress Log

## Current Status

Project phase:
- Early prototype + planning consolidation

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype focus:
- placeholder CPU datapath nodes
- XR interaction foundations
- initial instruction-system architecture drafts
- narrowed dissertation scope after supervisor discussion

Current milestone:
- V1 supervisor demo target on `2026-06-29`
- `add`, `addi`, and `lw` should be finished by `2026-06-28` at all costs
- this is the near-term prototype target, not the permanent upper bound of the whole project

## Latest Summary

The project now has:
- a Unity project committed and pushed
- a `Testing Ground` scene being used as the active prototype sandbox
- five placeholder CPU nodes in the scene:
  - `PC`
  - `Instruction Memory`
  - `Registers`
  - `ALU`
  - `Write Back`
- a physical XR push button that advances the node highlight sequence
- a first draft node-sequencing script in `Assets/MyScripts`
- an initial `InstructionSystemV1` script scaffold for future instruction definitions and lesson flow
- a clarified project direction focused on a narrow, defensible learning objective rather than a grand all-of-architecture simulator

## Chronological Entries

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

## Current Working Baseline

### Scene / Interaction Baseline

- `Testing Ground` is the sandbox scene
- the prototype currently supports a simple step-through node highlight loop
- the physical XR button interaction is working
- minimal visual feedback is acceptable; heavy animation is not required

### Architecture / Script Baseline

Current relevant scripts:
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuNodeSequenceController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionRuntimeSelection.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionEnums.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionUiLayout.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionFlowControllerDraft.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionSelectionUiControllerDraft.cs`

Interpretation:
- these are useful scaffolding
- they should not be scrapped outright
- they will likely be refined into a more stage-driven interaction/validation system

## Immediate Next Steps

Recommended next development priorities:

1. Treat `add`, `addi`, and `lw` as the non-negotiable V1 set for the `2026-06-28` milestone.
   Recommended implementation order:
   - `add`
   - `addi`
   - `lw`

2. Convert the current "highlight sequence" prototype into a gated lesson flow.
   This means:
   - stage progression
   - correctness checks
   - feedback for wrong choices
   - explicit lesson prompts

3. Build the first lesson loop around meaningful interactions only.
   Keep physical:
   - register selection
   - mux/path choices
   - destination/write-back decisions
   Keep UI-based:
   - opcode/funct/control lookup
   - format reminders
   - text prompts
   - short explanations

4. Extend the script architecture with stage/requirement data.
   Likely future additions:
   - instruction stage definitions
   - interaction requirement definitions
   - lesson validation logic

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
