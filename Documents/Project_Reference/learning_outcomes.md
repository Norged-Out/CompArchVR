# Learning Outcomes

This file records the final learning outcomes used to scope the dissertation project.

They are intentionally tied to what the prototype **actually implements**, rather than to a broader idealized computer-architecture curriculum.

The point was never to claim that the system teaches all of computer architecture. The point was to define a focused set of outcomes around instruction execution in a **single-cycle MIPS datapath**, and then build the VR interactions directly around those outcomes.

---

## Final Learning Outcomes

By completing the VR experience, learners should be able to:

1. **Decode the relevant fields of a supported single-cycle MIPS instruction.**
   - In practice, this means identifying opcode, source registers, destination register where relevant, immediate field where relevant, and funct field for R-type instructions.
   - Project coverage:
     - Learning mode decode
     - Practice mode typed binary-field decode
     - Test mode strict decode

2. **Explain the role of the major datapath components involved in instruction execution.**
   - This includes the register file, ALU, multiplexers, sign-extension path, data memory, and PC update logic.
   - Project coverage:
     - world-space phase stations
     - lesson-panel explanations
     - repeated use of phase-specific controls and validation panels

3. **Trace an instruction and its data through the major stages of a single-cycle datapath.**
   - The learner should be able to follow the instruction through IF, ID, EX, MEM, WB, and PC update, while recognizing that not every instruction uses every stage in the same way.
   - Project coverage:
     - routed physical map layout
     - fetch flow
     - decode register scanning
     - ALU, memory, write-back, and PC update stations

4. **Select the correct operands and control-facing choices needed to execute an instruction.**
   - The learner should be able to choose the right registers, immediate handling, ALU operation, memory action, write-back target, and next-PC path for the supported instruction set.
   - Project coverage:
     - decode scanner logic
     - immediate extender
     - ALU interaction
     - memory phase validation
     - write-back validation
     - PC update validation

5. **Predict the architectural state changes caused by a supported instruction.**
   - The learner should be able to anticipate the resulting changes to registers, data memory where relevant, and the program counter / next-PC path.
   - Project coverage:
     - persistent register bank
     - memory-bank updates
     - branch and jump handling
     - end-state validation in later phases

6. **Compare how different instruction classes change datapath behavior.**
   - The learner should be able to contrast the execution flow of R-type, immediate, load/store, branch, and jump instructions inside the same single-cycle CPU model.
   - Project coverage:
     - guided Learning mode
     - reduced-support Practice mode
     - strict Test mode
     - authored instruction set spanning arithmetic, immediate, memory, branch, and jump categories

7. **Complete a supported instruction walkthrough with reduced or no guidance.**
   - This is the practical culmination of the earlier outcomes and maps to the progression from Learning mode to Practice mode to Test mode.
   - Project coverage:
     - Learning mode with explicit scaffolding
     - Practice mode with limited hints and attempts
     - Test mode with minimal support and strict failure conditions

---

## Why These Outcomes Were Chosen

These outcomes were chosen because they reflect the actual educational progression embedded in the prototype:

1. decode the instruction
2. understand what the major components do
3. trace the path of execution
4. make the required choices
5. reason about the resulting state changes
6. compare instruction categories
7. perform the full walkthrough more independently

That progression also maps directly onto the three-mode structure:

- **Learning** emphasizes explanation, prompting, and corrective guidance
- **Practice** reduces support and requires more independent decoding and decision-making
- **Test** removes most support and expects independent completion

So the learning outcomes were not written after the build as a formality. They were meant to act as scope control.

---

## Course Grounding

These outcomes were not chosen in a vacuum. They are grounded in the kinds of concepts normally taught in undergraduate computer architecture and computer organization courses.

### University of Arizona Context

During the Bachelor's degree at the **University of Arizona**, teaching-assistant work was done for **CSC 252 - Computer Organization** under **Russell Lewis**.

The official University of Arizona catalog describes CSC 252 as covering:

