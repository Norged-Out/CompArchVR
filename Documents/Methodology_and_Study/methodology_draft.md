# Methodology Draft

## 1. Methodological Framing

This dissertation should be framed as a **small-scale exploratory evaluation** of a VR learning prototype for computer architecture, rather than as a large controlled experiment intended to prove definitive superiority over traditional teaching.

That distinction matters.

The project already has:

- a fully implemented prototype
- a live participant study already underway
- a mixed set of data sources
- a small and practical participant pool

What it does **not** have is:

- a formally balanced control group
- enough participants for strong inferential statistics
- a fully standardized teaching intervention comparable to a classroom module
- long-term retention measurement

So the cleanest and safest methodological claim is:

> the study evaluates whether the prototype appears educationally useful, usable, and promising as a supplemental learning tool, while also investigating how participants respond to it conceptually and experientially.

That is much easier to defend than pretending the project is a strict controlled comparison trial.

---

## 1.5 Pedagogical Origin and Curricular Grounding

The methodology also needs to acknowledge where the project came from.

The original motivation did not come from abstract interest in VR alone. It came from repeated teaching-assistant experience during the Bachelor's degree at the **University of Arizona**, specifically around **CSC 252 - Computer Organization**, taught in the orbit of **Russell Lewis**.

Across repeated semesters, the same kinds of student difficulties kept recurring:

- following the datapath from one stage to another
- understanding what control signals were actually doing
- separating registers, memory contents, and addresses
- reasoning about what changes state and when

The most effective support during office hours was often not more text, but drawing over the datapath and walking through execution step by step.

That teaching experience is important because it grounds the prototype in a real instructional problem rather than in a purely technological novelty pitch.

The dissertation is also situated in the current curricular context of **Trinity College Dublin**, especially the learning space around **CSU22022 - Computer Architecture I**, where datapath structure, control, ALU behavior, and the fetch-decode-execute cycle are part of the normal teaching pathway.

So methodologically, the study is not asking whether VR can replace architecture teaching. It is asking whether this kind of VR environment can act as a useful **supplementary intervention** for a known teaching difficulty inside a recognizable computer-architecture curriculum.

---

## 2. Recommended Overall Label

The study can still be described as a **mixed-methods exploratory study**.

That wording fits well because the project combines:

- structured questionnaire data
- a short knowledge check
- live researcher observation notes
- open-ended participant feedback

The mixed-methods label should not be oversold.
It does **not** mean the study is statistically heavy.
It simply means that both quantitative and qualitative evidence are being used together.

---

## 3. Core Aim of the Evaluation

The methodological aim is not merely to ask whether participants enjoyed VR.

The stronger aim is:

1. to examine whether the prototype supports understanding of selected single-cycle MIPS datapath concepts
2. to observe whether participants can move from guided completion toward more independent completion
3. to capture how participants perceive the prototype in relation to more traditional learning materials
4. to identify which parts of the prototype appear educationally helpful, confusing, or in need of refinement

---

## 4. Recommended Research Position

The dissertation should position the study as evaluating a **supplemental educational intervention**.

That means the prototype is not presented as:

- a replacement for lectures
- a replacement for textbooks
- a replacement for whiteboard explanation
- a replacement for programming-based architecture assignments

Instead, it is presented as a tool intended to sit **between abstract diagram-based teaching and deeper formal implementation work**.

This aligns well with the real origin of the project:

- repeated student confusion around datapath navigation
- difficulty connecting static diagrams to dynamic information flow
- the usefulness of step-by-step visual explanation during office hours

---

## 5. Participant Flow Actually Used

The current procedure is already close to a defendable workflow.

The session can be described as having five parts:

### Part A. Consent and Background Screening

Participants:

- read the Participant Information Leaflet
- sign the consent form
- complete the background and screening items from Section A

This establishes:

- prior architecture exposure
- prior VR familiarity
- pre-session confidence

### Part B. Short Tailored Refresher

Before entering VR, the participant receives a short verbal refresher tailored to their background.

This refresher does not try to fully teach the content in advance.
Instead, it exists to normalize baseline understanding so the VR session is not derailed by missing prerequisite vocabulary.

