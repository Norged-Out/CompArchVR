# Single-Cycle MIPS Datapath vs. VR Prototype

This document is a long-form reference note for the dissertation stage.

It exists for four reasons:

1. to explain how a **real textbook single-cycle MIPS datapath** works
2. to map that behavior onto the **actual VR prototype implementation**
3. to identify where the prototype is **hardware-faithful**, where it is **pedagogically abstracted**, and where it is **deliberately simplified**
4. to give later thesis chapters a strong backbone when discussing design choices, educational scope, limitations, and evaluation

This is intentionally more detailed than a presentation note or journal summary.
At this point, too much clarity is better than too little.

---

## 1. The Most Important Framing Up Front

The prototype is **not** a literal one-to-one hardware simulator of a classic single-cycle MIPS processor.

It is more accurately described as:

> a pedagogical VR learning environment built around the structure and reasoning of a single-cycle MIPS datapath, where hidden or parallel hardware processes are externalized into visible, spatial, learner-facing interactions.

That sentence matters, because many later questions reduce to it.

If somebody asks:
- "Is this exactly how a real single-cycle MIPS CPU behaves?"
- "Why does the learner carry an instruction physically?"
- "Why are muxes not shown directly?"
- "Why are control signals selected across later phases instead of fully resolved in decode?"

the answer is usually some variation of:

- the prototype preserves **reasoning structure**
- but it does not preserve **literal timing or internal invisibility**

This means the project should be defended as:
- **datapath-inspired**
- **instruction-execution-focused**
- **educationally structured**

rather than as:
- a cycle-accurate simulator
- a gate-level digital logic tutor
- or a complete MIPS architecture emulator

---

## 2. What A Real Single-Cycle MIPS Datapath Actually Does

Before comparing anything to the prototype, it is important to clarify how the textbook machine itself works.

### 2.1 Single-cycle does not mean "five real stages"

In teaching, people often speak about:
- Instruction Fetch (`IF`)
- Instruction Decode (`ID`)
- Execute (`EX`)
- Memory (`MEM`)
- Write-Back (`WB`)

These labels are useful, but in a **single-cycle** machine they are **not separate clocked steps**.

Instead:
- one full instruction completes in one clock cycle
- all needed combinational work happens between one clock edge and the next
- the only architectural state that changes at the end of the cycle is written back on that clock edge

So in a real single-cycle MIPS datapath:
- fetch is not one cycle
- decode is not a second cycle
- execute is not a third cycle
- memory is not a fourth cycle
- write-back is not a fifth cycle

They are better understood as **conceptual regions of activity** within one long combinational path.

This is one of the biggest places where a teaching prototype can easily drift from strict hardware truth.

### 2.2 What counts as state in the real machine

In the classic textbook single-cycle datapath, persistent architectural state mainly lives in:

- `PC`
- `Register File`
- `Data Memory`

Everything else is primarily combinational logic, including:

- Instruction Memory output
- Control Unit
- ALU Control
- ALU
- adders
- sign extender
- shift-left-2 unit
- multiplexers
- branch decision logic

This means the machine is mostly:
- reading state
- pushing values through logic
- then committing a new architectural state at the clock edge

### 2.3 The starting point of a real instruction

At the beginning of a cycle:
- the `PC` already contains the address of the current instruction
- that address goes into **Instruction Memory**
- Instruction Memory outputs the instruction bits
- an adder also computes `PC + 4`

Already, this tells us something important:
- in real hardware, the system does not ask "what should I do first?"
- the hardware wiring already begins doing the right thing automatically

That is one of the main differences between the real datapath and a learner-facing VR environment.

---

## 3. The Real Hardware Components, Properly Explained

This section explains the real textbook components before mapping them to the prototype.

### 3.1 Program Counter (PC)

The `PC` stores the address of the current instruction.

Its role is fundamental:
- it determines which instruction the processor is currently executing
- it participates in computing the next instruction address

In a single-cycle datapath:
- the `PC` value goes into Instruction Memory
- `PC + 4` is computed immediately
- branch/jump logic may later override the default next PC
- the selected next PC is committed at the clock edge

