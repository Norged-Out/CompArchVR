# Progress Log

## Current Status

Project phase:
- participant-study follow-through and dissertation revision phase

Current working scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current stable feature set:
- guided lesson support for `add`, `addi`, `lw`, `sw`, `sub`, `and`, `or`, `slt`, `beq`, `bne`, and `j`
- full three-mode baseline:
  - `Learning`
  - `Practice`
  - `Test`
- authored single-cycle phase spaces for fetch, decode, execute, memory, write-back, and PC update
- physical fetch flow through the `Instruction Module` and instruction terminals
- authored 32-register MIPS bank, datapacket flow, and dedicated ALU / memory / write-back / PC-update stations
- routed map, moving gates, and arrow guidance as the active navigation baseline
- wrist settings menu with status readouts, resets, guidance toggle, dev-mode tools, volume, FPS, and quit support
- spatial keyboard support, updated tutorial/onboarding surfaces, broad gameplay audio, and background music
- Android XR build stabilized enough to serve as the participant-study baseline

Current interaction systems already implemented:
- persistent register values with pose-only reset and mode-change rebaseline
- scanner-driven decode, immediate extension, ALU execution, memory access, write-back, and branch / PC-update handling
- practice-mode decode with typed bitfields, staged validation, limited hints, and limited attempts
- budgeted later practice phases with held failure states
- centralized dev-mode skip / force-complete support
- authored instruction and info catalogs

Current architecture truths:
- lesson UI is scene-authored and code-driven
- core lesson objects are expected to be wired through serialized scene references
- runtime scene lookup glue has been reduced and should not be reintroduced casually
- Unity UI event hookups are expected to be Inspector-bound instead of wired in code at runtime
- instruction selection and phase-info dropdown population now read from authored catalog assets instead of folder-wide runtime discovery
- the routed map is part of the lesson design, not just environment decoration
- the three-mode structure safely extends the same core lesson architecture instead of forking separate systems
- the current build direction is stability-first, not instruction-count-first

Current delivery state:
- supervisor checkpoint on `2026-07-24` is complete
- official presentation / demo target is now in its final ready state
- participant-study use has begun
- the Android XR build should now be treated as the real research baseline unless a blocker-grade regression appears

Current non-build focus:
- participant-session support
- thesis-facing consolidation of motivation, learning outcomes, study procedure, and references
- keeping documentation aligned with the actual built scope
- revising the research question and hypotheses so they match the prototype's real supplementary-tool scope
- cutting repetition across the written chapters during revision
- preparing a revised full dissertation draft for supervisor review by `2026-08-10`

Presentation support now prepared:
- deck under `D:\CompArchVR\Documents\Presentation\Presentation_PriyanshNayak.pptx`
- phone-readable script under `D:\CompArchVR\Documents\Presentation\presentation_script.md`
- recorded walkthrough demo prepared for presentation use

## Remaining Future Work / Post-Presentation Backlog

Main future-work items:
- additional jump-family work beyond `j`, such as `jal` or `jr`
- optional opening-sequence work if a reliable moving-platform or equivalent intro beat is still worth revisiting
- route/guidance tuning only if participant testing exposes a real need
- optional scanner/socket interaction expansion only if it can be done safely and consistently
- optional ambience / presentation polish beyond the current background music baseline
- delayed refactors for oversized scripts only if they do not destabilize the current study build

Higher-scope future directions:
- multicycle datapath extension
- pipelining
- stalls / hazards
- broader follow-up studies after the current participant-testing phase

Interpretation:
- the datapath lesson itself is complete enough for the presentation and studies
- the remaining risk is regression, not missing core single-cycle coverage
- future work should extend from this baseline rather than reopen the core lesson architecture

## Historical Milestone Archive

Everything below is preserved as condensed milestone history. It is intentionally shorter than the original day-by-day working diary, but it keeps the design evolution needed later for the dissertation and presentation.

Entries are kept in chronological order from oldest to newest.

### 2026-06-15 - Unity Prototype Baseline Established

Completed:
- added the Unity project to the repository and stabilized repo hygiene
- confirmed `Testing Ground` as the active prototype scene
- created the first CPU placeholder layout and early physical button interaction
- drafted the earliest `InstructionSystem` scaffolding
- established the expectation that the project would be driven from one main scene rather than a spread of disconnected experiments

Why it mattered:
- this established the repo, scene, and lesson-system baseline that all later work grew from
- it also made later cleanup easier, because even the roughest early work was still anchored to a persistent scene and repository history

### 2026-06-16 - Project Direction Narrowed Around A Defensible Dissertation Scope

Completed:
- reviewed the scope after supervisor discussion
- narrowed the project toward a small number of meaningful learning objectives
- anchored the "why VR?" answer in guided spatial reasoning rather than novelty
- created the persistent `ProjectJournal` workflow
- reframed the project away from a broad architecture sandbox and toward a teachable, defensible dissertation slice

