# Presentation Script

**Project:** Creating Novel Virtual Reality Interfaces for Teaching Computer Architecture  
**Presenter:** Priyansh Nayak

## Slide 1 - Title

Good morning, and thank you for being here.

My name is Priyansh Nayak, and this project explores whether virtual reality can be used as a useful supplemental tool for teaching computer architecture.

What I want to do today is move fairly quickly through the background, then spend most of the time on what I built, how a learner experiences it, and how I am evaluating it through my current participant study.

## Slide 2 - Why Computer Architecture Is Difficult to Learn

I want to begin with the teaching problem itself.

Computer architecture is difficult to learn partly because many of its most important processes are invisible. Students are expected to reason about registers, control signals, ALU behavior, memory access, and program counter updates, but these are usually introduced through static diagrams on a page.

So even when the diagram is technically complete, learners often struggle to build an intuitive sense of movement and dependency. They can see the picture, but they do not always understand the flow.

This was also something I saw very consistently during my own teaching assistant experience. During my Bachelor's at the University of Arizona, I worked as a teaching assistant for Computer Organisation across four consecutive semesters, and students kept struggling with the same cluster of issues: following datapath flow, understanding control signals, and tracking how values move from one stage to another.

What usually helped most was not more text. It was drawing over the datapath and walking through it step by step.

That, more than anything else, is the starting point of this project.

## Slide 3 - Existing Research and Design Implications

Once that teaching motivation was clear, the next question was whether VR was actually a sensible direction for this problem.

The literature suggested that it was, but with important caveats.

Across VR in computer science education more broadly, the research suggests that immersive systems can help make abstract processes more tangible through spatial interaction, visual embodiment, and guided exploration. In the architecture space specifically, the work that resonated with me most was the VR serious games paper by Tan et al., because it framed VR not just as a novelty, but as a possible instructional medium for architecture learning.

At the same time, the literature also makes something else very clear: immersion alone is not enough. If the interface is confusing, if the visuals are unclear, or if the learner is overloaded, then VR can easily become noise rather than support.

So the takeaway for me was not simply “VR is good.” It was much more specific than that.

There is evidence that VR can help with abstract learning, but there still seems to be room for more focused systems that help learners trace instruction execution, data movement, and control-facing decisions through a datapath in a structured way.

That is the gap I wanted to respond to, and it led directly to my research question.

## Slide 4 - Research Question and Learning Outcomes

The central research question is:

> To what extent does an immersive virtual reality-based learning environment improve students' understanding of abstract computer architecture concepts compared to traditional teaching methods?

I do want to make one thing explicit here: this project is not meant to replace traditional teaching methods. It is meant to supplement them.

That is why, in my participant sessions, I begin with a short tailored refresher rather than throwing someone directly into the headset without context. The VR environment is meant to reinforce, visualize, and strengthen understanding, not act as a complete substitute for lectures, diagrams, or guided explanation.

From that question, I defined a set of learning outcomes. By the end of the experience, learners should be able to decode a supported instruction, explain the role of major datapath components, trace instruction and data flow, select the correct operands and control-facing choices, predict state changes, compare instruction classes, and eventually complete a walkthrough with reduced or no guidance.

Those outcomes then shaped the scope of the system I built, which is what I will move into now.

## Slide 5 - System Overview and Scope

The prototype is a self-paced VR learning environment centered on a single-cycle MIPS datapath.

Instead of keeping everything inside one flat diagram, the major stages are turned into navigable stations: Instruction Fetch, Instruction Decode, Execute, Memory Access, Write-Back, and PC Update.

The learner physically moves through those spaces and interacts with instruction modules, registers, datapackets, and world-space panels while completing an instruction walkthrough.

This is also the point where I begin pairing the talk more directly with the prerecorded demo. [Glance at demo: **0:30**] The first clear learner interaction in the video appears at around **30 seconds**, where the guided lesson begins with physical Instruction Fetch.

The system supports three modes:
- Learning
- Practice
- Test

But it is intentionally scoped. It is not trying to be a full processor simulator. It focuses on a selected set of instructions and on the learning outcomes I just mentioned.

The next few slides really unpack that learner experience from three angles: the progression of support, the interaction design, and the actual walkthrough flow.

## Slide 6 - Progressive Learning Modes

The three modes are best understood as one shared lesson structure with different levels of support.

Learning mode is the most guided version. It gives explicit prompts, explanations, and immediate corrective feedback.

Practice mode reduces that support. The learner is asked to do more of the decoding and decision-making independently, with limited hints and attempts.

Then Test mode strips the support back further. The lesson and hint panels are removed, tolerance for mistakes is much lower, and the learner is expected to complete the same architectural journey with minimal help.

So the progression here is important: I am not teaching three separate systems. I am using one consistent lesson flow and gradually removing scaffolding.

That is also visible in the demo. Learning mode dominates the early part, [Glance at demo: **6:30**] Practice mode begins at around **6 minutes 30 seconds**, and [Glance at demo: **12:15**] the stricter Test mode appears later at around **12 minutes 15 seconds**.

So if slide 5 gave the overall shape of the environment, this slide gives the educational progression built on top of it.

## Slide 7 - Core Interaction Design

Once that progression was clear, the next design question was: which parts should actually be physical, and which parts should remain simple UI support?

My answer was to keep physical interaction only where it genuinely improves reasoning.

So, for example, instructions are physically fetched and transferred. Registers are physically selected during decode. Datapackets are then used to make value flow visible through later phases. World-space panels stay nearby to provide prompts, reference material, validation, and feedback.