### 3.2 Instruction Memory

Instruction Memory stores the program's instructions.

Its behavior is conceptually simple:
- input: address from `PC`
- output: 32-bit instruction at that address

In the classic diagram, Instruction Memory is usually treated as a read-only source for the current instruction.

### 3.3 Instruction Register

This is where confusion often appears, especially when people move between single-cycle, multicycle, and pipelined explanations.

In many broader CPU discussions, an **Instruction Register (IR)** is a register that stores the fetched instruction temporarily.

However:

> in the classic textbook single-cycle MIPS datapath, there is usually no separate Instruction Register

Why not?

Because:
- the instruction is fetched and fully used within the same cycle
- there is no need to hold it across later cycles

Its bits are simply used directly by the logic that needs them.

This point matters because if the questionnaire, lecture material, or examiner expects the concept of an Instruction Register, that concept may still be valid broadly, but the textbook single-cycle datapath itself does not usually isolate it as a separate state element.

### 3.4 Register File

The Register File stores the general-purpose registers.

It typically provides:
- two read ports
- one write port

Fields in the instruction determine:
- which registers are read
- which register is written back later

For example:
- `rs` selects read register 1
- `rt` selects read register 2
- destination may be `rt` or `rd`, depending on instruction type

### 3.5 Main Control Unit

The main control unit reads the opcode and generates coarse control signals such as:

- `RegDst`
- `ALUSrc`
- `MemtoReg`
- `RegWrite`
- `MemRead`
- `MemWrite`
- `Branch`
- `Jump`
- `ALUOp`

This is one of the most important features of the textbook datapath:
- decode is not just “understand the instruction”
- decode is also where the control structure decides how the rest of the datapath should behave

### 3.6 ALU Control

The ALU Control block refines the action of the ALU.

For some instructions, opcode alone is not enough.
R-type instructions all share the same broad opcode class, so the machine must also inspect `funct` to decide the final ALU action.

That is why the main control unit often outputs a smaller code like `ALUOp`, and the ALU Control block uses:
- `ALUOp`
- `funct`

to choose:
- add
- subtract
- and
- or
- set-less-than

### 3.7 ALU

The ALU performs several kinds of work:

- arithmetic
- logic
- comparison
- address calculation

This means the ALU is not “just for add/sub.”
It is reused across instruction classes:
- R-type arithmetic and logic
- immediate arithmetic
- effective address generation for load/store
- comparison for branch decisions

### 3.8 Sign Extender

The sign extender takes a 16-bit immediate and produces a 32-bit version, usually preserving signed meaning.

This is important for:
- `addi`
- `lw`
- `sw`
- `beq`
- `bne`

### 3.9 Shift Left 2

This unit is typically used for:
- branch target formation
- jump target formation

because word-aligned addresses are represented using shifted instruction bits.

### 3.10 Data Memory

Data Memory is used by:
- `lw`
- `sw`

and ignored by instructions that do not access memory.

### 3.11 Multiplexers

Muxes are one of the most structurally important but visually underappreciated parts of the datapath.

They choose between alternative paths such as:
- destination register = `rt` or `rd`
- ALU input 2 = register value or immediate
- write-back source = ALU result or memory data
- next PC = `PC + 4`, branch target, or jump target

In real hardware:
- learners do not “operate” muxes directly
- muxes are driven by control signals

### 3.12 Branch and Jump Logic

For branch:
- the ALU compares values, often by subtraction
- `Zero` plus `Branch` determines whether the branch is taken
- branch target uses `PC + 4 + (sign-extended immediate << 2)`

For jump:
- target comes from instruction bits and upper bits of `PC + 4`

### 3.13 Write-back

At the end of the cycle:
- a chosen value may be written into a chosen register
- or no register write occurs

That result may come from:
- the ALU
- or Data Memory

---

## 4. A Full Real Single-Cycle Walkthrough By Instruction Type

This section is useful because it shows how the real machine behaves across different instruction categories.

