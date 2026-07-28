# Decision Log

This file preserves the design decisions that still matter for the dissertation, presentation, and any later continuation work.

It is intentionally historical.
Do not treat it as a live state file without checking:
- `ProjectJournal/01_PROGRESS_LOG.md`
- `ProjectJournal/03_PROJECT_SNAPSHOT.md`

Everything below is kept in chronological order from oldest to newest, but overlapping micro-decisions have been merged so the file stays useful rather than bloated.

## 2026-06-16 - Foundational Scope And Motivation Decisions

### Why VR Must Be Justified Educationally

Decision:
- justify VR through guided spatial reasoning, not novelty

Why:
- the strongest project motivation came from earlier teaching-assistant experience in Computer Organisation at the University of Arizona
- students repeatedly struggled with datapath navigation, control signals, value flow, stacks, and function-call-related state changes
- the most effective intervention was usually to draw over the datapath and guide them through it step by step

Implication:
- the project should behave like a guided tracing supplement, not like a generic immersive spectacle
- in practice, this meant that almost every successful design choice had to answer a simple question: does this actually help a learner follow the datapath more clearly?

### Scope Must Stay Narrow And Defensible

Decision:
- prioritize one strong learning experience before expanding

Why:
- development time was limited
- a narrow polished experience was more defensible than a huge unfinished simulator

Implication:
- success does not require full MIPS coverage or a giant architecture sandbox
- the project could therefore prioritize coherence, completeness of the chosen lesson loop, and study readiness over raw architectural breadth

### VR And UI Should Do Different Jobs

Decision:
- use VR for meaningful physical/spatial choices
- use UI for prompts, reminders, lookup, and explanation

Use VR for:
- register selection
- datapath routing
- embodied phase progression

Use UI for:
- opcode/funct/control references
- instruction reminders
- feedback and explanation

Implication:
- not every hardware detail needs to become physical interaction
- this division of labor became one of the healthiest recurring design rules in the project, because it prevented the scene from turning into awkward VR-for-the-sake-of-VR interaction

### The Existing Instruction System Should Be Refined, Not Scrapped

Decision:
- keep the useful parts of `InstructionSystem` and evolve them

Why:
- instruction definitions and runtime lesson data were still valuable
- the main need was stage-driven validation, not total replacement

Implication:
- future changes should refactor useful scaffolding rather than restart from zero casually
- this saved time repeatedly later on, especially when the project expanded into practice and test behavior without discarding the guided baseline

### Journal Maintenance Is Part Of The Workflow

Decision:
- meaningful development sessions should update the repo journal

Why:
- the project needed persistent memory across long sessions, interruptions, and future thesis work

Implication:
- documentation upkeep is part of the process, not optional cleanup
- the journal was therefore expected to preserve not just what changed, but why the change seemed worth making at the time

### Keep Responsibilities Split Across Focused Scripts

Decision:
- avoid one controller owning scene setup, lesson state, UI, validation, and interactions all at once

Why:
- bloated controllers had already become hard to trust

Implication:
- future work should prefer smaller focused scripts, helpers, and explicit ownership boundaries
- even when one controller remained the public entry point, the internal logic should still be pushed into helpers or smaller units whenever possible

### Scene Layout Should Be Co-Authored, Not Silently Built

Decision:
- when visual placement and readability matter, prefer scene-authored layout over silent runtime construction

Why:
- layout quality has to be judged directly in Unity
- code-generated layout can be functional but still wrong in-scene

Implication:
- runtime-generated scene objects may exist as scaffolding, but not as the preferred final layout workflow
- this is one reason the project leaned so heavily into authored world-space panels and Inspector-bound references later on

## 2026-06-26 - Register Bank, Physical Tokens, And XR Feel

### Register Bank Should Be Scene-Authored

Decision:
- keep the register area as a permanent authored zone in `Testing Ground`

Why:
- spacing, readability, and reachability are scene-design concerns

Implication:
- extend the authored bank instead of replacing it with runtime-generated register UI
- this also kept the register zone readable as a physical area that the learner could walk into and reason about spatially

### Register Interaction Should Reuse XR Sample Feel

Decision:
- build register handling on top of XR Interaction Toolkit sample behavior where possible

Why:
- the sample feel was already preferred and lower-risk than rebuilding interaction basics from scratch

Implication:
- lesson behavior should layer on top of stable grab/highlight interaction rather than replace it
- whenever the project drifted too far from that stable baseline, it usually produced more friction than benefit

### Registers Should Stay Chunky And Readable

Decision:
- keep the blocky labeled register look

Why:
- readability and obvious affordance matter more than minimalism

