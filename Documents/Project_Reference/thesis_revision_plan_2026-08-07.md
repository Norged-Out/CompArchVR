# Thesis Revision Plan - 2026-08-07

This file records the supervisor feedback received on `2026-08-07` and the dissertation-side revision posture that follows from it.

It is meant to be a stable tracked reference so the main writing direction does not depend on chat memory alone.

## Source Note From The Actual Session

The following raw note should be preserved as a direct source record of the
session itself, separate from the structured interpretation below:

> I am allowed to change the research question, and I am encouraged to do so.
> He mentioned how even though I attempt to push for the interpretation, the
> scope mismatch in the methodology with the question and hypothesis and the
> actual project is going to cause issues, and I should simply change the
> question (and hypo appropriately) to instead focus on this whole thing as the
> supplemental tool it is meant to be. thus possibly revising a larger portion
> of the writing in its entirety.
>
> he likes my prose and writing style, but he feels that there is a lot of
> repeatition, especially in chapters 1-4 (so basically all of it), where I go
> on to recite the same thing over and over. He feels that it is better for me
> to state something once where it is most appropriate, and then either refer
> back to it if I need to in a later section (like "in chapter 2, we see
> this.." or "in chapter 3, it was stated...") than constantly retell the same
> things, unless the context in which I say it makes it important enough to do
> so. he wants me to cut down the repitition as I make my revision.
>
> for my implementation chapter, he had a special remark that he feels that
> even though it has a lot of valuable information, what he feels is that it
> should NOT read as a technical design document, but more so along the lines of
> "what I have done" and "how well has the datapath been translated" to it. He
> also said something along the lines of how he "doesn't need to know about
> every single class or item that was used to build a component" but more so
> "what that component does and how does it do so for the learner and wrt to
> how the same thing happens in a traditional datapath." He recommended not
> only adding screenshots of the various assets, which I already plan to, but
> to also directly connect it with figures of the datapath and components, so
> that is probably something I am going to work on.
>
> he is fine with me citing the article btw. he will also be happy if I use
> more of it, and really told me to make connections to that figure in my
> design/implementation chapter to draw the translated connections.
>
> he also mentioned that I should think a bit more about how Im addressing the
> translation of the standard traditional datapath to how my experience does
>
> he encourages me to have the remaining 3 chapters be results (which only
> commits the findings, no assumptions or analysis should happen here
> whatsover), discussion (where I actually attempt to draw the connections
> based on the data I manage to get, and how it matches to the hypothesis and
> goal" as well as conclusion section. future work should also be talked about
> at some point.
>
> He wanted me to consider putting very strong emphasis on why this
> dissertation matters in the end because that is what he and my second reader
> will be looking for specifically: it is meant to showcasing a new design tool
> that can be used later, is it a research of sorts, an analysis, or something
> else. In simple terms, what he told me was to truly think and write about the
> CONTRIBUTION my dissertation is making, and to write that with that in mind.
>
> he does not care for bloat, and would rather have me produce valuable,
> quality writing, than incessent writing bloated for the sake of bloating.
>
> he requests a revised full draft by monday for his proper one and only
> feedback session, as he choose to not count today's one as something formal.

## Authority And Scope

This file is now the active tracked planning reference for the dissertation rewrite phase leading to the revised full draft due on `2026-08-10`.

It supersedes older thesis-side planning assumptions where they conflict with the supervisor feedback received on `2026-08-07`, especially when those older assumptions:

- defend a broader comparative research question than the study can support
- collapse `Results` and `Discussion` into one chapter structure
- leave the implementation chapter framed too much as an internal construction log
- tolerate repeated re-explanation of the same major framing points across early chapters

The original research-plan PDF should still be preserved as historical project context, but it should not override this file during the current revision pass.

## Core Outcome Of The Meeting

The main conclusion of the meeting was that the dissertation should stop trying to defend a mismatch between:

- the original broad comparative research question
- the actual exploratory participant study
- the real educational role of the prototype as a supplementary tool

The cleaner path is to revise the research question, hypotheses, and later discussion so the dissertation honestly reflects the study and the built artifact.

The main writing consequence is simple:

- stop trying to save the old question rhetorically
- rewrite the framing so the question, hypotheses, methodology, design, implementation, and later claims are all pulling in the same direction

## Supervisor Feedback Summary

### 1. Revise the Research Question

The research question can be changed and is encouraged to change.

The current wording is too broad because it sounds like a strong comparison against traditional teaching methods, while the implemented project and the actual study are much narrower. The revised question should treat the prototype as what it really is:

- a supplementary learning tool
- an exploratory educational intervention
- a scoped VR translation of selected single-cycle datapath concepts

Practical consequence:

- the dissertation should no longer promise a strong direct comparison against traditional teaching methods if the study does not actually support such a claim
- the revised framing should stay close to usefulness, support, clarity, and exploratory educational value

### 2. Reduce Repetition Across the Draft

The current draft repeats several major points too often, especially across Chapters 1 to 4.

The supervisor's recommendation was:

- state a major point once where it fits best
- refer back to it later if needed
- avoid retelling the same explanation in full unless the new context genuinely demands it

This applies especially to:

- the supplementary-tool framing
- the motivation from teaching experience
- the problem of static datapath understanding
- the abstraction / translation justification

Practical consequence:

- each of those points should have a primary home
- later chapters may refer back to them, but should not restate them in full unless the new chapter genuinely needs a different angle

### 3. Reframe the Implementation Chapter

The implementation chapter should read less like an internal construction log and more like a technical account of:

- what was built
- what each major component does
- how each component helps the learner
- how each component relates to the traditional datapath

The supervisor does not need a detailed inventory of every class or every internal helper used to assemble a component unless that detail helps explain the translation clearly.

The chapter should make heavier use of:

- screenshots of the built components
- direct comparison to the datapath figure and its sub-parts
- explicit discussion of how the traditional datapath was translated into the VR experience

Practical consequence:

- implementation should foreground major learner-facing components such as fetch handoff, register bank, datapackets, scanners, control-signal interaction, memory bank, and PC update
- each major component should be discussed in relation to what it corresponds to in the traditional datapath and what has been simplified, externalized, or redistributed

### 4. The Mishra Datapath Figure Is Acceptable To Use

The supervisor is content for the dissertation to use the cited datapath figure.

He explicitly encouraged stronger connections between that figure and the design / implementation chapters.

### 5. Strengthen the Translation Argument

The supervisor wants the dissertation to think more carefully about how it explains the translation from the standard traditional datapath to the VR experience.

This should become a central part of the later revision, especially in:

- Design
- Implementation
- Discussion

Practical consequence:

- the dissertation should not treat translation as a side note
- it should become one of the central argumentative threads of the artifact chapters and the later interpretation

### 6. Split the Final Chapters More Cleanly

The supervisor encourages the remaining chapter structure to be:

1. `Results`
2. `Discussion`
3. `Conclusion and Future Work`

With the following distinction:

- `Results` should report findings only
- `Discussion` should interpret those findings, connect them to hypotheses and goals, and discuss what they mean

### 7. Make the Contribution Explicit

The supervisor wants the dissertation to state more strongly why it matters.

The examiners will be looking for the contribution:

- what new tool or design contribution was made
- what kind of investigation this dissertation actually performs
- what value the resulting work has for later teaching, design, or research

Practical consequence:

- the dissertation should repeatedly return to contribution, but without bloated repetition
- the contribution should be made explicit in the introduction, defended in the artifact chapters, and interpreted clearly in the discussion and conclusion

### 8. Do Not Write For Bloat

The supervisor prefers strong, valuable writing over page-padding.

The dissertation does not need to be bloated. It needs to be clear, worthwhile, and well-argued.

### 9. Revised Full Draft Deadline

The next target is a revised full draft for supervisor review by:

- `2026-08-10`

Today's feedback session should not be treated as the one formal written-feedback pass.

## Immediate Writing Consequences

### Research Question / Hypothesis Direction

The dissertation should move toward a question framed around usefulness or support rather than broad comparative superiority.

Working direction:

- focus on usefulness as a supplementary learning environment
- focus on support for understanding selected single-cycle datapath concepts
- keep hypotheses aligned with usability, conceptual support, guided-to-independent progression, and supplementary value

Candidate question directions worth keeping in mind during revision:

1. usefulness framing:
   - `How useful is a virtual reality-based supplementary learning environment for helping learners understand selected single-cycle MIPS datapath concepts?`
2. support framing:
   - `To what extent can a virtual reality-based supplementary learning environment support learners' understanding of selected single-cycle MIPS datapath concepts?`
3. exploratory evaluation framing:
   - `How well does a virtual reality-based supplementary learning environment support learners in tracing and reasoning through selected single-cycle MIPS datapath behaviour?`

The point is not to lock the final exact sentence here, but to keep the framing narrow enough that the methodology and later claims can support it honestly.

Working hypothesis direction:

- main hypothesis:
  - a structured VR learning environment that externalises instruction execution into visible, spatial, and interactive steps can support understanding of selected single-cycle datapath concepts and be perceived as a useful supplement to existing teaching materials
- supporting hypotheses should stay bounded around:
  - usability and clarity
  - conceptual support
  - guided-to-independent progression
  - perceived supplementary value

### Repetition-Cut Policy

During the next revision pass:

- identify each major repeated idea
- keep the strongest version
- trim later repetitions into brief references or remove them entirely

High-priority repeated ideas to audit:

- why VR is being used
- why the project is supplementary rather than a replacement
- why static datapath understanding is difficult
- why abstraction was necessary
- what the learner is meant to gain from the route
- how the study is exploratory rather than strongly comparative

### Implementation Revision Policy

Implementation should prioritize:

- the built phases and components
- the learner-facing function of each component
- the translation from textbook datapath behaviour to VR
- screenshots and figure-linked discussion

Implementation should de-prioritize:

- excessive class-by-class reporting
- internal helper inventories
- detail that does not help explain the artifact or its translation

A useful chapter-level test:

- after each subsection, ask whether the reader now understands:
  - what was built
  - what part of the datapath it corresponds to
  - what it asks the learner to do
  - what was changed or simplified in the translation

If a passage mainly answers `which classes existed?`, it is probably not carrying the right weight.

### Final-Chapter Revision Policy

The remaining chapters should preserve this separation:

- `Results`: findings only
- `Discussion`: interpretation, connections, contribution
- `Conclusion and Future Work`: closing summary and next directions

This separation should also control tone:

- `Results` should avoid drifting into "what this means"
- `Discussion` should avoid pretending to be raw reporting
- `Conclusion and Future Work` should not become the first place where the contribution is finally explained

## Current Chapter Structure To Work Toward

1. `Introduction`
2. `Background and Related Work`
3. `Methodology`
4. `Design`
5. `Implementation`
6. `Results`
7. `Discussion`
8. `Conclusion and Future Work`

Expected chapter roles in the rewrite:

### Chapter 1 - Introduction

- establish the educational problem
- establish why the project matters
- state the revised research question clearly
- state the dissertation contribution clearly
- give the reader a reliable chapter guide

### Chapter 2 - Background and Related Work

- place the project in the literature
- explain the architecture-learning difficulty in context
- cover supplementary teaching tools, immersive learning, and related architecture work
- avoid becoming a second introduction

### Chapter 3 - Methodology

- present the revised research question and aligned hypotheses
- explain the study design, participants, instruments, procedure, analysis plan, and ethical scope
- stay honest about the exploratory character of the work

### Chapter 4 - Design

- explain why the environment was shaped the way it was
- justify abstractions, pedagogical choices, lesson structure, mode progression, and translation strategy

### Chapter 5 - Implementation

- explain what was actually built
- describe the technical realization of the design
- connect major learner-facing components to the corresponding datapath elements
- use screenshots and figures where possible

### Chapter 6 - Results

- report the data
- summarize questionnaire, knowledge-check, and qualitative outcomes
- avoid discussion and broader interpretation here

### Chapter 7 - Discussion

- interpret the findings
- connect them back to the revised question and hypotheses
- discuss how far the prototype seems useful as a supplementary tool
- relate the findings back to the literature and the contribution

### Chapter 8 - Conclusion and Future Work

- summarize what was achieved
- restate the dissertation contribution
- acknowledge the main limits
- point to the most meaningful next steps

## Highest-Priority Tasks Before 2026-08-10

1. revise the research question and hypotheses
2. align the methodology chapter with that revised framing
3. cut repetition across Chapters 1 to 4
4. reshape the implementation chapter around datapath-to-VR translation
5. keep the contribution visible across the later draft

Suggested revision order:

1. settle the revised research question wording
2. update hypotheses and methodology framing to match it
3. update introduction chapter-guide wording to match the new final chapter structure
4. run a repetition-cut pass across Chapters 1 to 4
5. reshape implementation around translation and learner-facing function
6. draft `Results`
7. draft `Discussion`
8. draft `Conclusion and Future Work`

If time becomes tight, correctness of framing matters more than cosmetic prose polish.

## Existing Materials That Need To Be Read With Care

The following materials remain useful, but must now be read as partially historical or conditional:

- `ProjectJournal/02_MASTER_EXECUTION_PLAN.md`
  - still authoritative on project motivation, scope, and build history
  - no longer authoritative on every older dissertation-shape assumption if this file says otherwise

- `ProjectJournal/03_PROJECT_SNAPSHOT.md`
  - still authoritative on live build truth
  - should be used together with this file for current writing posture

- `ProjectJournal/04_DECISION_LOG.md`
  - still authoritative on historical rationale
  - should now be read with the `2026-08-07` dissertation-framing decisions in mind

- `Documents/Project_Reference/TCD_SCSS_CS7CS6_Research_Plan_PriyanshNayak.pdf`
  - preserve as historical context and formal project background
  - do not let its older framing override the supervisor-guided rewrite direction recorded here

## Reminder

The cleanest framing is no longer:

- "does VR improve understanding compared to traditional teaching methods?"

The cleaner framing is closer to:

- "how useful is this VR system as a supplementary tool for helping learners understand selected computer architecture concepts?"

That narrower framing fits:

- the prototype
- the study
- the methodology
- the actual contribution
