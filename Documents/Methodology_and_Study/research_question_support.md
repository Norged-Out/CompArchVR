# Research Question Support

## Central Research Question

> To what extent does an immersive virtual reality-based learning environment improve students’ understanding of abstract computer architecture concepts compared to traditional teaching methods?

This file exists to explain how that question can still be defended properly, even though the final study is not a large formal controlled experiment.

---

## 1. Why This Research Question Still Makes Sense

The question is broad, but it remains valid because the project does genuinely sit at the intersection of:

- immersive educational technology
- computer architecture instruction
- abstract concept visualization
- supplementary learning support

The project was never just about building “a VR thing.”
It was built around a very specific educational frustration:

- students often struggle to understand how information actually flows through a datapath
- static diagrams explain structure, but they do not always make dynamic execution intuitive
- learners frequently confuse registers, memory, control signals, and phase ordering

The prototype directly targets those problems.

This frustration was not hypothetical. It was repeatedly observed during teaching-assistant work in **CSC 252 - Computer Organization** at the **University of Arizona**, in the orbit of **Russell Lewis**. That experience is what turned the problem from a vague interest into a concrete research direction.

The question also remains locally relevant in the **Trinity College Dublin** context, especially around **CSU22022 - Computer Architecture I**, where learners are similarly expected to reason about datapath structure, control, fetch-decode-execute behavior, and state change.

---

## 2. What “To What Extent” Should Mean Here

The phrase “to what extent” should **not** be interpreted as requiring a perfect classroom-vs-VR experimental control setup.

In this dissertation, it is more realistic to interpret it as:

- to what extent participants report that the VR system helped
- to what extent participants can demonstrate understanding after using it
- to what extent the prototype appears to support learning goals that traditional materials often struggle to communicate clearly
- to what extent the prototype shows enough educational promise to justify future expansion and larger evaluation

This is a softer but still legitimate interpretation.

---

## 3. The Actual Gap the Project Addresses

The literature reviewed for the project points to three converging observations:

### 3.1 VR is often promising for abstract or invisible processes

Many papers in VR and computer science education argue that immersive environments can make hidden processes more concrete by turning them into spatial and interactive experiences.

### 3.2 Computer architecture remains difficult for learners

Architecture education often involves:

- abstract flow
- invisible state changes
- dense symbolic notation
- multiple parallel hardware concepts

These are exactly the kinds of ideas that can be hard to absorb from static diagrams alone.

### 3.3 There are still relatively few focused tools for instruction execution walkthrough

There is broader work on:

- serious games
- educational VR
- visualization systems

But there is less specifically focused work on placing the learner inside a structured environment where they manually trace and complete instruction execution through a single-cycle datapath.

That is the niche this project occupies.

---

## 4. How the Research Question Connects to the Prototype

The question is supported because the prototype directly operationalizes a set of design responses to the learning problem.

It does this by:

- turning phase progression into a spatial route
- turning operand selection into physical interaction
- making data flow visible through datapackets and stations
- splitting support across Learning, Practice, and Test modes
- guiding the learner from explicit scaffolding toward more independent completion

So the study is not vaguely asking whether VR is good.
It is asking whether **this specific kind of structured, phase-based VR interaction** appears to improve understanding of selected architecture concepts.

---

## 5. What Evidence Supports the Question

The question can be answered through multiple evidence channels.

### 5.1 Background + Baseline

Section A and the short baseline probe establish:

- prior familiarity
- self-reported confidence
- starting architecture knowledge on selected concepts

### 5.2 In-Session Performance

The three-mode structure produces behavioral evidence:

- how much guidance the participant needed
- whether they could move from guided to reduced-support completion
- where they got stuck
- which concepts needed intervention

### 5.3 Post-Session Questionnaire

The Likert sections provide direct structured feedback on:

- usability
- engagement
- clarity
- confidence
- perceived learning

### 5.4 Knowledge Check

The knowledge check provides post-session conceptual evidence tied to the project’s learning scope.

### 5.5 Open Feedback + Researcher Notes

These allow the dissertation to capture:

- what participants believed helped them
- what confused them
- how they compared the experience to diagrams, lectures, or other materials

---

## 6. How to Avoid Overclaiming

The research question is useful, but dangerous if interpreted too aggressively.

The dissertation should **not** pretend that the current study proves:

- long-term retention gains
- causal superiority over lectures
- general superiority of VR for all architecture teaching
- statistical generalizability

Instead, the question should be answered in a bounded way:

> within this small-scale exploratory evaluation, participants’ responses, observed interaction patterns, and post-session performance can be used to examine whether the prototype appears to improve understanding and whether it is perceived as a useful supplement to traditional teaching methods.

That answer is more honest and more defensible.

---

## 7. Why the Question is Still Worth Asking

Even if the final answer is cautious, the question remains valuable because it ties together:

- a real educational problem
- a deliberately designed prototype
- a grounded participant study
- a future pathway for broader evaluation

That means the dissertation can still make a useful contribution by showing:

1. how architecture-learning difficulty was translated into VR interaction design
2. how a structured educational VR prototype can be implemented around learning outcomes
3. what early participant evidence suggests about its educational value

---

## 8. Recommended Thesis Positioning

The cleanest way to frame the question in the dissertation is:

> The study investigates the extent to which a structured VR learning environment can support understanding of selected single-cycle MIPS datapath concepts, and whether participants perceive it as a valuable supplement to traditional teaching materials.

This keeps:

- the original spirit of the question
- the comparative angle
- the supplementary-learning emphasis

while reducing the risk of sounding like a full experimental claim that the study cannot realistically support.
