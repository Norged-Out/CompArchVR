# Project Snapshot

This file is meant to help me quickly resume work after breaks, session resets, or interrupted work periods.

## What This Project Is Trying To Be

A focused VR learning experience for computer architecture, centered on helping learners reason through single-cycle MIPS datapath execution more effectively than static diagrams alone.

## Current Agreed Direction

The project should:
- answer the "why VR?" question through guided spatial reasoning
- focus on one learning objective first
- use VR for meaningful physical/spatial choices
- use UI for lookup tables, prompts, and explanations

## Motivation Anchor

The project motivation is grounded in earlier teaching experience, not in VR novelty by itself.

During my Bachelor's at the University of Arizona, I worked as a teaching assistant for Computer Organisation for four consecutive semesters. Across those semesters, students repeatedly struggled with the same cluster of issues:
- mentally navigating the CPU datapath
- understanding what different control signals were doing for different instructions
- following how values moved between registers, ALU logic, memory, and PC updates
- carrying that reasoning into nearby topics such as stacks and function calls

The most effective teaching pattern was usually to draw over the datapath on a whiteboard or iPad and walk the learner through the flow step by step. This prototype is meant to turn that guided tracing process into an embodied, self-paced VR supplement.

## Current Active Prototype Area

Scene:
- `D:\CompArchVR\ThePrototype\Assets\Scenes\Testing Ground.unity`

Current prototype features:
- scene-authored lesson flow built around `Lesson Guide`
- full three-mode baseline across `Learning`, `Practice`, and `Test`
- practice-mode support for the ten core non-jump instructions, plus strict randomized `Test` flow built on that same pool
- authored world-space lesson UIs for intro, decode, ALU, memory, write-back, and PC update
- physical fetch flow, 32-register bank, datapacket system, immediate extender, and authored phase stations
- routed map, moving gates, and arrow guidance as the active navigation baseline
- wrist settings menu, spatial keyboard support, tutorial/onboarding surfaces, gameplay audio, and background music
- authored instruction and info catalogs backing lesson bookkeeping

Current supported instructions:
- `add`
- `addi`
- `lw`
- `sw`
- `sub`
- `and`
- `or`
- `slt`
- `beq`
- `bne`
- `j`

Practice-mode counterparts now exist for the same full baseline:
- `add`
- `addi`
- `lw`
- `sw`
- `sub`
- `and`
- `or`
- `slt`
- `beq`
- `bne`

Current build truth:
- all listed learning-mode instructions work through the current guided loop
- the ten non-jump instructions also have authored practice-mode variants that feed the shared downstream phase flow
- test mode is the stripped-down assessment variant of that same practice foundation
- the project is now effectively complete for the presentation and participant studies
- participant-study use has now begun, so the build should be treated as locked unless a real blocker appears
- further work should be treated as future work unless it fixes a real blocker
- centralized dev-mode support now exists inside the settings menu so late-phase testing no longer depends on replaying the whole lesson every time

More detailed current-state notes:
- fetch is no longer just UI framing; it uses a physical instruction upload/download handoff
- intro flow is no longer only a single instruction picker; it is now a mode-first entry point
- decode now combines field framing, opcode/funct validation where needed, source-operand scanning, and immediate generation
- practice decode uses encoded fetch display, full 32-bit binary display, typed bitfield entry, staged validation, and limited hints/attempts
- the settings-menu instruction readout no longer leaks the decoded assembly form during Practice `IF` / `ID`; it stays in hex until decode is behind the learner
- the settings-menu instruction readout for Practice and Test no longer reveals the full assembly form during `IF` / `ID`; once decode is behind the learner it falls back to the instruction name only
- decode no longer forces `rs` to be scanned before `rt`; required source registers can now be satisfied in either order while still respecting scanner roles
- later practice phases use limited validation attempts, limited scanner attempts, limited hints, and held restart-on-failure flow
- when no lesson is active, the two decode-stage lesson scanners can now act as simple preview scanners instead of staying uselessly inactive
- the first participant-facing build pass exposed a few build-only scanner/UI regressions, and those have now been treated as blocker-grade fixes rather than as reasons to reopen the architecture
- execute handles ALU signal interaction, operand acceptance, operation selection, and result spawning
- memory handles address validation, read/write behavior, memory-bank communication, and load-result spawning
- write-back handles destination validation, value-source validation, and final register updates
- PC update handles branch/jump framing, next-PC confirmation, and lesson conclusion
- learn-mode `j` now also reuses the PC-update phase to teach the jump control path without expanding into the rest of the jump family yet

What is already solved versus now deferred:
- already solved enough to build on:
  - core lesson flow
  - instruction fetch embodiment
  - register interaction
  - immediate extension
  - ALU execution
  - memory access for `lw` / `sw`
  - write-back
  - branch resolution
- now mostly deferred to future work:
  - additional jump-family work beyond `j`
  - optional opening-sequence work
  - optional socket/scanner interaction expansion only if it can be proven stable without introducing build-only regressions
  - light route/gate readability tuning only if participant testing exposes issues
  - optional VFX / ambience expansion beyond the current baseline

Current non-build focus:
- participant-session support
- presentation preparation
- thesis-facing consolidation of motivation, literature framing, and learning outcomes