Implication:
- future visual passes should preserve clear labels and strong physical identity
- the register tokens were not meant to look elegant first; they were meant to be graspable, legible, and hard to confuse

### Execution/Write-Back Should Use Physical Validation Zones

Decision:
- use authored pedestal/scanner-style validation rather than abstract UI-only confirmation

Why:
- the learner should physically place the correct object and see the datapath flow play out

Implication:
- later phases should continue validating through physical scene stations, not collapse back into pure menus
- this was important for keeping the experience grounded in visible state changes rather than abstract confirmation clicks

### Logical Register Values Must Stay Separate From Pose Reset

Decision:
- physical reset and logical value reset are separate concerns

Why:
- lesson-time register values need to survive ordinary prop recovery

Implication:
- local register reset remains pose-only
- value reset belongs to lesson/runtime control
- separating these two ideas also made it easier to justify persistent architectural state during longer lesson or practice runs

## 2026-06-27 to 2026-06-29 - Scene-Authored Lesson Flow And Phase Ownership

### Lesson UI Should Be Zone-Specific World-Space Panels

Decision:
- use authored world-space panels near the relevant interaction zones

Why:
- prompts should live near the action instead of on one giant detached interface

Implication:
- `Intro UI`, decode/register guidance, `ALU`, memory, write-back, and PC-update panels should all be treated as authored scene objects
- the scene was therefore organized less like a dashboard and more like a sequence of local teaching stations

### The MVP Baseline Is Intro -> Decode -> Register Setup

Decision:
- extend from the verified early lesson path rather than replace it

Why:
- the project already had a tested minimal slice that worked

Implication:
- `addi`, `lw`, and later phases should grow from the working `add` baseline
- this was a practical guardrail against breaking stable behavior while trying to broaden instruction coverage

### Authored Panels Must Use Layout Components Plus Runtime Rebuilds

Decision:
- authored UI remains in-scene, while code updates content and rebuilds layout when needed

Why:
- runtime text changes were not enough by themselves to keep authored panel layouts correct

Implication:
- future panels should follow the same authored-layout plus rebuild pattern
- in other words, the UI was not static, but it was still treated as authored structure first and runtime content second

### Decode Success Should Require An Explicit Continue Press

Decision:
- correct decode completion should show success feedback and wait for one more continue press

Why:
- this is clearer pedagogically than instant auto-advance

Implication:
- other gated steps can reuse this rhythm when immediate auto-progression feels too abrupt
- the user should be allowed to recognize success before the system quietly moves them elsewhere

### Core Lesson Objects Should Be Inspector-Wired

Decision:
- core lesson scene objects should be bound through serialized Inspector references

Why:
- authored scene wiring is easier to inspect and trust than runtime scene lookup

Implication:
- avoid `Find*`-style hookup for the core lesson path
- this became especially valuable once the scene grew dense enough that hidden runtime lookup would have been painful to debug

### ALU Should Use Datapackets, Not Drag Raw Registers Everywhere

Decision:
- after successful scanning, later phases should operate on emitted datapackets

Why:
- this better separates register identity from value flow
- it matches the mental model of reading values out of the register file and carrying those values onward

Implication:
- the register bank remains the source of operand identity
- datapackets carry the visible value flow through later stages
- this is one of the clearest examples of the project choosing teachability over literal hardware realism

### Control Decode Should Be A Real Gated Phase

Decision:
- keep decode as an authored lesson phase between intro and later physical work

Why:
- the scene and teaching flow already treated decode as a real checkpoint

Implication:
- later lesson work should extend after this baseline rather than bypass it
- decode had to stay educationally meaningful, because it was the point where instruction bits began turning into concrete datapath behavior

### ALU-Specific Choices Belong To Execute

Decision:
- keep `ALUOp` and `ALUSrc` in the execution phase instead of front-loading them in decode

Why:
- pushing all signal choices into decode would make later phases passive

Implication:
- execution keeps meaningful learner interaction instead of becoming a trivial confirmation step
- this decision strongly shaped the rest of the project, because it kept later stations active instead of reducing them to visual aftermath

### Decode Should Gather Only The Operands Actually Read

Decision:
- `ID` should prepare source operands, not finish destination handling

Why:
- this matches the source-read nature of the register file more cleanly
- it makes later write-back decisions meaningful

Implication:
- destination confirmation can be deferred to `WB`
- the result is not a perfect mirror of hardware internals, but it is a better teaching sequence for the learner-facing experience

### Write-Back Should Become Its Own Physical Phase

Decision:
- replace temporary explanation-only write-back handling with a real authored phase

Why:
- the learner should explicitly validate the final architectural update instead of having it hidden in a text transition

