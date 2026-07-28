# Presentation Q&A Preparation

## How To Use This File

- Use this as a revision pack, not a script to memorize word-for-word.
- For most questions, start with the **short answer**, then expand only if needed.
- Do not overclaim. The safest framing is:
  - this project is a **focused educational prototype**
  - it is designed to support learning of **single-cycle MIPS datapath execution**
  - the current study is an **in-progress participant evaluation**, not a final proof of effectiveness

## One-Minute Project Summary

### If asked: "What is your project in one minute?"

**Short answer**

This project explores whether virtual reality can help students understand abstract computer architecture concepts more intuitively. I built a self-paced VR learning environment around a single-cycle MIPS datapath, where learners physically move through Instruction Fetch, Decode, Execute, Memory, Write-Back, and PC Update. The system uses three modes, Learning, Practice, and Test, to progressively reduce guidance. The main goal is not to replace traditional teaching, but to supplement it by making instruction flow, value movement, and control-facing decisions more visible and interactive.

**Longer answer**

The core motivation came from repeated student difficulties I observed while teaching computer organization. Many students could eventually reach the correct answer, but they struggled to build an intuitive mental model of how registers, ALU operations, memory access, control signals, and program-counter updates all fit together during execution. So instead of building a general-purpose simulator, I built a focused VR environment that lets learners step through the datapath as a structured experience. The project is being evaluated through participant sessions using guided VR tasks, Likert-style experience measures, a knowledge check, and open-ended feedback.

## Core Defense Strategy

### What this project is

- A focused VR learning environment for **single-cycle MIPS instruction execution**
- A supplement to existing teaching materials
- A prototype designed around explicit learning outcomes
- A research artifact being evaluated through participant study sessions

### What this project is not

- Not a full MIPS simulator
- Not a replacement for lectures, diagrams, or conventional coursework
- Not yet a multicycle or pipelined architecture tool
- Not yet a final large-scale empirical proof of learning improvement

## Likely Opening Questions

### Why computer architecture?

**Short answer**

Because it is a domain where many critical processes are abstract, dynamic, and largely invisible, which makes it difficult for students to form accurate mental models.

**Expanded answer**

Computer architecture asks students to reason about interactions between registers, control signals, ALU operations, memory access, and program-counter behavior over time. In many courses, these are introduced through static diagrams and textbook explanations. Those materials are useful, but they often do not make flow and state changes intuitive. This makes the area particularly suitable for an approach that emphasizes spatial tracing and staged interaction.

### Why this topic specifically?

**Short answer**

Because I had already seen this learning difficulty firsthand as a teaching assistant.

**Expanded answer**

During my Bachelor's at the University of Arizona, I was a teaching assistant for Computer Organization for four consecutive semesters across two years. In office hours, I repeatedly saw students struggle with the same issues: navigating the datapath diagram, understanding which control signals mattered for which instruction, following stack and memory behavior, and mentally tracing how values moved through the processor. The most effective support often came from drawing over the datapath and walking through it step by step. This project grew directly from that teaching experience.

### Why VR?

**Short answer**

Because VR can make abstract, invisible, and time-dependent processes more tangible through spatial interaction and guided exploration.

**Expanded answer**

The appeal of VR here is not novelty by itself. It is that VR can place the learner inside a representation of the datapath, where movement, station layout, object transfer, and staged interaction help externalize processes that are normally hidden. The learner is not only reading about instruction flow but physically traversing a structured environment that mirrors it. That is the key educational hypothesis.

### Why not just use a normal simulator?

**Short answer**

Traditional simulators are strong for correctness and inspection, but they usually do less to support intuitive spatial and temporal reasoning for beginners.

**Expanded answer**

Simulators such as MARS are excellent for writing and testing assembly and checking results, but they are mostly screen-based tools. They help users inspect outputs and internal state, but they do not necessarily help first-time learners build an embodied understanding of how values and decisions move through the datapath over time. My project is intended to complement, not replace, those tools by focusing on conceptual walkthrough and mental-model formation.

## Why VR Might Work

### Why do you believe VR is actually helpful here?

**Short answer**

Because the learning task is strongly tied to tracing flow, comparing stages, and understanding spatially distributed components that interact over time.

