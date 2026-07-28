# Rapid-Fire Q&A

## Core Pitch

**What is the project?**  
A VR learning environment for teaching single-cycle MIPS datapath execution through guided, practice, and test-based instruction walkthroughs.

**What is the core problem?**  
Computer architecture is hard to learn because important processes like control flow, value movement, and state change are dynamic, abstract, and largely invisible.

**What is the core idea?**  
Let learners stand inside the datapath and physically trace instruction execution instead of only reading static diagrams.

## Motivation

**Why this topic?**  
Because repeated teaching experience showed that students consistently struggled to build mental models of datapath behavior.

**Why are you personally motivated to do this?**  
Because while teaching Computer Organization, I repeatedly had to whiteboard instruction flow, stack updates, and signal behavior one-on-one for students.

**Why not just use MARS or a simulator?**  
Those tools are strong for correctness, but weaker at building intuitive spatial and temporal understanding for beginners.

## Why VR

**Why VR?**  
Because VR can make abstract and hidden processes spatially explorable and physically traceable.

**Why might VR help here?**  
Because this learning task depends on understanding flow across multiple interacting components over time.

**Why might VR not help?**  
Because immersion alone is not enough; poor VR can increase friction, discomfort, and cognitive overload.

**So why use VR despite that risk?**  
Because the prototype was deliberately constrained and guided to test whether careful VR design can help in this specific educational context.

## Research Question

**What is the research question?**  
To what extent does an immersive virtual reality-based learning environment improve students' understanding of abstract computer architecture concepts compared to traditional teaching methods?

**Why “to what extent”?**  
Because the effect may be mixed, partial, or context-dependent rather than simply yes or no.

**Are you proving VR is better than traditional teaching?**  
No. The current study is exploratory and formative, not a final controlled proof.

## Learning Outcomes

**How were the learning outcomes chosen?**  
From the real teaching problem, the scoped system design, and what the prototype can actually support and evaluate.

**What are the outcomes about?**  
Decoding, component understanding, flow tracing, control-facing decisions, state-change prediction, instruction-class comparison, and reduced-guidance completion.

**Which outcomes are most directly measurable?**  
Decoding, tracing, control-facing decisions, state-change prediction, and unsupported completion.

## Scope

**Why single-cycle MIPS?**  
It is complex enough to be meaningful, but still bounded enough for a stable dissertation prototype and study.

**Why not multicycle or pipelining yet?**  
Because they are the next research stage, not a small extension of a participant-ready baseline.

**Why MIPS?**  
Because it gave a clear, compact, and familiar basis for the final prototype and study.

**Why not a full simulator?**  
Because the goal was a focused educational environment, not a full processor implementation.

## System Design

**Why three modes?**  
To progressively reduce scaffolding while keeping the same underlying lesson structure.

**What does Learning mode do?**  
Guided walkthrough with explicit prompts, lesson text, hints, and corrective feedback.

**What does Practice mode do?**  
Requires the learner to decode and complete more independently with limited hints and attempts.

**What does Test mode do?**  
Acts as the strict assessment version with no lesson panel, no hint panel, and minimal tolerance for mistakes.

**Why random instruction selection in Test mode?**  
To reduce memorization and push transfer of understanding.

## Design Decisions

**Why was Decode narrowed to operand preparation?**  
So later phases still contain meaningful decisions instead of becoming passive confirmation steps.

**Why distribute control-facing decisions across later phases?**  
Because Execute, Memory, and Write-Back should remain active learning phases.

**Why separate register choice from later value flow?**  
Because physical registers are useful for Decode, but datapackets make later value movement easier to see and manage.

**Why use datapackets?**  
To make value transfer explicit and traceable through the datapath.

**Why use physical stations?**  
To turn the datapath sequence into a navigable spatial experience.

**Why not make every interaction more realistic?**  
Because educational clarity mattered more than realism for its own sake.

## Architecture

**What is the top-level architecture?**  
Authored lesson data feeds a shared lesson runtime, which coordinates scene panels, phase controllers, and learner-facing interactions.

**What is `CpuLessonFlow`?**  
The scene-facing lesson flow root that owns mode, active instruction, runtime state, and lesson progression.

