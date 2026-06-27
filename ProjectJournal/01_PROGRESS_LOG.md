# Progress Log

## Current Status

Project phase:
- scene-authored MVP lesson rebuild for the June 29 V1 check-in

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype focus:
- minimal playable lesson loop in `Testing Ground`
- scene-authored intro and per-step world-space UI
- authored 32-register MIPS bank with local reset
- register scanner validation path for `rs`, `rt`, and destination register
- keeping only the lesson/code scaffolding that still matches the current scene

Current milestone:
- V1 supervisor demo target on `2026-06-29`
- `add`, `addi`, and `lw` should be finished by `2026-06-28` at all costs
- this is the near-term prototype target, not the permanent upper bound of the whole project

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
- a cleaner lesson architecture split across focused scripts instead of one large controller, though some old lesson junk still needs pruning
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

### 2026-06-26 - Register Bank Moved Toward Permanent Scene Authoring

Completed:
- treated the user-created `Register Bank` object in `Testing Ground` as the canonical register area
- repurposed `RegisterBank` from a runtime button spawner into a scene-owned manager for authored register pieces
- added `RegisterToken` to represent one physical grabbable MIPS register
- added `RegisterBankAuthoring` to build the register bank in the Unity editor instead of during play
- added `RegisterBankResetButton` so the bank can have its own local reset control
- attached the register-bank authoring components to the existing `Register Bank` object in the scene file
- updated lesson reset/load flow so authored registers are snapped back to their home poses when the lesson resets
- updated `LessonSetup` so it prefers the authored scene register bank before creating any runtime fallback

Changed:
- the intended register interaction path is now based on grabbing physical register tokens rather than pressing runtime-generated register buttons
- the authored registers are set up to reuse XR Interaction Toolkit sample interactables as their base, especially the stock highlight affordance and dynamic-attach grab feel
- the register-bank reset is now separated from the lesson reset, so lost pieces can be returned home without restarting the whole lesson

Next:
- open `Testing Ground` in Unity and let the authoring script populate the 32 register objects plus the local reset button
- eyeball spacing, readability, and reachability on the bank plane with the user present
- save the scene after that editor pass so the generated register layout becomes permanently serialized
- test that grabbing a register still drives the current lesson flow correctly

Risks / Notes:
- Unity was not opened during this session, so the 32 authored register children are not yet serialized into the scene file
- the scene is wired so opening it in the editor should generate them under `Register Bank`; saving the scene afterward will make them permanent
- runtime lesson UI and other fallbacks still exist for now, but the register-bank direction is now explicitly scene-authored first

### 2026-06-26 - Register Bank Visual Pass Corrected In Repo

Completed:
- moved register-related scripts into `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers`
- added `D:\CompArchVR\ThePrototype\Assets\MyPrefabs\Registers\Register Token.prefab`
- changed the authored register direction away from the bad tiny-cylinder look
- reworked the authoring flow so it full-rebuilds register tokens from the custom register prefab
- changed the authored token presentation toward the earlier chunky labeled design:
  - base block
  - top block
  - floating register label
  - highlight affordance glow bound to the top block
- kept the registers grabbable with XR grab behavior instead of button-only behavior
- kept a local push-button reset path for the register bank

Changed:
- the register bank no longer depends on the sample cylinder as its visible body
- the register visuals are now intended to match the earlier look more closely while still behaving like grabbable objects
- register code is no longer buried inside the `CpuLesson` folder

Next:
- let Unity recompile and rebuild the `Register Bank` objects in-scene
- confirm the new register size, spacing, glow, and label readability visually
- tweak the layout with the user if the bank needs another sizing pass

Risks / Notes:
- this pass was done from the repo side without visually validating the rebuilt result in Unity yet
- the previous bad register objects shown in-editor were likely unsaved generated objects; the new authoring pass should replace them on rebuild

### 2026-06-26 - Register Bank Baseline Confirmed And ALU Zone Direction Chosen

Completed:
- confirmed in Unity that the register-bank local reset button works and returns all register tokens to their home poses
- confirmed the existing CPU lesson flow still works with the current register-bank setup
- disabled automatic lesson start on play so scene edits can be made more safely during iteration
- documented the current color behavior of lesson-selected registers:
  - hover/grab feedback can temporarily change the body color
  - validated lesson picks can remain green until reset
  - the local register reset restores them to their default visual state

Changed:
- the project baseline is now a scene-authored 32-register bank rather than a bank still waiting to be generated and saved
- the next intended interaction pattern for `Execution` and `WriteBack` is no longer abstract "selection only"
- instead, those phases should use physical placement pedestals that inspect the dropped register token and validate it against the current lesson step