Why it mattered:
- this was the real scope lock: one strong learning experience first, expansion second
- most later design decisions, including the three-mode structure and the refusal to turn the project into a full simulator, trace back to this point

### 2026-06-26 - Register Bank Direction Locked In

Completed:
- authored a permanent 32-register MIPS bank into `Testing Ground`
- introduced resettable physical register tokens and dedicated scanners
- committed to the chunky labeled register look and XR-sample interaction feel
- turned the register bank into a visible and explorable part of the scene rather than background support data

Why it mattered:
- the lesson shifted away from abstract UI-only register choice and toward embodied operand selection
- it also established one of the clearest examples of why VR was being used at all: physically choosing and tracing values instead of only reading a diagram

### 2026-06-27 - Scene-Authored UI Direction Locked In

Completed:
- treated the world-space lesson panels under `Lesson Guide` as the real source of truth
- stopped relying on older runtime-generated lesson UI assumptions
- leaned into authored scene placement, Inspector wiring, and layout-group rebuilds as the trusted workflow

Why it mattered:
- the project committed to authored scene panels and Inspector wiring instead of procedural layout
- this decision later shaped how almost every lesson, hint, and settings surface was built and maintained

### 2026-06-28 - Intro / Decode / Register MVP Verified

Completed:
- stabilized the early `add` path through intro, decode, and register preparation
- separated lesson flow and control-decode responsibilities more clearly
- confirmed scene-authored UI layout rebuilds were the correct baseline
- proved that the user could begin at the intro panel, fetch an instruction, decode it, and prepare operands without the system collapsing

Why it mattered:
- this was the first validated guided lesson slice, and it proved the project structure could work
- after this point, the project no longer felt hypothetical; it had a real playable teaching loop, even if only a narrow one

### 2026-06-29 - ALU And Write-Back Direction Became Concrete

Completed:
- moved ALU-specific decisions into the execution phase
- introduced datapackets as the carrier for value flow
- made local register reset pose-only
- prepared immediate/sign-extension groundwork for `addi` and `lw`
- turned write-back into a direction the project would eventually treat as a real physical phase rather than a hidden outcome

Why it mattered:
- the project stopped being only a guided register picker and became a real datapath walkthrough
- spreading the logic across later phases also protected the lesson from becoming "decode everything once, then walk through passive fetch quests"

### 2026-07-09 - Environment And Flow Became Part Of The Teaching Design

Completed:
- reworked the map into a routed multi-zone lesson space
- treated `PC Update` as a proper final phase, especially for branch logic
- started using route clarity and phase movement as instructional tools rather than decoration
- made the environment easier to read as a phase order rather than just as a themed room

Why it mattered:
- the environment itself began reinforcing datapath order physically
- this also made later guidance arrows, gates, and tutorial support far more meaningful, because they were now layered onto a deliberate route

### 2026-07-12 to 2026-07-14 - Support Systems And Final-Phase Usability Were Added

Completed:
- added settings-menu support as a lightweight wrist-menu baseline
- introduced navigation guidance, route arrows, and gate-driven progression
- kept tutorial planning alive through both video and coaching-card directions
- added opt-in opening-sequence polish helpers without forcing scene-wide use
- started treating participant usability and first-time onboarding as core product concerns rather than late polish

Why it mattered:
- the prototype became usable as a guided experience instead of only a technical walkthrough
- many of the later "quality of life" decisions grew directly from this period, especially the emphasis on recoverability and route clarity

### 2026-07-17 - Practice Mode Groundwork And Register-Bank Rebaseline Direction Landed

Completed:
- began extending the stable lesson architecture toward `Practice` instead of forking it
- moved major UI event hookups to Inspector-bound callbacks
- formalized the authored register defaults and persistent-value direction
- made mode changes the clean boundary for restoring authored register values
- started reshaping old code paths so the new mode work would not be glued on top of brittle assumptions

Why it mattered:
- this created the foundation for reduced-handholding play without destabilizing the guided lesson
- it was also one of the key cleanup periods where scene wiring, runtime ownership, and register-state policy were brought into better alignment

### 2026-07-18 - Practice Mode Became A Real End-To-End Path

Completed:
- finished the first playable practice slice around `add`
- extended practice behavior through `ALU`, `Memory`, `WB`, and `PC Update`
- added budgets for hints, validation attempts, and scanner attempts
- added centralized dev-mode skip support
- stabilized scanner-failure behavior and imported spatial keyboard support
- established the stricter decode experience where learners must actively reconstruct instruction fields instead of being handed everything

Why it mattered:
- practice mode stopped being an idea and became a real, playable extension of the lesson
- it also confirmed that the same environment could support more than one pedagogical intensity without needing a second world or second lesson architecture

### 2026-07-19 - Catalogs Replaced Folder Discovery And Decode Became Less Punitive

Completed:
- replaced folder-wide discovery with authored instruction and info catalogs
- removed decode's forced `rs`-then-`rt` ordering
- reduced reliance on path-based resource assumptions that could become fragile in builds