**Expanded answer**

This is not a case where VR is being used because something can be made three-dimensional. It is being used because the target knowledge depends on understanding sequences, dependencies, and state changes across multiple components. The environment maps each major phase to a dedicated location, makes object flow visible through fetched modules and datapackets, and uses physical interaction only where it improves clarity. The design is aligned with the specific cognitive challenge rather than with VR for its own sake.

### Why VR might *not* work?

**Good defensive answer**

That is a valid concern, and the literature also warns about it. VR can introduce friction, discomfort, novelty effects, and cognitive overload if badly designed. That is exactly why this prototype was deliberately scoped, heavily guided in Learning mode, simplified in its interactions, and evaluated with usability and experience measures alongside the knowledge check. The project does not assume immersion automatically improves learning; it tests whether a carefully constrained design can help in this particular context.

## Research Question Defense

### Why this research question?

**Research question**

> To what extent does an immersive virtual reality-based learning environment improve students' understanding of abstract computer architecture concepts compared to traditional teaching methods?

**Why this question works**

- It is grounded in a real educational difficulty
- It connects the system design to measurable educational outcomes
- It is broad enough to justify a prototype and study, but specific enough to stay on architecture learning
- It does not assume VR is better; it asks whether and to what extent it helps

### Why say "to what extent"?

Because the goal is not to force a binary yes/no claim. VR may help some aspects more than others. For example, it may help tracing, flow visibility, or engagement more strongly than raw factual recall. That wording leaves room for a more honest and nuanced evaluation.

### Why compare against traditional teaching methods if you are not running a full controlled experiment?

**Safe answer**

The research question is framed against the broader teaching context, but the current study should be presented as an exploratory, formative evaluation rather than a definitive controlled comparison. The project is testing whether the environment is usable, educationally coherent, and promising enough to justify stronger comparative studies later.

## Learning Outcomes Defense

## Final Learning Outcomes

1. Decode the relevant fields of a supported single-cycle MIPS instruction.
2. Explain the role of the major datapath components involved in instruction execution.
3. Trace an instruction and its data through the major stages of a single-cycle datapath.
4. Select the correct operands and control-facing choices needed to execute an instruction.
5. Predict the architectural state changes caused by a supported instruction.
6. Compare how different instruction classes change datapath behavior.
7. Complete a supported instruction walkthrough with reduced or no guidance.

### Why these learning outcomes?

**Short answer**

Because they align tightly with what the prototype actually teaches and what the learner can demonstrably do inside the system.

**Expanded answer**

The learning outcomes were not written as generic ambitions. They were derived from the actual educational problem, the project scope, and the concrete interactions supported by the prototype. The early outcomes focus on identifying and explaining, then tracing and selecting, then predicting and comparing, and finally completing the walkthrough with less support. That progression also matches the structure of Learning, Practice, and Test mode.

### How did you come up with them?

They came from three sources:

- repeated teaching difficulties observed in practice
- the scoped instructional goals of the prototype
- what the environment can actually evaluate through interaction and post-session questioning

In other words, they were chosen by asking: what should a learner know or be able to do after using this system, and which of those can this prototype meaningfully support?

### What did your supervisor mean by implementing learning outcomes one at a time?

**Good answer**

I interpret that as making sure each major interaction and design decision can be justified against a concrete learning objective, rather than building a large experience first and only afterward trying to claim educational value. The prototype evolved in exactly that way: decode, tracing, operand selection, control-facing decisions, and state-change reasoning were built as separate capabilities and then integrated.

### Which learning outcomes are most directly measured?

Most directly:

- decoding instruction fields
- tracing instruction flow
- selecting operands and control-facing choices
- predicting resulting state changes
- completing the walkthrough with reduced support

These are reflected both in the VR tasks and in the knowledge check / participant behavior.

## Literature and Related Work

## The Literature Story in One Line

The literature suggests that VR can help make abstract computer science concepts more tangible, that architecture learning is a valid but still relatively underexplored target, and that good educational VR depends on careful guidance, visualization clarity, and cognitive-load management.

## Papers You Should Be Ready To Mention

### Core architecture-learning papers

#### Dascalu et al. (2017)