## Authored Register Bank Reference

These are the current authored logical defaults in the `Register Zone` baseline:

| Register | Value |
| --- | ---: |
| `$zero` | 0 |
| `$at` | 1 |
| `$v0` | 0 |
| `$v1` | 0 |
| `$a0` | 4 |
| `$a1` | 8 |
| `$a2` | 12 |
| `$a3` | 16 |
| `$t0` | 10 |
| `$t1` | 20 |
| `$t2` | 30 |
| `$t3` | 40 |
| `$t4` | 50 |
| `$t5` | 60 |
| `$t6` | 70 |
| `$t7` | 80 |
| `$s0` | 268500996 |
| `$s1` | 268501008 |
| `$s2` | 268501020 |
| `$s3` | 268501032 |
| `$s4` | 268501044 |
| `$s5` | 268501056 |
| `$s6` | 268501068 |
| `$s7` | 268501080 |
| `$t8` | 90 |
| `$t9` | 100 |
| `$k0` | 0 |
| `$k1` | 0 |
| `$gp` | 268500992 |
| `$sp` | 8192 |
| `$fp` | 8192 |
| `$ra` | 0 |

Notes:
- the `s` registers plus `gp` are currently being used as stable data-memory-style base addresses
- ordinary lesson resets do not wipe register values anymore
- authored defaults are restored when the active lesson mode changes

## Instruction Bank Reference

### Learning Mode

| Instruction | Assembly |
| --- | --- |
| `add` | `add t0, t1, t2` |
| `addi` | `addi v1, t3, 4` |
| `lw` | `lw at, 12(s2)` |
| `sw` | `sw t6, 12(s4)` |
| `sub` | `sub t5, t3, t6` |
| `and` | `and t8, t4, t7` |
| `or` | `or t9, a0, a1` |
| `slt` | `slt v0, a2, a3` |
| `beq` | `beq k0, k1, 8` |
| `bne` | `bne a0, a1, 8` |
| `j` | `j target` |

### Practice Mode

| Instruction | Encoded Display | Runtime Assembly |
| --- | --- | --- |
| `add` | `0x018D5820` | `add t3, t4, t5` |
| `addi` | `0x201F0010` | `addi ra, zero, 16` |
| `lw` | `0x8EA2000C` | `lw v0, 12(s5)` |
| `sw` | `0xAE6F000C` | `sw t7, 12(s3)` |
| `sub` | `0x03C15022` | `sub t2, fp, at` |
| `and` | `0x03BE1824` | `and v1, sp, fp` |
| `or` | `0x0026C025` | `or t8, at, a2` |
| `slt` | `0x0008082A` | `slt at, zero, t0` |
| `beq` | `0x105B0004` | `beq v0, k1, 4` |
| `bne` | `0x14C70004` | `bne a2, a3, 4` |

### Test Mode

Test mode currently reuses the same ten-instruction practice pool:
- `add`
- `addi`
- `lw`
- `sw`
- `sub`
- `and`
- `or`
- `slt`
- `beq`
- `bne`

Differences:
- instruction is selected randomly at lesson start
- lesson and hint panels are hidden
- each phase allows only one validation failure and one scanner failure
- no hints are available

## Most Important Script Areas Right Now

- lesson flow:
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Flow`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\UI`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\CpuLesson\Support`
- shared authored-data helpers:
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\Shared\Info`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\Shared\UI`
- core interaction systems:
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\Registers`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionFetch`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\ALU`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\Memory`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\WriteBack`
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\PcUpdate`
- instruction data model:
  - `D:\CompArchVR\ThePrototype\Assets\MyScripts\InstructionSystem`

## Historical Initial Rollout Order

1. `add`
2. `addi`
3. `lw`

Why:
- `add` teaches the cleanest register-register path
- `addi` adds immediate handling without memory complexity
- `lw` adds address calculation and memory read/write-back behavior

## Current Project Stage

- supervisor checkpoint on `2026-07-24` is complete
- official presentation target remains `2026-07-29`
- the Android XR build is now the study-use baseline
- core development is effectively complete; remaining work should be limited to critical fixes or clearly justified future extensions

## If I Resume After A Break

Key files to reread:
- `ProjectJournal/02_MASTER_EXECUTION_PLAN.md`
- `ProjectJournal/01_PROGRESS_LOG.md`
- `ProjectJournal/04_DECISION_LOG.md`
- `ProjectJournal/03_PROJECT_SNAPSHOT.md`

## If I Need To Stop Mid-Session

Before stopping:
- make sure `01_PROGRESS_LOG.md` reflects the current state
- make sure any major scope shift is written into `04_DECISION_LOG.md`

## Future Work Hand-Off

If development resumes beyond participant testing, the most plausible follow-up areas are:

1. more jump-family work:
   - `jal`
   - `jr`
2. optional opening-sequence work if a stable moving-platform or equivalent intro beat is worth reviving
3. further route/guidance readability tuning if testing exposes a need
4. optional scanner/socket interaction expansion only where it is proven stable
5. optional deeper ambience / VFX polish
6. later large-scope extensions:
   - multicycle datapath
   - pipelining
   - stalls / hazards