The way I think about it is this: instead of asking the learner to stare at the datapath from the outside, I am trying to place them inside it. In a sense, they are following the journey that the instruction and its values would normally take through the processor. Another way to put it is that the learner is almost acting like the moving signal inside the machine, stepping through each decision point rather than only observing it after the fact.

That is the educational goal. The learner is not just being told that information moves. They are tracing that movement, stage by stage, through the environment itself.

You can see examples of that in the screenshots here, and more clearly in the walkthrough: [Glance at demo: **2:00**] register selection becomes visible around **2 minutes**, and [Glance at demo: **2:30**] the ALU interaction starts around **2 minutes 30 seconds**.

At the same time, I did not want realism for its own sake. If a realistic interaction added friction without improving understanding, I simplified it.

So this slide is really about the interaction philosophy behind the build, and the next one turns that into the actual step-by-step learner journey.

## Slide 8 - Instruction Walkthrough

If I condense the learner journey into one sequence, it looks like this:

The learner fetches the instruction, decodes it, identifies the required operands, retrieves register values and any immediate, makes the relevant execution decisions, performs memory or write-back behavior where needed, and finally confirms the resulting register, memory, and PC changes.

This slide mainly acts as an orientation point while the demo is running.

The timing in the walkthrough lines up quite well with the flow shown here:
- around **30 seconds**: Instruction Fetch
- around **50 seconds**: Instruction Decode begins
- around **2 minutes**: register selection
- around **2 minutes 30 seconds**: ALU phase
- around **4 minutes 30 seconds**: Write-Back
- around **5 minutes 50 seconds**: PC Update
- around **7 to 8 minutes**: Practice mode decode
- around **11 minutes**: branch-control behavior
- around **15 minutes**: memory access

So rather than reading the list on the slide, I use this to anchor the audience while the video shows what the process actually feels like in motion. The main visual checkpoints I would track here are [Glance at demo: **0:30, 0:50, 2:00, 2:30, 4:30, 5:50, 7:00-8:00, 11:00, 15:00**].

After that learner-facing view, I briefly step behind the scenes and show how the system is organized underneath.

## Slide 9 - System Architecture Flow

Behind the experience is a structured lesson architecture made up of authored lesson data, a shared lesson flow, scene-authored panels and phase stations, and a set of shared runtime support systems.

The main point of this slide is not the individual class names. It is the overall organization.

I deliberately preferred a modular, scene-authored structure over one large dynamic controller doing everything at runtime. That made the system easier to iterate on, easier to debug, and easier to keep aligned with the actual educational flow.

So, in simple terms: authored content feeds shared lesson logic, that logic coordinates the phase systems, and those phase systems drive the learner-facing interactions.

From there, I move into how the system is being evaluated in practice.

## Slide 10 - Research Study

The participant study is meant to evaluate both learning-facing and experience-facing outcomes.

In practice, the session flow is fairly structured.

Participants first read the participant information material, sign the consent form, and complete a short background section. After that, I give a tailored refresher on assembly, instruction flow, and the datapath depending on their prior background.

That refresher matters because, again, the system is meant to support teaching, not replace it.

Participants then complete a structured VR session using selected Learning and Practice instructions, with minimal intervention from me unless they become genuinely stuck. After the headset portion, they complete experience ratings, a knowledge check, and open-ended feedback, while I also keep observational notes during the session.

So the study is not just asking whether the system is interesting. It is asking whether it is usable, understandable, and educationally meaningful.

That naturally brings me to a few of the decisions that most shaped the final prototype.

## Slide 11 - Design Decisions and Scope Control

I only want to emphasize three design decisions here, because these were the ones that most strongly shaped what the project became.

First, I deliberately scoped the project as a **single-cycle MIPS learning environment**, not a full simulator. That kept the system teachable, testable, and stable.

Second, I did **not** collapse all control-signal reasoning into Instruction Decode. In principle, that might sound more architecturally tidy, but educationally it made later phases too passive. By distributing meaningful control-facing choices across Execute, Memory, and Write-Back, those later stations still remain active teaching moments.

Third, I separated **register identity** from **value flow**. In Decode, learners physically choose registers. After that, datapackets carry the values through later phases. That made it much easier to trace movement through the datapath without forcing the learner to carry full register objects through every later stage.

So these decisions were really about preserving clarity, pacing, and meaningful interaction rather than maximizing simulation fidelity.

Those same decisions also explain the current limitations of the system.

## Slide 12 - Limitation and Future Work

The clearest limitation is scope.

This prototype is intentionally centered on a **single-cycle MIPS datapath**, so it does not yet cover multicycle behavior, pipelining, stalls, or hazards. It also supports a focused instruction subset rather than the entirety of MIPS.

There is also an evaluation limitation. The current study is based on short participant sessions, so while it can tell me quite a lot about usability, clarity, and immediate learning support, it does not yet say much about longer-term retention or broader classroom deployment.

The most natural future direction would be to extend the environment into **multicycle and pipelined execution**, because those are exactly the contexts where tracing overlap, control, and timing becomes even harder for students.

Other future directions include broader control-flow coverage, clearer onboarding, stronger adaptive guidance, and larger follow-up studies with a stronger comparative evaluation structure.

So I see this project as a stable instructional baseline with room to grow, rather than as a finished endpoint.

## Slide 13 - Thank You / Questions

To conclude, this project started from a very practical teaching problem: students often struggle to mentally trace what a processor is doing from a static diagram alone.

The contribution here is an attempt to turn that guided tracing process into a structured VR learning experience that is spatial, interactive, and progressively scaffolded.

Again, the aim is not to replace traditional teaching, but to supplement it with a tool that helps learners visualize and reason through instruction execution more concretely.

Thank you very much, and I would be happy to take any questions.