**Why it matters**

- Direct precedent for VR in computer architecture learning
- Supports immersive visualization of hidden hardware processes
- Good comparison point for educational intent

#### Tan et al. (2023/2024 in your notes)

**Why it matters**

- Closest implementation to your final prototype
- Serious-game framing
- Supports staged learning through interaction

#### Tlili et al. (2016)

**Why it matters**

- Not VR, but useful evidence that interactive/game-based learning can help architecture topics
- Helpful as a non-VR comparison point

#### Alnuaimi and Awad (2025)

**Why it matters**

- Nearby systems / hardware education context
- Useful for usability-study precedent

### Broader VR in CS education papers

#### Pirker (2020 / 2021)

**Why it matters**

- Strong general motivation for VR in computer science education
- Supports the idea that immersive visualization can help with abstract concepts

#### Agbo et al. (2021)

**Why it matters**

- Systematic review grounding
- Helps position the work inside a broader CS-education literature base

#### Janani et al. (2026)

**Why it matters**

- Very recent scoping review
- Useful for showing that the area is still active and evolving

### Design-constraint / cautionary papers

#### Cook (2019)

**Why it matters**

- Reminds you that educational VR is not automatically effective
- Supports discussion of usability, deployment, and design constraints

#### Korkut and Surer (2023)

**Why it matters**

- Useful for visualization, clutter, readability, and interaction tradeoffs
- Supports your simplified phase layout and guidance decisions

#### Samala et al. (2025)

**Why it matters**

- Broad educational VR context
- Good for high-level framing, less central to the architecture-specific case

#### Lee et al. (2020)

**Why it matters**

- Supports the argument that VR can deepen intuitive understanding
- Also warns against complexity overwhelming the learner

## If asked: "What is the research gap?"

**Short answer**

There is good evidence that VR can support learning in computer science more broadly, and some precedent in computer architecture specifically, but focused tools for instruction-level datapath tracing, value movement, and control-facing decision-making remain limited.

**Expanded answer**

The literature shows three things. First, VR is promising for abstract and invisible concepts. Second, architecture learning is a legitimate target for immersive educational tools. Third, good educational VR depends on careful design and not just immersion. The remaining gap is that there are still relatively few focused environments built specifically around instruction execution as a step-by-step learner experience, especially one that combines guided walkthrough, reduced-support practice, and strict test progression inside the same system.

### If asked: "What did you actually learn from the literature?"

Good points to say:

- VR is most useful when it clarifies hidden processes, not when it merely adds spectacle.
- Guidance matters; immersion alone is not enough.
- Cognitive overload is a genuine risk.
- Interaction should be meaningful, not excessive.
- Educational VR benefits from staged progression and careful scaffolding.
- There is enough precedent to justify the idea, but still enough gap to justify building a new focused system.

## Scope and System Design

### Why single-cycle MIPS?

**Short answer**

Because it is complex enough to be educationally meaningful, but still bounded enough to build, explain, test, and evaluate within a Master's dissertation.

**Expanded answer**

A single-cycle MIPS datapath provides the essential educational structure: fetch, decode, execute, memory access, write-back, and PC update. It lets the learner reason about instruction classes, control choices, operand flow, and state changes without immediately introducing the further complexity of multicycle timing, pipelining, stalls, and hazards. It was the right level of ambition for a research prototype.

### Why MIPS and not RISC-V?

**Defensible answer**

The project ultimately used MIPS because the educational framing, datapath conventions, and available teaching familiarity were better aligned with the prototype as it evolved. The underlying educational problem is not unique to MIPS, but MIPS provided a clear, well-known, and compact basis for the final implementation.

### Why not make a full simulator?

Because that would have stretched the project away from its educational purpose. A full simulator would increase engineering scope dramatically, but would not necessarily improve the teaching value of the core learning experience. The project needed to stay teachable, stable, and evaluable.

### Why use three modes?

Because a single mode cannot serve every learning stage equally well.

- **Learning** supports first exposure
- **Practice** reduces scaffolding and forces independent decoding
- **Test** acts as strict assessment using the same lesson structure

This lets the project model progression rather than only explanation.

### Why not build three totally separate systems?