Typical refresher coverage includes:

- basic assembly structure
- what the datapath is meant to represent
- high-level purpose of IF, ID, EX, MEM, WB, and PC update
- how registers, memory, and immediate values differ

This refresher is important to defend, not hide.
It reflects the project’s stated goal as a **supplementary** learning tool rather than a standalone replacement for formal instruction.

### Part C. Pre-VR Baseline Probe

A short baseline probe can be integrated before VR using a small subset of already approved question-bank items.

This is not a new instrument.
It is a targeted reuse of approved questions to establish whether the participant could already answer a few key architecture questions before interacting with the prototype.

### Part D. VR Session

Participants then complete a structured VR session using selected instructions and modes.

The researcher may:

- choose 1-2 instructions for Learning mode
- choose 1 instruction for Practice mode
- optionally include Test mode depending on time and participant comfort

During the session, the researcher keeps observational notes on:

- where help was needed
- which concepts caused confusion
- whether the participant improved in independence
- whether interaction design appeared helpful or obstructive

### Part E. Post-Session Evaluation

After VR, participants complete the post-session form:

- Likert sections on usability, engagement, perceived learning, and related dimensions
- a knowledge-check section
- open-ended feedback

This produces the primary post-session evidence.

---

## 6. What the Study is Actually Comparing

This is the most important methodological clarification.

The phrase “to what extent” in the research question sounds strongly comparative, but the project does not currently operate like a classical A/B experiment with matched groups.

So comparison should be operationalized in a more realistic and defensible way.

The study can compare:

### 6.1 Baseline vs. Post-Session Conceptual Responses

Using the short approved baseline subset and the post-session knowledge check, the dissertation can discuss whether participants appear to improve in their ability to answer selected architecture questions.

This is the closest thing the study has to a before/after conceptual comparison.

### 6.2 Participant Perception of VR vs. Traditional Learning Materials

The questionnaire already includes items that let participants compare the VR experience against prior learning materials or conventional teaching support.

That gives an **indirect comparative angle** without requiring a fully separate control condition.

### 6.3 Guided vs. Reduced-Guidance Performance Within the Same System

The three-mode structure also creates a useful internal comparison:

- Learning mode provides guidance
- Practice mode reduces guidance
- Test mode removes most support

This lets the dissertation discuss whether participants can progress from supported to more independent completion within the same environment.

That is not the same as lecture-vs-VR comparison, but it is still a meaningful comparative structure.

---

## 7. Recommended Claim Strength

The methodology should support **measured claims**, not maximal claims.

Good claims:

- the prototype appears promising as a supplemental learning tool
- participants generally responded positively to the visual and spatial structure
- the environment supports tracing and reasoning about instruction execution
- the progressive mode structure provides a plausible path from guided learning to independent practice

Risky claims:

- VR definitively outperforms traditional teaching
- the prototype causes long-term learning gains
- the system is broadly generalizable to all architecture learners
- the study proves effectiveness in a statistical causal sense

---

## 8. Why This Method Still Works

Even without a formal control group, the methodology remains valuable because it captures several things at once:

- immediate user experience
- conceptual performance after use
- observed learner difficulty points
- perceived comparative value
- design implications for future educational VR systems

That combination is enough for a strong dissertation if the framing stays honest.

The project does not need to claim final proof.
It needs to claim that the prototype was designed carefully, evaluated seriously, and produced useful evidence about educational VR for computer architecture.

---

## 9. Suggested Thesis Framing Sentence

If a single sentence is needed for the dissertation:

> This study adopts a mixed-methods exploratory methodology to evaluate a VR-based supplementary learning environment for single-cycle MIPS instruction execution, combining background screening, a short approved baseline probe, structured VR task completion, post-session questionnaire responses, knowledge-check performance, researcher observation notes, and open-ended participant feedback.

---

## 10. Practical Bottom Line

The methodology should now be treated as:

- exploratory
- mixed-methods
- supplementary-learning focused
- small-scale
- comparative in a limited and carefully defined sense

That is the safest and most defensible version of the study.