- basic machine organization
- elementary hardware concepts
- CPU internals
- machine operations and instructions
- assembly language concepts and programming

That is an important reference point because the project grew out of repeated teaching experience in that environment, especially around student difficulty with:

- datapath navigation
- control signals
- instruction flow
- value movement between registers, memory, and execution units

Relevant official references:

- [University of Arizona CSC 252 catalog entry](https://catalog.arizona.edu/courses/0097831)
- [University of Arizona Computer Science profile for Russell Lewis](https://cs.arizona.edu/person/russell-lewis)
- [Russell Lewis faculty page listing CSC 252 teaching history](https://www2.cs.arizona.edu/~russelll/)

### Trinity College Dublin Context

The dissertation itself sits in the context of **Trinity College Dublin**, where the closest curricular grounding is **Computer Architecture I (CSU22022)**.

The official module description highlights:

- digital logic
- register-transfer language
- ALU and shifter design
- multiplexer and tristate buses
- datapath design
- the instruction fetch-decode-execute cycle

The module learning outcomes also explicitly include:

- describing the organisation and execution behaviour of processor systems
- designing control units and datapaths

That matters because the VR prototype does **not** try to replace this course material. It instead acts as a supplementary environment that supports learners in visualizing and reasoning through the execution behavior that such courses already teach.

Relevant official references:

- [Trinity School of Computer Science and Statistics module page for CSU22022 - Computer Architecture I](https://teaching.scss.tcd.ie/module/csu22022-computer-architecture-i/)
- [Trinity module directory listing for CSU22022](https://www.tcd.ie/students/orientation/visiting-exchange/module-directory/Computer%20Science%20and%20Statistics.php)

---

## Relationship Between the Outcomes and the Real Courses

The final learning outcomes sit in the middle of two curricular realities:

- they are **narrower** than a full university course in computer architecture
- but they are **more concrete and operational** than a vague “understand the datapath” objective

In other words, they were written to match what the VR system can responsibly claim.

The prototype can help learners:

- decode supported instructions
- trace instruction and value flow
- reason about components and state changes
- compare instruction categories
- complete instruction walkthroughs with progressively less help

It cannot responsibly claim that it teaches:

- full assembly programming
- full digital logic design
- HDL-based control-unit construction
- pipelined hazard handling in depth
- the entirety of a standard architecture course

That boundary is important both pedagogically and methodologically.

---

## Scope Boundaries

These learning outcomes should **not** be stretched into claims that the prototype teaches:

- full MIPS coverage
- multicycle execution
- pipelining and hazards
- gate-level digital logic construction
- full assembly programming
- compiler-level or systems-level processor design

Those areas belong to future work, not to the present build.

---

## External References Worth Keeping in Mind

These are not all necessarily mandatory citations in the dissertation, but they are useful anchors when discussing why the final outcomes look the way they do:

- **University of Arizona CSC 252 - Computer Organization**
  - grounds the original teaching experience and motivation
- **Russell Lewis**
  - grounds the course/instructional context in which the repeated student difficulties were observed
- **Trinity College Dublin CSU22022 - Computer Architecture I**
  - grounds the dissertation in the local curricular context
- **Pirker et al. / Agbo et al. / Janani et al.**
  - useful for broader VR-in-education motivation
- **Dascalu et al. / Tlili et al. / Tan et al.**
  - useful for computer-architecture-specific grounding
- **Cook / Korkut & Surer**
  - useful for caution around VR overload, clarity, and instructional design constraints

---

## Short Version for Slides or Thesis Summary

If a short version is needed for a presentation slide, the outcomes can be collapsed to:

1. Decode the relevant fields of a supported single-cycle MIPS instruction.
2. Explain the role of the major datapath components involved in instruction execution.
3. Trace an instruction and its data through the major stages of a single-cycle datapath.
4. Select the correct operands and control-facing choices needed to execute an instruction.
5. Predict the architectural state changes caused by a supported instruction.
6. Compare how different instruction classes change datapath behavior.
7. Complete a supported instruction walkthrough with reduced or no guidance.
