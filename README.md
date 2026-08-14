# Creating Novel Virtual Reality Interfaces for Teaching Computer Architecture

This repository contains my Master’s dissertation project at Trinity College Dublin.

It now reflects the final submitted baseline of the project as evaluated for the dissertation. Any future changes would be personal-interest extensions rather than part of the formal submission.

At its core, this project was an attempt to make computer architecture feel less abstract and less miserable to learn. The main prototype is a VR learning environment built around a single-cycle MIPS datapath, where the learner can move through the instruction flow step by step and interact with the system in a more physical, spatial way than a normal diagram or lecture slide would allow.

Demo video:  
[YouTube walkthrough](https://youtu.be/Y8tFYnWeLtI)

Official build release:  
[itch.io page](https://norgedout.itch.io/comparchvr)

## What’s in here

- [Creating Novel Virtual Reality Interfaces for Teaching Computer Architecture.pdf](<Creating Novel Virtual Reality Interfaces for Teaching Computer Architecture.pdf>)  
  The main dissertation report in PDF form.

- `ThePrototype/`  
  The main Unity project. This is where the actual VR system lives.

- `Documents/Literature_Review/`  
  Research papers, reading notes, and bibliography support material used to shape the research gap and dissertation framing.

- `Documents/Presentation/Presentation_PriyanshNayak.pptx`  
  The presentation deck used for the dissertation demo.

- `ProjectJournal/`  
  My running project notes: progress tracking, design decisions, planning, pivots, and general project state over time.

- `Latex/`  
  The live dissertation source project, including chapter files, appendices, figures, bibliography, and front-matter material.

- `SPSS/`  
  Study-analysis material, including the data-processing scripts, SPSS syntax, password-protected raw data file, and supporting quantitative-analysis outputs.

## Quick Project Summary

The final prototype supports three modes:

- **Learning Mode** for guided walkthroughs
- **Practice Mode** for reduced-support completion
- **Test Mode** for stricter independent assessment

The learner moves through the main datapath stages of instruction execution, including Instruction Fetch, Decode, Execute, Memory Access, Write-Back, and Program Counter Update.

## Supported Instructions

`Learning Mode` covers `add`, `addi`, `lw`, `sw`, `sub`, `and`, `or`, `slt`, `beq`, `bne`, and `j`.  
`Practice Mode` and `Test Mode` use the same ten-instruction non-jump pool, with Test selecting from that pool randomly.

| Mode | Authored instruction set |
| --- | --- |
| Learning | `add t0, t1, t2`; `addi v1, t3, 4`; `lw at, 12(s2)`; `sw t6, 12(s4)`; `sub t5, t3, t6`; `and t8, t4, t7`; `or t9, a0, a1`; `slt v0, a2, a3`; `beq k0, k1, 8`; `bne a0, a1, 8`; `j target` |
| Practice / Test | `add t3, t4, t5`; `addi ra, zero, 16`; `lw v0, 12(s5)`; `sw t7, 12(s3)`; `sub t2, fp, at`; `and v1, sp, fp`; `or t8, at, a2`; `slt at, zero, t0`; `beq v0, k1, 4`; `bne a2, a3, 4` |

## Register Table

These are the current authored logical defaults for the 32-register bank:

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

## Memory Table

The authored data-memory bank currently starts with the following word values:

| Address | Value |
| --- | ---: |
| `268500992` | 10 |
| `268500996` | 20 |
| `268501000` | 30 |
| `268501004` | 40 |
| `268501008` | 50 |
| `268501012` | 60 |
| `268501016` | 70 |
| `268501020` | 80 |
| `268501024` | 90 |
| `268501028` | 100 |
| `268501032` | 110 |
| `268501036` | 120 |
| `268501040` | 130 |
| `268501044` | 140 |
| `268501048` | 150 |
| `268501052` | 160 |
| `268501056` | 170 |
| `268501060` | 180 |
| `268501064` | 190 |
| `268501068` | 200 |
| `268501072` | 210 |
| `268501076` | 220 |
| `268501080` | 230 |
| `268501084` | 240 |

## Extra Note

The final playable build is distributed through [GitHub Releases](https://github.com/Norged-Out/CompArchVR/releases) and the public [itch.io page](https://norgedout.itch.io/comparchvr), rather than being stored directly in the repository itself.