### 4.1 R-type example: `add rd, rs, rt`

In the real datapath:

1. `PC` addresses Instruction Memory
2. instruction bits come out
3. opcode indicates R-type
4. Register File reads `rs` and `rt`
5. control decides:
   - destination will be `rd`
   - ALU input 2 comes from register, not immediate
   - result comes from ALU, not memory
   - register write will happen
6. ALU control reads `funct`
7. ALU performs addition
8. no memory access is used
9. ALU result is written to `rd`
10. `PC` becomes `PC + 4`

### 4.2 Immediate example: `addi rt, rs, imm`

1. instruction is fetched
2. opcode indicates immediate arithmetic
3. Register File reads `rs`
4. immediate is sign-extended
5. control selects immediate as ALU input 2
6. ALU adds register + immediate
7. result is written to `rt`
8. `PC` becomes `PC + 4`

### 4.3 Load example: `lw rt, offset(rs)`

1. instruction is fetched
2. Register File reads base register `rs`
3. immediate is sign-extended
4. ALU computes effective address = `rs + offset`
5. Data Memory is read at that address
6. memory data is written into `rt`
7. `PC` becomes `PC + 4`

### 4.4 Store example: `sw rt, offset(rs)`

1. instruction is fetched
2. Register File reads:
   - base register `rs`
   - data register `rt`
3. immediate is sign-extended
4. ALU computes effective address
5. Data Memory writes the value from `rt` to that address
6. no register write-back happens
7. `PC` becomes `PC + 4`

### 4.5 Branch example: `beq rs, rt, imm`

1. instruction is fetched
2. Register File reads `rs` and `rt`
3. immediate is sign-extended
4. branch target is computed from `PC + 4 + (imm << 2)`
5. ALU compares `rs` and `rt`
6. if comparison indicates equality, branch logic chooses branch target
7. otherwise next PC remains `PC + 4`
8. no register write-back

### 4.6 Jump example: `j target`

1. instruction is fetched
2. target field is extracted
3. jump target is formed
4. next PC becomes jump target
5. no register read or write is central to the computation in the usual datapath view

---

## 5. What The VR Prototype Is Really Doing Instead

Now we can compare that real behavior against the actual implementation in `ThePrototype`.

The prototype is built around a scene-authored lesson flow with controllers such as:

- `CpuLessonFlow`
- `LessonGuideController`
- `FetchFlow`
- `DecodeFlow`
- `IntroPanelController`
- `DecodePanelController`
- `AluController`
- `MemoryController`
- `WriteBackController`
- `PcUpdateController`

The project also uses authored lesson objects such as:

- `InstructionTerminal`
- `InstructionModule`
- `RegisterScanner`
- `ImmediateExtender`
- datapacket systems

This means the project re-expresses internal datapath behavior as:
- world-space stations
- prop movement
- physical validation
- UI-supported explanation
- explicit learner decisions

---

## 6. Component-By-Component Mapping: Real Hardware vs. Prototype

This is the main comparison section.

## 6.1 Program Counter (PC)

### In real single-cycle MIPS

The `PC` is the starting address source for the instruction and one of the final update targets of the cycle.

It is active immediately.
No learner or external controller has to “choose” to use it.

### In the prototype

The `PC` exists conceptually from the beginning, but it is not the first explicit learner action in fetch.

Instead:
- the learner starts from the intro flow
- chooses or receives an instruction
- sees fetch embodied through terminal upload/download
- later revisits PC behavior at the `PC Update` station

This is implemented mainly through:
- `IntroPanelController`
- `FetchFlow`
- `PcUpdateController`
- `PcBranchService`

### What is preserved

- `PC` still matters for instruction sequencing
- branch and jump eventually still affect the next instruction path
- `PC + 4` and next-PC choice are not discarded conceptually

### What is changed

- real `PC` logic is automatic and parallel
- prototype `PC` logic is delayed into a visible, explicit teaching phase

### Why this change exists

The project separates:
- getting an instruction into the learner’s hands
from
- reasoning about how the next instruction address changes

