# Progress Log

## Current Status

Project phase:
- First playable `add` vertical slice baseline

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype focus:
- gated `add` lesson flow
- XR button-driven register selection
- reusable datapath lesson scaffolding for `addi` and `lw`
- narrowed dissertation scope after supervisor discussion

Current milestone:
- V1 supervisor demo target on `2026-06-29`
- `add`, `addi`, and `lw` should be finished by `2026-06-28` at all costs
- this is the near-term prototype target, not the permanent upper bound of the whole project

## Latest Summary

The project now has:
- a Unity project committed and pushed
- a `Testing Ground` scene being used as the active prototype sandbox
- a scene-linked lesson flow + node map on `CPU Placeholder Nodes`
- a first playable `add` walkthrough that auto-loads on play
- explicit datapath highlighting by node id rather than incidental sequence order
- a runtime-created `Data Memory` placeholder so the scene is ready for `lw`
- a world-space lesson UI that shows instruction, stage, explanation, and feedback
- a curated physical register bank for operand selection
- a cleaner lesson architecture split across focused scripts instead of one large controller
- draft `addi` and `lw` instruction assets for later extension

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

### 2026-06-16 - Add Vertical Slice Implemented

Completed:
- evolved `CpuNodeSequenceController` from pure sequence stepping into a reusable node highlighter that can also route the existing advance button through lesson progression
- added `DatapathNodeRegistry` to map logical datapath node ids to the actual placeholder renderers in `Testing Ground`
- converted `InstructionFlowControllerDraft` into the active V1 lesson runtime
- expanded `InstructionFlowStep` with gating metadata:
  - required interaction type
  - required register selections
  - optional write-back confirmation role
- expanded `InstructionDefinition` with:
  - assembly text
  - field breakdown text
  - expected operand registers
  - curated register-bank choices
- added `LessonInteractableButton` for runtime-built XR register buttons and the reset button
- added `AddInstructionDefinition`, plus draft `AddiInstructionDefinition` and `LwInstructionDefinition`, under `Assets/MyData/Resources/InstructionDefinitions`
- attached the lesson controller and datapath registry components to `CPU Placeholder Nodes` in `Testing Ground`
- implemented a runtime-created `Data Memory` placeholder so the scene now supports the full single-cycle node set
- implemented a runtime world-space lesson UI near the test zone
- implemented a curated physical register bank for the first `add` lesson
- implemented the first playable `add` walkthrough stages:
  - `ProgramCounter`
  - `InstructionMemory`
  - `Registers`
  - `ALU`
  - `WriteBack`
  - recap/completion

Changed:
- the original advance button now behaves as `Advance Step` during lesson play instead of blindly stepping the legacy sequence
- register selection is now physically gated and wrong choices show local feedback
- write-back requires a final physical confirmation of `rd`
- `DataMemory` is available in the scene model but intentionally skipped by the `add` lesson

Next:
- test the in-headset readability and physical spacing of the runtime-created UI and register bank
- validate that the register buttons feel reliable with the chosen XR interaction path
- polish the `add` walkthrough wording and pacing if any step feels awkward
- start extending the same lesson framework into `addi`

Risks / Notes:
- the new UI, register bank, reset button, and `Data Memory` placeholder are created at runtime on play rather than authored as static scene objects
- this was the safest path from the current repo state without opening the Unity editor from here
- the unrelated local file `ThePrototype/Assets/XR/AndroidXR/AndroidXRSettingsInitializer` was left untouched

### 2026-06-16 - Lesson Runtime Refactored

Completed:
- refactored the oversized lesson controller into a smaller orchestration script:
  - `CpuLessonFlow`
- split the lesson runtime into focused helper scripts:
  - `NodeMap`
  - `LessonSetup`
  - `LessonUI`
  - `RegisterBank`
  - `RegisterButton`
  - `LessonChecks`
  - `ButtonFactory`
- moved the fallback `add` definition builder into:
  - `InstructionDefaults`
- removed the now-obsolete runtime button script:
  - `LessonInteractableButton`
- updated the instruction selection draft to start the lesson after loading an instruction

Changed:
- runtime-generated UI, register-bank, and reset-button creation are now explicitly treated as fallback scaffolding rather than the preferred long-term scene workflow
- the main lesson flow now focuses on lesson state, progression, validation calls, and presentation updates instead of also constructing every runtime object itself
- script names were simplified where it mattered most for readability:
  - `InstructionFlowControllerDraft` -> `CpuLessonFlow`
  - `DatapathNodeRegistry` -> `NodeMap`

Next:
- when Unity is open again, co-author the lesson UI and interaction layout in-scene instead of relying on runtime placement for final positioning
- validate that the refactored lesson still behaves correctly in play mode
- begin extending the same lesson framework into `addi` after the `add` slice is confirmed stable

Risks / Notes:
- this refactor was done without opening Unity today, so behavior still needs in-editor validation later
- the scene/layout collaboration rule is now explicit: when placement matters visually, prefer user-guided scene authoring over silent code-built layout

## Current Working Baseline

### Scene / Interaction Baseline

- `Testing Ground` is the sandbox scene
- the prototype now auto-loads a playable `add` lesson on play
- the original scene advance button is still the primary progression input
- the scene can still build a runtime lesson UI, runtime reset button, and runtime register bank as fallback scaffolding
- minimal visual feedback is acceptable; heavy animation is not required

### Architecture / Script Baseline

Current relevant scripts:
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuNodeSequenceController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\CpuLessonFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\NodeMap.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonSetup.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonUI.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\RegisterBank.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\RegisterButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonChecks.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\ButtonFactory.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefinition.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionRuntimeSelection.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionEnums.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionUiLayout.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionDefaults.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystemV1\InstructionSelectionUiControllerDraft.cs`

Interpretation:
- these are useful scaffolding
- they have now been pushed into a working first-pass lesson system with a cleaner separation of responsibilities
- the next work should refine and extend them rather than replace them

## Immediate Next Steps

Recommended next development priorities:

1. Treat `add`, `addi`, and `lw` as the non-negotiable V1 set for the `2026-06-28` milestone.
   Recommended implementation order:
   - `addi`
   - `lw`

2. Validate the first playable `add` slice in headset.
   This means:
   - check readability of the lesson UI
   - check reachability/comfort of the register bank
   - confirm the advance/reset/register interactions feel reliable
   - decide what should stay runtime-generated versus what should become scene-authored

3. Refine the `add` lesson wording and pacing if needed.
   Focus on:
   - clearer stage prompts
   - cleaner wrong-answer feedback
   - stronger recap wording

4. Extend the same framework into `addi`.
   Reuse:
   - instruction assets
   - node registry
   - UI system
   - curated register bank workflow

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