**What is `LessonGuideController`?**  
The runtime coordinator that binds lesson flow to panels and phase stations.

**Why use catalogs?**  
To keep instruction and info ordering explicit and authored instead of relying on fragile folder discovery.

**Why not one giant controller?**  
Because that would make the system harder to reason about, extend, and debug.

## Research Study

**Why do a study at all?**  
Because the dissertation is not only about building the system, but evaluating whether it is educationally meaningful and usable.

**What is the study flow?**  
Consent and screening, tailored refresher, VR session, post-session ratings, knowledge check, and open-ended feedback.

**Why tailor the refresher?**  
To avoid confusing gaps in prior knowledge with failure of the VR system itself.

**Why use both Learning and Practice in the study?**  
Because together they show both supported use and more independent performance.

**What are you measuring?**  
Usability, engagement, clarity, confidence/perceived learning, knowledge-check performance, and open-ended feedback.

**Is this a controlled experiment?**  
No, not yet. It is a structured participant evaluation of a functioning prototype.

## Ethics

**What are the key ethical concerns?**  
Informed consent, voluntary participation, VR discomfort, privacy, data handling, and third-party headset/platform processing.

**How did you mitigate discomfort?**  
Participants are warned in advance and can pause or stop immediately without penalty.

**How is data handled?**  
Administrative data is separated from research responses, and research data is anonymized as early as practicable.

**Why mention Meta account processing in the documents?**  
Because even without intentional account-data collection, third-party device/platform processing should still be disclosed.

## Literature

**Which papers matter most?**  
Dascalu, Tan, Tlili, Pirker, Agbo, and Janani.

**Why Dascalu?**  
Direct precedent for VR in computer architecture learning.

**Why Tan?**  
Closest serious-game style implementation to this prototype.

**Why Tlili if it is not VR?**  
Because it still supports interactive/game-based approaches for architecture learning.

**Why Pirker / Agbo / Janani?**  
They position the work in VR for computer science education more broadly.

**Why Cook and Korkut?**  
They support caution around educational VR design, clutter, readability, and cognitive load.

**What is the research gap?**  
There is promise in VR for CS education and some architecture precedent, but focused tools for instruction-level datapath tracing and control-facing reasoning remain limited.

## Limitations

**What is the biggest limitation?**  
The system is intentionally limited to a single-cycle MIPS datapath.

**What else is limited?**  
Instruction coverage is a focused subset, and the evaluation is based on short participant sessions rather than long-term learning outcomes.

**What can you not claim?**  
That VR is universally better, that this proves long-term learning gain, or that the system teaches all of computer architecture.

## Future Work

**What is the clearest next step?**  
Extend the environment into multicycle and pipelined behavior, including stalls and hazards.

**Other future work?**  
Broader instruction coverage, adaptive guidance, larger studies, retention-focused evaluation, and classroom deployment.

## Skeptical Questions

**Is this just gamification?**  
No. Interaction is used to support tracing, reasoning, and decision-making, not to turn the system into a reward-driven game.

**Could this have been done in 2D?**  
Some parts, yes. The argument is that VR may add value for spatially tracing hidden flow, not that 2D tools are useless.

**How do you know any benefit is not just novelty?**  
I do not assume that. That is why the evaluation includes usability, clarity, and knowledge-related measures, and why future long-term studies matter.

**Why not augment an existing simulator instead?**  
That is a valid future direction, but this dissertation focused on a more embodied interaction model from the ground up.

**What if learners only learn the interface?**  
That risk exists, which is why the interface was kept closely aligned to architecture-relevant actions and followed by a concept-oriented knowledge check.

## Personal Reflection

**What did you learn from doing this?**  
That educational VR depends far more on scope control, guidance, and interaction justification than on immersion alone.

**What is your main contribution?**  
A focused VR architecture-learning prototype, a three-mode progression model, a set of grounded design decisions, and an in-progress participant evaluation.

## Emergency Recovery Lines

**If completely stuck**

- That is best understood as a scope decision rather than a missing feature.
- I would separate the educational intention from the engineering ambition there.
- I have been careful not to overclaim that point because the study is still in progress.
- That would be a strong next-stage extension once the current baseline is fully evaluated.