Because that would duplicate logic and fracture the learning structure. The stronger design was to keep the same underlying flow and progressively remove support.

### Why physically move through phases?

Because spatial traversal is part of the pedagogical idea. The environment is meant to turn datapath progression into a navigable sequence, reinforcing that instruction execution is not just a list of labels but a flow through distinct architectural responsibilities.

## Design Decisions You Should Defend

### 1. Decode was narrowed to operand preparation

**What you did**

Instruction Decode does not fully resolve every control decision up front. Instead, it focuses on field decoding, operand identification, register retrieval, and immediate preparation.

**Why**

If all control decisions were fully settled inside Decode, later phases such as Execute, Memory, and Write-Back would risk collapsing into passive confirmation steps. Distributing decisions across later phases preserves meaningful learner interaction where those architectural consequences actually matter.

### 2. Register identity and value flow were separated

**What you did**

Learners choose physical registers during Decode, but later phases operate on emitted datapackets instead of dragging the original register tokens everywhere.

**Why**

This keeps Decode grounded in identifying the correct sources, while making value movement clearer and easier to trace through later phases. It also reduces physical clutter and interaction friction.

### 3. Scene-authored world-space panels were preferred over heavy runtime generation

**What you did**

Most UIs and stations are authored directly in-scene and driven by code, rather than being fully generated at runtime.

**Why**

That kept layout, readability, and control stable. For a VR learning environment, visual predictability matters a lot. Scene authoring also made iterative adjustment of panel placement, scaling, and learner-facing clarity much easier.

### 4. Physical interaction was used selectively

**What you did**

You used physical fetching, scanning, extension, packet placement, and control-facing UI where they helped reasoning, but simplified or avoided interaction when realism would add friction without educational payoff.

**Why**

The goal was educational clarity, not full physical realism.

### 5. Practice and Test were built as extensions of Learning

**What you did**

You kept one core lesson architecture and layered stricter behaviors on top.

**Why**

This preserved conceptual continuity for the learner and kept the codebase more coherent and maintainable.

## Project Architecture Questions

### What are the main architectural layers of the system?

**Short answer**

Authored lesson data feeds a shared lesson flow runtime, which coordinates scene panels, phase stations, support systems, and learner-facing interactions.

**Expanded answer**

At the top level there are authored assets, especially instruction definitions and info catalogs. These feed the core lesson runtime, centered on `CpuLessonFlow`. `LessonGuideController` coordinates the flow with the scene-authored UI and station controllers. Then there are phase-specific controllers for Decode, ALU, Memory, Write-Back, and PC Update, plus support systems such as the register bank, immediate extender, instruction terminals, settings menu, and route guidance.

### Why not have one giant controller script?

Because that would make the system harder to reason about, harder to extend, and harder to debug. The project deliberately moved toward a split architecture with smaller focused classes such as flow services, panel refs, phase controllers, and support helpers.

### What is `CpuLessonFlow` responsible for?

- owns current lesson mode and active instruction selection
- coordinates lesson state and progression
- exposes the lesson API used by UI and phase systems
- delegates detailed behavior to smaller flow services

### What is `LessonGuideController` responsible for?

- binds the lesson flow to scene panels and phase controllers
- refreshes authored UI state
- coordinates mode-specific decode behavior
- handles high-level panel visibility and progression wiring

### Why use catalogs instead of folder-wide discovery?

Because explicit authored catalogs are a cleaner source of truth. They preserve ordering, avoid fragile resource scanning behavior, and make it easier to control which instructions appear in each mode.

## Mode-Specific Questions

### What does Learning mode do?

- guided walkthrough
- explanatory lesson text
- hint support
- immediate corrective feedback
- best for first exposure

### What does Practice mode do?

- learner must decode the instruction independently
- reduced or removed lesson-style support
- limited hints and attempts
- same downstream architecture, less scaffolding

### What does Test mode do?

- strict assessment layer on top of Practice-like behavior
- random instruction selection from the test pool
- no lesson panel / hint panel
- one chance, one scan budget style behavior

### Why random instruction selection in Test mode?

To reduce rote memorization of a chosen item and push the learner toward transferable understanding of the same lesson structure across instructions.