Implication:
- the final lesson loop must include real `WB` behavior
- write-back therefore became a visible instructional phase rather than a buried implementation detail

## 2026-07-09 to 2026-07-14 - Environment, Guidance, Audio, And Support Layers

### The Map Is Part Of The Teaching Experience

Decision:
- treat the routed environment as instructional structure, not scenery

Why:
- physical movement through fetch, decode, execute, memory, write-back, and PC update reinforces the datapath sequence

Implication:
- environment readability is part of lesson quality
- in this project, spatial layout was treated as part of the pedagogy, not merely as environmental presentation

### Branch Resolution Belongs To PC Update

Decision:
- treat `PC Update` as a dedicated final phase, especially for branch reasoning

Why:
- it gives the learner a clear place to reason about `PC + 4`, offsets, and next-PC choice

Implication:
- future control-flow work should extend from this phase rather than scatter PC logic across the lesson
- this kept control-flow reasoning from becoming a vague side-effect of earlier phases

### 3D Audio Is A Guidance Tool

Decision:
- use sound as instructional support, not just cosmetic polish

Why:
- directional cues help attract the learner toward the next relevant space

Implication:
- phase-change, success/failure, and wayfinding cues matter more than ambience-only polish
- the audio layer was therefore justified by usability first, and only secondarily by atmosphere

### Navigation Guidance Should Be Gated And Minimal

Decision:
- use authored doors/gates and only the currently relevant route arrows

Why:
- constant global arrow noise would reduce readability

Implication:
- guidance should reveal route, not flood the scene
- this supported the broader design goal of making assistance visible when needed without turning the whole world into a permanent HUD

### Tutorial Planning Should Keep Video And Card Paths Alive

Decision:
- keep both the video path and the coaching-card path available during onboarding planning

Why:
- both were cheap enough to preserve while the final onboarding direction was still unsettled

Implication:
- either path was acceptable as long as it stayed small and practical
- the project deliberately avoided turning onboarding into a second large subsystem

### Opening-Sequence Polish Should Stay Opt-In

Decision:
- keep offset/lerp opening helpers dormant by default and use them only when explicitly enabled

Why:
- the goal was polish experimentation, not scene-wide runtime movement

Implication:
- intro-sequence work should stay object-by-object and reversible
- this kept optional visual flair from threatening the stability of the main lesson

### Settings Menu Should Stay Lightweight

Decision:
- build on the XR wrist-menu baseline rather than replace it with a giant custom system

Why:
- the hand-menu foundation was already useful for resets, status, and recovery

Implication:
- settings work should focus on refinement, not architectural replacement
- the menu only needed to solve real usability problems, not become a feature showcase of its own

## 2026-07-17 to 2026-07-20 - Practice/Test Architecture And Study-Facing Rigor

### Practice Mode Must Extend The Existing Lesson Architecture

Decision:
- build practice mode on top of the stable guided lesson architecture

Why:
- the guided flow already worked and should not be destabilized with a second unrelated system

Implication:
- shared systems should only split where behavior truly differs by mode
- this is why practice and test were framed as stricter pedagogical overlays rather than separate lesson products

### UI Events Should Be Inspector-Bound

Decision:
- bind the main Unity UI events in the Inspector while keeping option population in code

Why:
- authored event wiring is easier to inspect and maintain

Implication:
- future UI changes should preserve scene-authored event hookup
- it also reduced the amount of hidden runtime behavior that could surprise later debugging sessions

### Register Values Should Persist During Ordinary Use

Decision:
- keep register values persistent during normal lesson use and restore authored defaults when the active mode changes

Why:
- this makes the register bank feel like a real stateful system, more in line with the persistent data-memory behavior

Implication:
- local register reset stays pose-only
- mode change is the safe authored rebaseline boundary
- that separation was important once practice and test began reusing the same bank under different instructional expectations

### Idle Decode Scanners May Fall Back To Utility Preview Use

Decision:
- when no lesson is active, the two lesson decode scanners may act as simple preview scanners

Why:
- otherwise they sit uselessly inactive between runs

Implication:
- once a lesson starts, they must return to strict lesson-owned behavior
- this was a small utility-minded decision, but it fit the broader preference for keeping scene objects useful when possible

### Practice Decode Should Extend One Decode Controller Through Helpers

Decision:
- keep a single decode-controller entry point and push practice-specific behavior into focused helpers

Why:
- decode is the most likely place for architecture sprawl

Implication:
- new practice decode behavior should be added through ref containers, helper views, and captured input state rather than a second controller stack
- this was one of the clearest efforts to extend behavior without letting one already-busy controller turn into an unreadable dumping ground