That is more teachable spatially, even though it is less hardware-faithful.

---

## 6.2 Instruction Memory

### In real single-cycle MIPS

Instruction Memory passively responds to the `PC`.
It is addressed storage, not a physical courier process.

### In the prototype

Instruction source is represented by authored data and terminal behavior:

- `InstructionDefinition`
- `PracticeInstructionDefinition`
- `InstructionCatalog`
- `CpuLessonFlow`
- `InstructionTerminal`

Instead of addressing a visible instruction-memory block with the `PC`, the learner:
- selects an authored instruction in Learning/Practice
- or receives a random one in Test
- then interacts with a fetch terminal that materializes the instruction as a module

### What is preserved

- one active instruction is still sourced into the lesson flow
- the learner still recognizes that execution begins with making that instruction available

### What is changed

- real memory addressing is replaced by authored instruction selection plus upload/download interaction

### Best interpretation

Instruction Memory is not directly simulated as a learner-facing component.
Its role is pedagogically represented through:
- authored instruction banks
- instruction selection
- and the terminal/module fetch metaphor

---

## 6.3 Instruction Register (IR)

### In real single-cycle MIPS

Again, the strict hardware point matters:

- the classic textbook single-cycle datapath generally does **not** isolate a separate Instruction Register
- the fetched instruction is used directly in the same cycle

### In the prototype

The `InstructionModule` is not a literal IR.
It is closer to a pedagogical object that stands in for:

- the instruction having been fetched
- the instruction now being available to later logic
- the transition from fetch space to decode space

Key scripts:
- `InstructionModule`
- `InstructionTerminal`

### What is preserved

- the learner is made aware that decode does not happen "from nowhere"
- the instruction becomes a concrete active object in the lesson

### What is changed

- there is now a portable instruction object
- there is a visible handoff
- the process is staged and serialized

### Why this matters for questionnaire interpretation

Because the questionnaire includes Instruction Register language, the safest written framing later is:

> The build does not implement a strict textbook single-cycle instruction register as a separate state-holding hardware element. Instead, the instruction module acts as a learner-facing metaphor for fetched-instruction availability and transfer between the authored fetch and decode spaces.

That keeps you honest while still accounting for the concept.

---

## 6.4 Register File

### In real single-cycle MIPS

The register file reads operands automatically based on instruction fields.
No one "goes and gets" the registers physically.

### In the prototype

Register access is one of the most physically embodied parts of the project.

Implemented through:
- `RegisterBank`
- `RegisterToken`
- `RegisterScanner`
- `RegisterScannerZone`
- decode scanner behavior
- later write-back scanner behavior

The learner must:
- identify which source registers matter
- physically collect them
- place them in the proper scanner zones

### What is preserved

- source-register identity matters
- different instruction fields map to different operand roles
- register state matters to later execution

### What is changed

- automatic operand read becomes embodied operand collection
- register-file access becomes a teaching activity rather than an invisible hardware event

### Why this design is defensible

This choice exposes something students often struggle with:
- not just what the fields are,
- but how those fields correspond to actual values used later in execution

The register bank therefore becomes both:
- a simplified architectural model
- and a manipulable scene-level teaching surface

---

## 6.5 Decode Fields

### In real single-cycle MIPS

Decode is automatic:
- opcode informs control
- fields route register reads
- funct may refine ALU control
- immediate is extracted

### In the prototype

Decode becomes a substantial learner-facing phase with mode-specific behavior.

Key scripts:
- `DecodeFlow`
- `DecodePanelController`
- `LearnDecodeView`
- `PracticeDecodeView`
- `PracticeDecodeFlow`
- `DecodeHintBuilder`
- `DecodeTextBuilder`

### Learning mode

The learner is guided through:
- opcode classification
- funct confirmation where needed
- source-register preparation
- immediate preparation where relevant

### Practice mode

The learner is shown:
- encoded instruction display
- full binary representation

and must:
- manually decode the relevant fields
- type the required bit patterns
- pass staged validation

### Test mode