## Instruction and Interaction Questions

### Which instructions are supported?

**Learning**

- add
- addi
- lw
- sw
- sub
- and
- or
- slt
- beq
- bne
- j

**Practice / Test**

- add
- addi
- lw
- sw
- sub
- and
- or
- slt
- beq
- bne

### Why these instructions?

Because they cover the main categories needed for the educational goal:

- R-type arithmetic / logic
- immediate arithmetic
- load/store memory behavior
- branch control flow
- one basic jump example in Learning mode

This gives meaningful variety without turning the project into a full ISA implementation.

### Why is `j` only in Learning mode?

Because it was added late as a useful control-flow example, but not fully propagated into the stricter Practice/Test evaluation flow. That was a reasonable scope boundary for the current study baseline.

### Why not include pipelining and hazards?

Because that would be the next major research stage, not a small extension. The current prototype first establishes a clear single-cycle baseline before moving to multicycle or pipelined complexity.

### Why use datapackets?

Because they make value flow visible. Instead of telling the learner that data conceptually moves between components, the system makes that transfer concrete and trackable.

### Why use register scanners?

Because they turn operand retrieval into an explicit action. That helps connect decoded instruction fields to actual register values in the environment.

### Why sign extension as a separate action?

Because it is an important architectural step that students often memorize procedurally without really noticing where it happens. Making it explicit reinforces that immediate handling is part of the datapath.

### Why a settings menu?

Because participant-facing usability matters in a study setting. It provides reset, audio, guidance, diagnostics, and dev/testing support without cluttering the teaching flow itself.

## Research Study Questions

### Why do a research study at all?

Because the project is not only a technical build. It is a dissertation investigating whether the designed learning environment is educationally meaningful, usable, and worth further development. A study is needed to gather evidence rather than relying on intuition alone.

### Why this study structure?

Because it balances realism, safety, and practicality.

- participants first understand the study and provide consent
- background questions help contextualize their responses
- a tailored conceptual refresher prevents confusion caused purely by forgotten prerequisite knowledge
- VR tasks provide direct interaction with the system
- post-session measures capture experience, learning-related performance, and feedback

### Why tailor the briefing?

Because participants come with different backgrounds in both VR and computer architecture. The goal is not to trap them with prerequisite gaps unrelated to the prototype itself, but to evaluate the learning environment fairly.

### Why allow some assistance during the session?

Because this is a participant study, not an adversarial exam. The main goal is to observe how the system supports learning and where it breaks down, not to maximize participant failure. Assistance is minimized, but not forbidden if someone becomes genuinely stuck.

### Why use both Learning and Practice in the study?

Because together they show two different things:

- Learning shows whether the system can teach and orient
- Practice shows whether the learner can begin to act more independently

That is more informative than only showing one extreme.

### Why not rely only on questionnaires?

Because questionnaires alone would miss actual task performance and behavior. The study combines:

- observed interaction in VR
- Likert-style experience measures
- a knowledge check
- open-ended feedback

This gives a richer picture.

### What exactly are you measuring?

- usability / user experience
- engagement and clarity
- confidence / perceived learning
- knowledge-check performance on covered concepts
- qualitative strengths, weaknesses, and suggestions

### Is this a quantitative or qualitative study?

Best answer:

It is mixed-method in spirit. It includes structured ratings and a knowledge check, but also relies on qualitative written feedback and researcher observation notes. At the current dissertation stage, it is closer to a formative evaluation than a large-scale statistical study.

### Why not run a randomized control trial?

Because that would require a larger sample, tighter experimental control, and more mature deployment conditions than were realistic for the current dissertation scope. The present study is more appropriately framed as an exploratory evaluation of a functioning educational prototype.

### Why use Microsoft Forms?

Because it provides a practical, structured, and accessible way to collect screening data, experience ratings, the knowledge check, and open-ended feedback in a consistent format across participants.

## Ethics Questions

### What are the key ethical issues?

- informed consent
- voluntary participation
- right to withdraw
- VR discomfort risk
- privacy and data handling
- use of third-party VR hardware/platforms

### How did you address VR discomfort?

- participant information explicitly describes possible discomfort
- participants can stop immediately
- sessions can be paused or ended without penalty
- headset use is monitored during the session