Why it mattered:
- the lesson data model became more build-safe, and decode became less punishing without losing rigor
- this was one of the cleaner examples of a small mechanical change improving rigor and usability at the same time

### 2026-07-20 - Test Mode Finalized

Completed:
- implemented `Test` mode on top of the established practice foundation
- hid lesson/hint panels in `Test`
- locked `Test` to strict randomized instruction selection and one-mistake budgets
- completed the intended three-stage learner progression from supported walkthrough to independent assessment

Why it mattered:
- the project achieved the final three-mode structure:
  - guided learning
  - reduced-support practice
  - strict assessment
- this also gave the dissertation a cleaner educational story, because progression and scaffolding could now be discussed explicitly rather than implied

### 2026-07-24 - Final Study-Build Polish Landed

Completed:
- updated tutorial surfaces and onboarding
- added background music
- improved route readability and small UI/interaction QoL passes
- added the learn-mode `j` instruction as a late bounded control-flow inclusion
- reduced a large amount of presentation-facing roughness without reopening the project architecture

Why it mattered:
- the project entered a feature-complete state for presentation and study use
- from this point onward, the default question was no longer "what is still missing?" but "what still needs to be made safer or clearer?"

### 2026-07-26 - Android XR Build Stability Recovered

Completed:
- traced and fixed the build-only startup regression
- preserved only the safe socket-style interaction changes
- treated the Android XR build as the study baseline from this point onward
- accepted that device-side truth mattered more than editor-side confidence

Why it mattered:
- the risk surface shifted from feature gaps to build trust and deployment safety
- this was a major emotional and practical pivot, because it forced final development to prioritize reliability over experimentation

### 2026-07-27 - Participant Hotfixes And Presentation Framing Consolidation

Completed:
- fixed build-only scanner/UI regressions exposed by real participant use
- corrected register label clarity and participant-facing wording issues
- stepped back from unstable late QoL experiments
- consolidated the project framing around actual learning outcomes, participant study, and prior teaching experience
- used real study sessions to reveal issues that had not shown up during ordinary Unity testing

Why it mattered:
- participant sessions, not editor-only testing, became the real source of truth for urgent fixes
- this also helped sharpen the presentation story by reconnecting the build back to the original teaching motivation

### 2026-07-28 - Presentation Material Consolidation

Completed:
- cleaned and simplified the presentation support folder
- finalized the markdown speaking script for phone-readable use
- tightened presentation framing around the actual deck, study procedure, and supported learning outcomes
- aligned the narrative, build, and documentation so they all described the same bounded project

Why it mattered:
- the project is now being carried forward more by presentation/study delivery than by new system development
- at this stage, the journals matter less as a live engineering scratchpad and more as a stable historical record for the dissertation

### 2026-07-28 - Final Review And Demo Recording

Completed:
- performed one last participant-facing bug-fix pass around lesson behavior and presentation clarity
- recorded a full walkthrough covering all three learning modes
- confirmed that the deck, build, and supporting materials now describe the same final project state

Why it mattered:
- the focus is no longer feature completion, but confident delivery of the already-built system
- this marks the point where the project moved from active prototyping into demonstration and study-use readiness

### 2026-07-31 - Thesis-Prep Documentation Reorganization

Completed:
- reorganized the `Documents` folder into clearer buckets for ethics, literature, methodology, presentation, and project reference material
- moved bibliography work into the root `Latex` folder to separate writing assets from presentation/study notes
- restored fuller learning-outcome and methodology support notes after an earlier over-trim
- reintroduced stronger curricular grounding around University of Arizona teaching experience and Trinity course context
- separated reusable study-note templates from private participant-session notes

Why it mattered:
- the repository now better reflects the project’s transition from build work to dissertation-writing support
- the methodology and learning-outcome notes are now more useful as reference material for later thesis chapters instead of reading like overcompressed summaries

### 2026-08-07 - Supervisor Feedback Reshaped The Dissertation Revision Plan

Completed:
- reviewed the current dissertation draft with the supervisor and received a clearer direction for the full revision pass
- confirmed that the research question may and should be revised so it matches the project's actual scope as a supplementary learning tool
- confirmed that the methodology, hypotheses, and broader framing need to be brought into line with that revised question rather than defended through interpretation alone
- confirmed that repeated re-explanation across Chapters 1 to 4 should be cut down substantially during revision
- confirmed that the implementation chapter should emphasize what was built and how the traditional datapath was translated for the learner, rather than reading like an inventory of classes and internal construction details
- confirmed that the remaining chapter structure should separate `Results`, `Discussion`, and `Conclusion and Future Work`
- confirmed that the dissertation must make its contribution more explicit and should prioritize quality and value over page-padding
- locked the next major writing deadline as a revised full draft for supervisor review by `2026-08-10`

Why it mattered:
- this removed the biggest thesis-side structural problem, namely the mismatch between the broad comparative research question and the narrower exploratory study that was actually carried out
- it also gave the revision pass a much cleaner target: less repetition, stronger contribution framing, and a more explicit datapath-to-VR translation story