The learner gets:
- no lesson panel
- no hint panel
- one-attempt style strictness

### What is preserved

- field meaning still matters
- instruction formats still matter
- decoding is still the conceptual moment where an instruction becomes actionable

### What is changed

- decode is no longer a hidden logic process
- it becomes a visible educational challenge

---

## 6.6 Main Control Unit

### In real single-cycle MIPS

The main control unit centralizes opcode-driven control generation.

This is critical to the textbook machine.

### In the prototype

There is no single learner-facing “control unit object” that silently resolves the rest of the datapath.

Instead, control-facing choices are distributed across:
- decode
- execute
- memory
- write-back
- PC update

### What is preserved

- control still matters
- instruction class still governs later behavior

### What is changed

- centralized internal control generation becomes distributed learner interaction

### Why this choice was made

This is one of the most important deliberate design decisions in the whole project.

If every relevant signal were simply “decided in decode and then done automatically,” then:
- later phases would lose much of their educational value
- EX, MEM, and WB could collapse into passive observation rather than active reasoning

So the prototype intentionally gives those later phases meaningful work.

This is not hardware-faithful.
It is pedagogy-first.

---

## 6.7 Multiplexers

### In real single-cycle MIPS

Muxes quietly choose between multiple paths.

They are critical to:
- register destination selection
- ALU operand selection
- write-back source selection
- next-PC selection

### In the prototype

Muxes are effectively removed as direct learner-facing entities.

Their functional role is absorbed into:
- learner choices
- control signal choices
- validation steps

Examples:
- `ALUSrc`-style behavior becomes choosing the correct execute input interpretation
- `MemtoReg`-style behavior becomes choosing the correct write-back value source
- next-PC mux behavior becomes branch/jump path reasoning in `PcUpdateController`

### What is preserved

- path choice still matters
- execution still depends on selecting the correct alternative

### What is changed

- muxes are no longer distinct hardware teaching objects
- they are replaced by intrinsic learner decisions

### Best thesis phrasing

> In the prototype, multiplexers are not modeled as separate learner-facing hardware components. Their role is pedagogically externalized into explicit learner decisions and control-facing selections at the authored phase stations.

That is probably the cleanest wording you can reuse later.

---

## 6.8 Sign Extension

### In real single-cycle MIPS

Sign extension is silent logic.
It just happens when needed.

### In the prototype

Sign extension is turned into a dedicated scene interaction:
- `ImmediateExtender`
- `ImmediateExtenderZone`

### What is preserved

- immediates must still be interpreted correctly
- sign extension remains instruction-dependent

### What is changed

- invisible logic becomes explicit physical action

### Why this is one of the best abstractions

Unlike some other transformations, this one adds strong educational value because sign extension is:
- easy to overlook
- important for understanding address arithmetic and immediates
- normally invisible in the real diagram

So the prototype makes it deliberately visible.

---

## 6.9 ALU Control

### In real single-cycle MIPS

ALU Control refines broad decode information into the specific ALU operation.

### In the prototype

It is not taught as a gate-level microcircuit.

Instead, the learner handles the consequence of ALU control through:
- operation choice
- phase-specific validation

Key scripts:
- `AluController`
- `AluExecutionService`
- `AluPracticeFlow`

### What is preserved

- R-type instructions need more specific interpretation than just “this is arithmetic”
- operation type still matters deeply

### What is changed

- internal ALU-control circuitry is not part of the learner task
- its practical outcome is

This is where the project clearly departs from a bottom-up digital design course and focuses instead on datapath-level understanding.

---

## 6.10 ALU

### In real single-cycle MIPS

The ALU performs:
- arithmetic
- logic
- comparison
- address generation

### In the prototype

The ALU becomes a dedicated world-space station:
- input scanners
- operation selection
- result creation

Key scripts:
- `AluController`
- `AluInputScanner`
- `AluInputScannerZone`
- `DataPacketToken`

### What is preserved

- different instruction classes use the ALU differently
- operand correctness still matters
- operation correctness still matters

### What is changed