### How did you address privacy and data handling?

- contact/admin data stored separately from research responses
- research responses anonymized as early as practical
- password-protected devices and approved storage
- no intentional collection of Meta account data

### Why mention Meta headset processing in the documents?

Because even if the research team does not intentionally collect account data, it is still good ethical practice to disclose that third-party hardware and software may involve device-level or platform-level processing outside the research team’s direct control.

### Why was ethics necessary for this project?

Because human participants are being asked to engage with a VR system, provide responses, and contribute data to research. The moment a dissertation moves from building a system to evaluating it with people, ethical safeguards become essential.

## Technical Implementation Questions

### What engine and toolkit did you use?

- Unity
- XR Interaction Toolkit
- Meta Quest target via Android XR build

### Why Unity?

Because it provided the fastest path to building an interactive VR environment with scene-authored world-space UI, XR interaction support, and iterative prototyping for a dissertation-scale project.

### Why XR Interaction Toolkit?

Because it provides a mature interaction framework for grabbing, UI interaction, sockets, hands/controller support, and world-space XR workflows, which made it a sensible base rather than building low-level interaction systems from scratch.

### Why not a custom engine or lower-level stack?

Because the dissertation’s contribution is in educational interaction design and evaluation, not low-level rendering or engine development.

### How is code structured?

High-level answer:

- instruction assets and catalogs define authored lesson content
- `CpuLessonFlow` manages lesson state and mode/instruction selection
- `LessonGuideController` coordinates scene UI and phase controllers
- phase controllers own ALU / Memory / WB / PC behavior
- support systems handle registers, terminals, immediate extension, settings, and guidance

### What were the hardest technical problems?

Likely examples you can mention:

- maintaining clean flow across three modes without duplicating the whole system
- keeping scene-authored UI and runtime logic synchronized
- stabilizing Android build behavior versus editor behavior
- getting participant-facing interactions and guidance polished enough for real sessions

### Why inspector-bound UI events instead of more code-side binding?

Because scene-authored UI was a deliberate design choice. Inspector-bound events keep the relationship between visual elements and runtime actions more explicit and editable in-scene, which fit the project’s workflow better.

### Why a lot of authored scene references?

Because for VR educational UI, exact placement, readability, and panel structure mattered. Scene-authored references gave stronger control than heavy runtime construction.

## Limitations Questions

### What are the biggest limitations?

1. The prototype is intentionally limited to a **single-cycle MIPS datapath**.
2. Instruction coverage is broad enough for the study but still only a **focused subset**.
3. The current evaluation is based on **short participant sessions**, not long-term learning or classroom deployment.

### Why are these acceptable limitations?

Because they are aligned with the dissertation’s actual scope. The project needed to produce a defensible, usable, evaluable prototype rather than an overextended and unstable simulator.

### What can you *not* claim?

Do not claim:

- that VR is universally better than traditional teaching
- that this prototype proves long-term learning gain
- that the system fully teaches computer architecture
- that the current study is equivalent to a large controlled experiment

Safer claims:

- the prototype is promising
- it supports the targeted learning outcomes
- participant testing is underway
- early observations help identify strengths and limitations

## Future Work Questions

### What is the clearest next step?

Move beyond the single-cycle baseline into **multicycle execution and pipelined behavior**, including stalls, hazards, and overlapping instruction flow.

### Other strong future-work points

- broader instruction coverage, especially more control-flow variants
- refined onboarding and adaptive guidance
- larger participant groups and stronger comparative evaluation
- retention-focused follow-up studies
- optional classroom deployment studies

### Why is pipelining future work and not current scope?

Because pipelining introduces a major jump in conceptual complexity, implementation burden, and evaluation scope. It is best treated as the next research stage rather than forced into a prototype already being used for participant study.

## Defense of Specific Choices

### Why use MIPS syntax with dollar-prefixed registers?

Because that matches the representation used throughout the teaching context and avoids ambiguity between register names and ordinary labels.

### Why keep register values persistent across lessons in the final system?

Because persistent state makes architectural consequences more meaningful, especially for repeated interaction and comparing before/after effects. At the same time, authored baseline values still exist for rebasing when needed.