Next:
- build authored pedestal zones for the `ALU` / `Execution` phase and later `WriteBack`
- let each pedestal validate only during its active lesson step
- let the pedestal accept a register after a short stable condition such as:
  - the token being ungrabbed
  - or the token resting in-zone for roughly 1-2 seconds
- use those pedestals later to support instructions that reuse the same logical register in more than one role by scanning and storing the register identity rather than requiring duplicate physical copies

Risks / Notes:
- the current "selected register stays green" behavior is acceptable for now but should be revisited later if it becomes visually confusing during longer multi-step lessons
- the pedestal interaction plan is the current preferred route for `EX` and `WB`, because it preserves physical VR interaction while still allowing lesson gating and duplicate-register handling

### 2026-06-27 - Scene-Authored UI Direction Locked In

Completed:
- confirmed that `Intro UI` in `Testing Ground` is a real `Spatial Panel Scroll` world-space UI under `Lesson Guide`
- stopped treating the scene as if the old placeholder-node/runtime-UI setup were still authoritative
- added minimal authored intro text into the `Intro UI` content area
- kept the register bank, register scanners, and local reset as the current physical interaction baseline
- accepted that the next lesson pass must be wired against the actual current scene, not the older placeholder layout

Changed:
- the scene-authored UI direction is now the primary path, not fallback
- future lesson steps should be represented by authored panels placed where the player needs them
- the next MVP should begin from the intro panel, show instruction/decode content, and then hand off to the register-zone panel

Next:
- add a real start button element to the intro panel content
- duplicate the intro panel into a register-zone lesson panel and retarget its content
- reconnect the lesson flow so it starts from the intro panel instead of auto-starting or depending on dead placeholder assumptions
- prune `MyScripts` so only the currently relevant lesson/register code remains

Risks / Notes:
- the repo still contains older lesson/runtime scripts that reflect superseded scene assumptions
- any lesson code changes should now be validated against the current authored `Testing Ground` scene first

### 2026-06-27 - Minimal Intro-To-Register MVP Rewired

Completed:
- attached a new slim `CpuLessonFlow` and `LessonGuideController` to `Lesson Guide`
- rewired the lesson around the current authored scene instead of the deleted placeholder-node setup
- removed the broken temporary intro text overrides from the scroll content
- kept the authored `Register Zone`, scanners, and reset button as the physical interaction baseline
- pruned old lesson/runtime-button scripts that no longer match the current scene

Changed:
- the MVP no longer depends on `CPU Placeholder Nodes`, runtime-spawned lesson canvases, or runtime XR register buttons
- `Lesson Guide` now owns lesson start/progression UI behavior
- the lesson should now begin from `Intro UI`, then hand off to a register-area panel for scanner-driven validation

Next:
- verify the two-panel lesson flow inside Unity play mode
- adjust the register-area panel position if it lands awkwardly relative to the authored bank
- if stable, extend the same pattern into `addi` and `lw`

Risks / Notes:
- the register-area panel is currently cloned from the intro panel at runtime for speed
- if the layout feels right, the next cleanup pass should serialize that second panel permanently into the scene

## Current Working Baseline

### Scene / Interaction Baseline

- `Testing Ground` is the sandbox scene
- the lesson framework exists, but it should not auto-start on play during general scene iteration
- the original scene advance button is still the primary progression input
- the preferred register path is now the authored `Register Bank` with 32 permanent register tokens
- minimal visual feedback is acceptable; heavy animation is not required

### Architecture / Script Baseline

Current relevant scripts:
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuNodeSequenceController.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\CpuLessonFlow.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\NodeMap.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonSetup.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonUI.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBank.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankAuthoring.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterToken.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterBankResetButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\RegisterButton.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\LessonChecks.cs`
- `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers\ButtonFactory.cs`
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
   - check reachability/comfort of the authored register bank
   - confirm the advance/reset/register interactions feel reliable
   - decide what should stay runtime-generated versus what should become scene-authored

3. Build the next interaction layer around placement pedestals.
   Focus on:
   - `Execution` / `ALU` pedestals that scan a placed register token
   - `WriteBack` pedestals that confirm the correct destination register
   - success / failure colors tied to active lesson-step validation
   - storing scanned register identity so reused registers can be handled cleanly

4. Refine the `add` lesson wording and pacing if needed.
   Focus on:
   - clearer stage prompts
   - cleaner wrong-answer feedback
   - stronger recap wording

5. Extend the same framework into `addi`.
   Reuse:
   - instruction assets
   - node registry
   - UI system
   - curated register bank workflow
   - pedestal validation workflow

6. Keep the post-V1 door open.
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