- internal combinational operation becomes explicit validated interaction

---

## 6.11 Datapackets and Internal Buses

### In real single-cycle MIPS

Values travel along wires and buses.
There is no object called a datapacket.

### In the prototype

Datapackets physically embody values moving through the machine.

They are used across:
- ALU
- memory
- write-back
- PC update support flows

### What is preserved

- values still originate somewhere and matter later
- later stages still consume earlier outputs

### What is changed

- buses become graspable tokens
- value propagation becomes physical transport

### Why this matters

This is one of the prototype’s strongest metaphors.
It makes “value flow” a visible thing rather than a line on a diagram.

---

## 6.12 Data Memory

### In real single-cycle MIPS

Memory is used for `lw` and `sw` only.

### In the prototype

Memory is represented as:
- a physical station
- visible addresses
- visible values
- explicit load/store interaction

Key scripts:
- `MemoryController`
- `DataMemoryBank`
- `DataMemoryStore`
- `MemoryWord`

### What is preserved

- effective address matters
- memory data differs from register data
- load and store are distinct

### What is changed

- silent memory use becomes explicit learner-facing interaction

This is one of the areas where the prototype is especially good for addressing common confusion between:
- address
- stored value
- register contents

---

## 6.13 Write-Back

### In real single-cycle MIPS

Write-back is simply where the architectural register update occurs if needed.

### In the prototype

Write-back becomes a full station:
- learner confirms destination register
- learner confirms source value
- register state updates visibly

Key scripts:
- `WriteBackController`
- `WriteBackPacketScanner`
- `WriteBackRegisterScanner`

### What is preserved

- not every instruction writes back
- when write-back happens, destination and data source matter

### What is changed

- the end-of-cycle update becomes a dedicated interactive teaching moment

---

## 6.14 Branch Logic

### In real single-cycle MIPS

Branch decision happens as part of the same overall cycle:
- compare operands
- evaluate condition
- form target
- choose next PC

### In the prototype

Branch behavior is made explicit and delayed into the final control-flow-oriented station.

Key scripts:
- `PcUpdateController`
- `PcBranchService`

### What is preserved

- branch depends on both condition and target computation
- branch changes next-PC path

### What is changed

- real simultaneous behavior becomes explicit later-phase reasoning

---

## 6.15 Jump Logic

### In real single-cycle MIPS

Jump target formation is part of next-PC logic.

### In the prototype

Jump is currently supported in limited learning-mode form, mainly as a guided control-flow case.

### What is preserved

- jump is distinct from sequential flow
- jump changes control path rather than ordinary data result

### What is changed

- the full low-level target-formation detail is not surfaced with full hardware fidelity

---

## 7. Phase-By-Phase Mapping: Real Machine vs. Lesson Flow

This section maps the six lesson-facing phases against the real machine.

## 7.1 Instruction Fetch

### Real machine

Fetch means:
- PC addresses instruction memory
- instruction emerges
- PC+4 is computed in parallel

### Prototype

Fetch means:
- active instruction is chosen or assigned
- fetch terminal uploads the module
- learner carries module to decode terminal

### Meaning

This is not literal fetch.
It is a **visible instruction-acquisition and transition metaphor**.

---

## 7.2 Instruction Decode

### Real machine

Decode includes:
- field interpretation
- control generation
- register read
- immediate preparation

### Prototype

Decode includes:
- field learning or validation
- source-register identification
- register scanning
- immediate generation and sign extension

### Meaning

Decode remains the intellectual entry point into the datapath, but it becomes far slower and more learner-facing than in hardware.

---

## 7.3 Execute

### Real machine

ALU performs the necessary operation during the same instruction cycle.

### Prototype

EX is a station where the learner:
- places the right inputs
- chooses the right control-facing behavior
- triggers result generation

### Meaning

This preserves function, but not timing.

---

## 7.4 Memory Access

### Real machine

Load/store access memory if and only if the instruction demands it.

### Prototype

MEM is a full interactive zone where that access is validated and observed.

### Meaning