### Why include audio?

Because audio supports state change awareness, confirmation, and pacing. In VR, auditory feedback helps reinforce interaction outcomes without requiring the learner to constantly inspect panels.

### Why include a map and route guidance?

Because the environment is spatial, so navigation itself can become friction. Guidance reduces wasted effort and helps the learner focus on architecture rather than getting lost.

### Why include a settings menu and dev mode in a research prototype?

Because participant-facing studies require stability and recoverability. Reset controls, guidance toggles, volume, and dev support are practical study tools, not just development conveniences.

## Hard / Skeptical Questions

### "Is this just gamification?"

**Strong answer**

No. The project uses interaction and progression, but the core design is not about reward loops or entertainment mechanics. It is about making instruction execution, value flow, and control-facing decisions more visible and learnable. If anything, the interaction is deliberately constrained to keep the educational purpose dominant.

### "Could this have been done just as well in 2D?"

**Balanced answer**

Some parts could absolutely be taught in 2D, and I do not claim VR replaces those tools. The question is whether VR can add value for this specific challenge by making hidden processes spatially traceable and physically navigable. The prototype is designed around that added value, not around the assumption that all educational problems require VR.

### "Why not augment an existing simulator instead?"

Good answer:

That is a valid alternative and could be strong future work. For this dissertation, the aim was to explore a more embodied interaction model from the ground up, where movement through stations and object flow were core to the learning experience.

### "How do you know improvements are not just novelty effects?"

Best answer:

I do not assume they are not. That is one reason the current evaluation is framed cautiously. Novelty may contribute to engagement, which is why usability, clarity, and knowledge-related measures all matter, and why long-term follow-up would be important future work.

### "Why not include a control group now?"

Because the current dissertation needed to prioritize building a stable, study-ready educational prototype first. A stronger controlled comparison would be a logical next-stage evaluation once the system and study process are more mature.

### "What if the participant just learns your interface rather than architecture?"

That is a real risk in any educational tool. My mitigation was to keep the interface focused on architecture-relevant actions, reduce unnecessary mechanics, and test not only experience but also concept-oriented questions and less-guided task completion.

## Questions About What You Personally Learned

### What skills did you acquire?

Good points:

- VR interaction design for education
- scoping and stabilizing a dissertation-scale prototype
- translating educational goals into system interactions
- building multi-mode progression on a shared runtime architecture
- participant-study preparation and ethical research workflow
- balancing technical implementation with pedagogical design

### What was the most important lesson from the project?

That educational VR succeeds or fails less on immersion itself and more on whether every interaction can be justified against a learning goal. Scope control and pedagogical clarity mattered more than feature count.

## Questions About the Study Being In Progress

### How should you talk about results if asked?

Use cautious wording:

- participant testing is underway
- early sessions are informing usability and clarity observations
- the study is designed to gather both structured ratings and qualitative feedback
- final claims should wait until data collection and analysis are complete

### If asked for informal observations so far

You can say things like:

- participants are able to engage with the environment and complete selected walkthroughs
- the sessions are useful for revealing clarity issues and interaction friction
- early observations reinforce the importance of onboarding, guidance, and careful wording

Do **not** overstate these as final findings.

## Why The Research Study Still Matters Even If Results Are Not Final

Because the study is part of the dissertation contribution. It shows that the project is not just a speculative design, but an implemented system undergoing real participant evaluation. Even before final analysis, the study process helps validate feasibility, usability, and educational coherence.

## Paper-Specific Quick Answers

### Why cite Dascalu?

Because it is one of the clearest direct precedents for VR in computer architecture learning.

### Why cite Tan?

Because it is the closest serious-game style architecture-learning implementation to this prototype.

### Why cite Tlili if it is not VR?

Because it supports the broader idea that interactive/game-based formats can help with architecture learning.

### Why cite Pirker / Agbo / Janani?

Because they position the project inside VR for computer science education more broadly, rather than making it look like an isolated one-off idea.

### Why cite Cook and Korkut?

Because they help justify design caution, especially around clutter, guidance, and educational VR constraints.

### Why keep DBMS / abstract-visualization papers in the wider reading set?