### Later Practice Phases Should Share A Common Budgeted Failure Rhythm

Decision:
- `EX`, `MEM`, `WB`, and `PC Update` should all share the same broad practice grammar:
  - limited validation attempts
  - limited scanner attempts
  - limited hints
  - held restart-on-failure state

Why:
- otherwise each phase would teach a different punishment/recovery pattern

Implication:
- practice mode feels like one system instead of four unrelated penalty rules
- consistency of failure handling mattered here almost as much as the actual phase content

### Dev Utilities Should Stay Centralized In Settings

Decision:
- keep skip/debug support in the settings menu

Why:
- phase-local debug buttons would clutter the scene and pollute the lesson UI

Implication:
- testing relief stays available without becoming learner-facing noise
- this also helped protect the study build from wearing its developer utilities on the main lesson surfaces

### Practice Instructions Should Overlay Learning Instructions

Decision:
- let practice instruction assets override a learning-mode base instead of duplicating full lesson definitions wholesale

Why:
- the later phase flow is mostly shared
- full duplication would create drift and maintenance overhead

Implication:
- learning-mode instructions remain the authoritative lesson baseline
- practice variation could then focus on what actually changed, especially decode difficulty and field presentation

### Practice Decode Should Use Typed Bitfields

Decision:
- use spatial-keyboard-backed text entry for practice decode fields instead of binary dropdown picking

Why:
- typed entry better breaks guided-mode muscle memory and makes decode more deliberate

Implication:
- future practice/test decode should reuse the same normalization and field-validation helpers
- in other words, the input method itself was part of the assessment design

### Practice/Test Must Not Leak The Decoded Assembly Too Early

Decision:
- keep player-facing readouts in encoded form during `IF` and `ID` for reduced-support modes

Why:
- showing the full assembly too early would undermine the entire point of decode

Implication:
- convenience UI must be audited for information leakage whenever mode difficulty changes
- this became a recurring concern whenever helper text or menu surfaces risked revealing too much

### Instruction And Info Bookkeeping Should Live In Authored Catalog Assets

Decision:
- use authored `ScriptableObject` catalogs for instruction selection and phase-info bookkeeping

Why:
- path-based folder discovery stopped being the right shape once the instruction bank and mode structure grew

Implication:
- future expansion should extend catalogs, not reintroduce path sweeps
- this also made the project easier to reason about as authored content rather than as a directory-driven runtime guess

### Decode Should Not Force rs Before rt

Decision:
- required source-register scans may complete in either order as long as scanner roles are still respected

Why:
- the meaningful lesson check is role correctness, not arbitrary sequencing

Implication:
- decode remains strict about what belongs where, but not about needless ordering friction
- this was a usability-softening decision that preserved rigor while removing a rule that taught nothing

### Test Mode Should Be A Harsh Practice-Derived Assessment Layer

Decision:
- reuse the practice foundation, but remove lesson/hint support, randomize instruction choice, and give only one scanner/validation failure per phase

Why:
- this creates a genuinely stricter independent-assessment mode without building a third unrelated lesson system

Implication:
- the three-mode structure stays conceptually clean and mechanically related
- test mode therefore remains legible as "practice with the supports stripped away," which is easy to explain to both participants and examiners

## 2026-07-26 to 2026-07-28 - Build Lock, Participant Truth, And Final Framing

### The Android XR Build Is The Authoritative Study Baseline

Decision:
- treat the current Android XR build as the main research baseline

Why:
- the project now covers the intended lesson loop and has already entered participant use

Implication:
- late changes should only happen if they solve a real blocker
- the standard for touching the build became much higher once it crossed into actual study use

### Participant Sessions Are A Better Source Of Truth Than Speculative Polish

Decision:
- use participant-facing regressions, not editor-only assumptions, as the priority source for urgent fixes

Why:
- several real problems only surfaced during actual device-side study use

Implication:
- future last-mile fixes should be surgical and participant-driven
- participant sessions exposed issues that editor confidence alone had completely failed to reveal

### Presentation And Thesis Framing Must Match The Built Scope

Decision:
- anchor presentation and thesis framing to the implemented learning outcomes and the finished single-cycle build

Why:
- overclaiming is now riskier than underselling optional future work
- the strongest project story comes from repeated datapath-teaching pain points and the guided tracing response to them

Implication:
- the main claims should stay centered on:
  - decode
  - explain
  - trace
  - select
  - predict
  - compare
  - complete with reduced or no guidance
- future-work topics such as wider jump support, multicycle coverage, and pipelining should stay clearly separate from baseline claims
- the presentation and dissertation should therefore describe the build that truly exists, not the larger architecture-teaching platform it could become later