This is a pedagogical expansion of the memory portion of the real datapath.

---

## 7.5 Write-Back

### Real machine

Register update happens if enabled.

### Prototype

WB becomes a clear learner-facing checkpoint.

### Meaning

This makes the final architectural change more explicit than the hardware naturally does.

---

## 7.6 PC Update

### Real machine

This is not a separate later step.
It is part of the same cycle’s control-flow resolution.

### Prototype

It is a separate authored teaching phase.

### Meaning

This is one of the prototype’s clearest educational serializations of parallel hardware behavior.

---

## 8. What The Prototype Preserves Well

The prototype preserves a strong amount of meaningful computer architecture reasoning:

- instruction classes matter
- fields matter
- operands matter
- values come from specific places
- sign extension matters
- ALU decisions matter
- memory access differs from arithmetic
- write-back is selective
- control-flow instructions change next-PC behavior
- architectural state changes are treated as important outcomes

This is why the prototype should not be dismissed as a shallow “VR gimmick.”
It genuinely preserves the logic of instruction execution, even where it changes the way the learner encounters that logic.

---

## 9. What The Prototype Deliberately Simplifies

The main deliberate simplifications are:

- parallel behavior is serialized
- control generation is distributed instead of centralized
- muxes are absorbed into decisions
- datapackets replace buses
- instruction transfer is spatialized
- sign extension becomes explicit
- gate-level logic is hidden
- write-back and PC update are expanded into dedicated pedagogical phases

These changes were mostly made in response to one educational concern:

> if the system remained too faithful to invisible hardware behavior, it would risk preserving realism while failing as a teaching tool

That is the real trade-off at the heart of the project.

---

## 10. Likely Criticisms And The Best Interpretations

### Criticism: “This is not really how a single-cycle MIPS CPU works.”

Best response:

That is true at the level of literal timing and internal invisibility.
The prototype is not meant to be cycle-accurate hardware. It is a structured educational abstraction built around the logic and reasoning of single-cycle instruction execution.

### Criticism: “Why is fetch not driven explicitly by the PC?”

Best response:

Because the project chooses to separate instruction acquisition from explicit next-PC reasoning. The real datapath does those in parallel, but the prototype serializes them to reduce early cognitive overload and make later PC control-flow reasoning more visible.

### Criticism: “Why use an instruction module if a textbook single-cycle datapath has no instruction register?”

Best response:

The instruction module is not a literal hardware instruction register. It is a learner-facing metaphor for fetched instruction availability and handoff between fetch and decode.

### Criticism: “Why are control signals not all resolved during decode?”

Best response:

Because if that were done strictly, later phases would lose much of their educational value and collapse into passive confirmation. The project deliberately distributes control-facing decisions so execute, memory, write-back, and PC update remain meaningful learning activities.

### Criticism: “Why not teach gates and ALU construction first?”

Best response:

Because this project is scoped as a datapath-level supplement, not a digital logic construction environment. It assumes gate-level logic is addressed elsewhere in the curriculum and instead targets the integration problem students often struggle with: tracing instruction execution and state change through the datapath.

---

## 11. Final Positioning For The Dissertation

The strongest final positioning statement is:

> The project should be framed not as a literal hardware-faithful simulator, but as a pedagogical VR abstraction of a single-cycle MIPS datapath. Its primary contribution is not cycle accuracy, but the externalization of hidden architectural relationships into spatial, interactive, learner-manipulable forms. In particular, the prototype preserves instruction decoding, operand identification, sign extension, ALU decision-making, memory access, write-back reasoning, and next-PC consequences, while deliberately simplifying or redistributing centralized control generation, mux selection, and parallel hardware timing to support teachability and embodied reasoning.

That paragraph should be treated as one of the key cornerstone formulations for later writing.

---

## 12. Short Internal Takeaway

If this whole document had to collapse into one plain statement, it would be:

> The prototype teaches the logic of single-cycle instruction execution more than the literal mechanics of single-cycle hardware.

That is not a weakness by itself.
It just needs to be stated clearly and defended honestly.