Because while the project pivoted away from DBMS as a topic, those papers still helped shape thinking about how VR can represent abstract, non-physical structures and why clarity matters.

## Questions On The Pivot

### Why did the project move away from the older DBMS angle?

Because the strongest final motivation, prior teaching experience, and most coherent educational use case were in computer architecture rather than database systems. The project became more defensible when it aligned directly with that lived teaching problem.

### Did the older reading still matter?

Yes. Some of the abstract-visualization and educational VR design lessons remained useful even after the topical pivot.

## If Asked About Novelty

### What is actually novel here?

Good answer:

The novelty is not that VR has never been used in education, or even that architecture has never been explored in VR. The novelty is the particular combination of:

- a focused single-cycle datapath learning environment
- progression across Learning, Practice, and Test modes
- physical tracing of instruction and value flow
- explicit alignment to learning outcomes
- participant-study evaluation in an educational VR context

## If Asked About Contribution

### What are your contributions?

1. A functioning VR learning prototype for single-cycle MIPS instruction execution.
2. A three-mode pedagogical structure that progressively reduces support.
3. A set of interaction and scope decisions grounded in both teaching experience and literature.
4. An in-progress participant study evaluating the system’s usability, learning support, and learner experience.

## If Asked What You Would Change With More Time

- add pipelining and hazards
- expand instruction family coverage further
- improve adaptive support
- strengthen classroom-style deployment and evaluation
- refine onboarding and learner state visibility further

## Fast Answers For Slide-Specific Questions

### Slide 2: Why is architecture hard?

Because hidden, dynamic processes are often taught with static diagrams.

### Slide 3: What does the literature say?

VR is promising, but only when clarity, guidance, and cognitive-load control are handled carefully.

### Slide 4: Why these learning outcomes?

They were chosen from the real teaching problem and from what the prototype can actually support and evaluate.

### Slide 5: Why this scope?

Single-cycle MIPS was the right boundary for a stable, teachable, study-ready prototype.

### Slide 6: Why three modes?

To progressively remove scaffolding without changing the underlying lesson structure.

### Slide 7: Why these interactions?

Because each one was kept only where it improved understanding of flow, values, or decisions.

### Slide 8: Why this walkthrough?

Because it mirrors the actual architectural sequence the learner is supposed to understand.

### Slide 9: Why this architecture?

To keep authored lesson content, runtime flow, and scene systems modular and maintainable.

### Slide 10: Why this study design?

Because it balances ethical practice, participant variability, practical feasibility, and the need for structured feedback.

### Slide 11: Why these design decisions?

Because they preserved meaningful learner interaction while keeping the system teachable and stable.

### Slide 12: Why these limitations?

Because they are the honest boundary conditions of a focused Master's dissertation prototype.

## Things To Avoid Saying

- "VR is definitely better than traditional teaching."
- "This proves students learn better."
- "The system teaches all of computer architecture."
- "This is basically a full processor simulator."
- "Pipelining was easy to add later."

## Better Alternatives

- "The project investigates whether VR can meaningfully support this kind of learning."
- "The current study is exploratory and formative."
- "The system focuses on a carefully bounded subset."
- "The prototype is intended as a supplement to existing teaching materials."
- "The next major extension would be multicycle and pipelined behavior."

## Final Confidence Checklist

Before the Q&A, make sure you can answer these without hesitation:

- Why architecture is hard to learn
- Why VR is justified here
- Why single-cycle MIPS was chosen
- Why the project is not a full simulator
- Why Decode was narrowed
- Why control decisions were spread across phases
- Why datapackets exist
- Why three modes exist
- What the research gap is
- Why the participant study is structured the way it is
- What the main limitations are
- What the clearest future work is
- What you learned from the literature
- What your personal teaching experience contributed

## Last-Resort Recovery Lines

### If stuck on a question

- "The clearest way to answer that is to separate the educational intention from the engineering scope."
- "I would frame that as a scope decision rather than an omission."
- "That is a fair challenge, and it is exactly why I have been careful not to overclaim the current evaluation."
- "The literature helped shape that choice, but the strongest driver was what the prototype needed to teach clearly."
- "That would be a strong next-stage study once the current prototype baseline is fully established."
