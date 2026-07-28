# Presentation Script

**Project:** Creating Novel Virtual Reality Interfaces for Teaching Computer Architecture  
**Presenter:** Priyansh Nayak

## Slide 1 - Title

Hello, my name is Priyansh Nayak, and this project explores how virtual reality can be used to support the learning of computer architecture. In this presentation, I will briefly explain the educational motivation, the system I built, how a learner experiences it, and how I am evaluating it through participant study sessions.

## Slide 2 - Why Computer Architecture Is Difficult to Learn

One of the core reasons computer architecture is difficult to learn is that many of its most important processes are effectively invisible. Students are expected to understand how registers, control signals, the ALU, memory, and the program counter all work together during instruction execution, but these processes are dynamic and abstract, while they are often introduced through static two-dimensional diagrams.

This problem was also very familiar to me from my own teaching assistant experience. During office hours, students repeatedly struggled to follow datapath flow, understand what control signals were doing, and reason about how values moved through the system. In many cases, the most effective help was to draw over the datapath and walk them through it step by step.

## Slide 3 - Existing Research and Design Implications

That motivation is also supported by the broader research context. Existing work in VR for computer science education suggests that immersive interaction can make abstract and invisible processes more tangible. Prior work in architecture learning and adjacent CS education also points toward the value of simulation, visualization, and experiential learning.

At the same time, the literature makes it clear that immersion alone is not enough. Clear visualisation, intuitive interaction, appropriate guidance, and good management of cognitive load are all important. So the takeaway for this project was not simply to build something immersive, but to build something focused, guided, and educationally deliberate.

## Slide 4 - Research Question and Learning Outcomes

This leads to the central research question:

> To what extent does an immersive virtual reality-based learning environment improve students' understanding of abstract computer architecture concepts compared to traditional teaching methods?

To support that question, the prototype was designed around a set of learning outcomes. At a high level, the learner should be able to decode a supported instruction, understand the major datapath components involved in execution, trace the instruction through the datapath, make the correct control-facing decisions, predict the resulting architectural state changes, compare different instruction classes, and eventually complete the full walkthrough with reduced or no guidance.

## Slide 5 - System Overview and Scope

The prototype itself is a self-paced VR learning environment centered on a single-cycle MIPS datapath. Learners move through dedicated stations for Instruction Fetch, Instruction Decode, Execute, Memory Access, Write-Back, and PC Update.

The system supports three modes: Learning, Practice, and Test. At the same time, it is intentionally not trying to be a full architectural simulator. Instead, it focuses on a selected subset of instructions and on learning outcomes related to decoding, tracing, decision-making, and reasoning about state changes.

At this point, I will move into the learner-facing part of the system and show what that looks like in practice.

## Slide 6 - Progressive Learning Modes

The three modes are designed as a progression in support. Learning mode gives the learner explicit prompts, explanatory scaffolding, and immediate corrective feedback. Practice mode reduces that support and requires the learner to decode and complete more of the process independently, with limited hints and attempts.

Test mode removes the lesson and hint support entirely and acts as the strict assessment version of the same lesson flow. So rather than teaching three separate systems, the environment gradually removes scaffolding while keeping the same underlying structure.

## Slide 7 - Core Interaction Design

The interaction design is based on a simple principle: use physical interaction only where it helps the learner reason more clearly about the datapath. Instructions are physically fetched at the beginning of the lesson. Registers are scanned to retrieve operand values. Datapackets then make value flow visible as the learner moves through later phases.

Each major datapath phase is represented as a dedicated station in the environment, and world-space UI panels provide prompts, validation, reference support, and feedback. Whenever additional realism would have added friction without improving learning value, I simplified the interaction instead.

## Slide 8 - Instruction Walkthrough

This slide shows the overall learner journey. The learner starts by fetching and transferring the instruction, then decodes its fields and identifies the required operands. They retrieve the correct register values and any required immediate, make the relevant decisions during execution, complete memory access or write-back where needed, and finally confirm the resulting register, memory, and program-counter changes.

The environment is designed so that this sequence is not only explained, but also physically navigated. At this stage, the prerecorded walkthrough is helpful because it shows how the learner actually experiences the system in motion.

## Slide 9 - System Architecture Flow

Behind that learner-facing experience is a structured lesson architecture. Authored lesson data, such as instruction definitions and information catalogs, feed into a shared lesson runtime. That runtime is coordinated through the lesson guide, which connects the scene panels, phase stations, and learner-facing interactions.

The main point here is not the code itself, but the architectural choice: the system relies on authored scene content and shared flow systems, rather than building everything dynamically from one large controller.

## Slide 10 - Research Study

To evaluate the system, I am running participant study sessions. Participants first read the project information material, sign the consent form, and complete a short screening and background section. I then give a tailored refresher on assembly, instruction flow, and datapath stages, depending on their background.

After that, they complete a structured VR session using selected Learning and Practice instructions, with minimal intervention from me unless they become genuinely stuck. Once the session ends, they complete system-experience ratings, a knowledge check, and open-ended feedback, and I also keep observation notes during the session.

## Slide 11 - Design Decisions and Scope Control

There were several important design decisions that shaped the final prototype.

First, I deliberately scoped the project as a single-cycle MIPS learning environment rather than a full simulator, because the goal was to make the lesson teachable, testable, and stable.

Second, I narrowed Instruction Decode to operand preparation and deferred destination selection to Write-Back, so that later phases still contained meaningful learner decisions instead of becoming passive confirmation steps.

Third, I separated register identity from value flow: learners choose physical registers during Decode, but later phases operate on datapackets, which makes value movement easier to trace through the datapath.

Together, these decisions helped keep the system focused and understandable.

## Slide 12 - Limitation and Future Work

The current prototype also has clear limitations. It is limited to a single-cycle MIPS datapath, and although it supports a useful instruction subset, it does not attempt to implement the whole architecture.

The current evaluation is also based on short participant sessions, so it does not yet tell us about long-term retention or broader classroom deployment.

The clearest future direction would be to extend the environment toward multicycle and pipelined behavior, including stalls and hazards. Other future directions include broader instruction coverage, stronger learner-support systems, and larger follow-up studies with stronger comparative evaluation.

## Slide 13 - Thank You / Questions

To conclude, the main contribution of this project is a focused VR learning environment that allows learners to step through datapath execution spatially, physically, and progressively, rather than only reading static diagrams.

Thank you very much, and I would be happy to take any questions.
