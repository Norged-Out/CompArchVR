# Formal Revision Plan for Chapters 6--8

This plan is based on the current live dissertation state in the LaTeX project as of Monday, August 10, 2026, together with the supervisor feedback received from Gareth Young on the same date. It is intended to guide the next substantive dissertation pass, with specific attention to the fact that Chapters 1--5 now provide a workable foundation, while Chapters 6--8 remain the weakest and most underdeveloped part of the report.

The practical implication is straightforward: the dissertation should not spend further meaningful time polishing the earlier chapters until the Results, Discussion, and Conclusion are able to carry the core empirical and argumentative weight of the work. The report is now at the stage where the credibility of the dissertation will depend less on further refinement of framing and implementation detail, and more on whether the collected study evidence is transformed into a coherent, bounded, and convincing final argument.

## 1. Current Dissertation State

The live dissertation currently presents a clear trajectory across the first five chapters. The Introduction establishes the educational problem with an appropriate level of restraint, frames the VR system as a supplementary tool rather than a replacement for conventional teaching, and introduces the revised research question in a way that is much better aligned with the actual scale of the study. The Background and Related Work chapter is also in a generally stable position: it identifies a narrow educational difficulty, situates the prototype within architecture-learning and VR-in-computer-science-education literature, and avoids overclaiming about VR as a medium.

The Methodology chapter is substantially stronger than the current back-end chapters, but it still contains one issue that now deserves explicit strategic attention. In its present form, it retains the main hypothesis and four supporting hypotheses. Gareth's feedback suggests that these may no longer be the strongest way to structure the report, especially given the exploratory and mainly descriptive character of the study. That does not necessarily mean they must be removed immediately, but it does mean that the later chapters should be written in a way that can support either of two outcomes: retaining the hypotheses with very careful evidential wording, or later reframing them as evaluation dimensions through which the main research question is answered.

The Design and Implementation chapters together already contain the strongest non-evaluative contribution of the dissertation. In particular, the implementation chapter now makes visible a more defensible and interesting contribution than merely having built a VR teaching application. What emerges from the current text is a deliberate translation of hidden, effectively concurrent datapath behaviour into sequential, learner-visible teaching phases distributed across a guided VR route. This idea should not be left buried inside Chapter 5. It must be brought back explicitly in Chapters 7 and 8 as one of the main conceptual contributions of the dissertation.

By contrast, the current Results, Discussion, and Conclusion chapters are still at outline level. The present `results.tex` remains a list of prompts rather than a chapter. `discussion.tex` is only lightly scaffolded and does not yet operate as interpretation. `conclusion.tex` remains a reminder structure rather than a finished closing argument. These chapters therefore need not just filling in, but a more disciplined and integrated redrafting process.

## 2. Overarching Strategic Direction

The next phase of dissertation writing should be governed by four high-level principles.

First, the report must stay bounded in its claims. The revised research question is appropriate precisely because it asks "to what extent" in an exploratory sense. That bounded framing must now be protected all the way through the final chapters. The study cannot support a strong causal claim that the VR system itself improved learning, nor can it support a broad claim that VR is superior to conventional teaching in computer architecture education.

Second, the findings must be treated as the outcome of the overall learning session rather than of the VR system in isolation. This point now becomes central. Participants completed a baseline probe, then received a tailored verbal refresher, then undertook the VR lesson, then completed the post-session form. Any baseline-to-post-session shift must therefore be interpreted as evidence following the whole learning session, not as clean proof of VR-driven improvement.

Third, the evidence sources must be written as parts of one empirical story rather than as separate mini-datasets. The background data, Likert responses, knowledge-check performance, observed progression, and open-ended feedback should not appear as unrelated piles of evidence. Chapter 6 must be structured so that those strands are reported distinctly but also made to speak to one another.

Fourth, the later chapters must foreground the strongest contribution of the project in its current form. That contribution is not simply technical implementation. It lies in the pedagogical design decision to turn compact, partly hidden datapath behaviour into a staged, visible, interactive route that lets learners trace instruction execution as a sequence of meaningful decisions.

## 3. Chapter 6 Revision Plan: Results

Chapter 6 now needs to become a genuinely data-led findings chapter. Its role is not to argue yet, but to report clearly enough that the later argument becomes possible. The chapter should therefore avoid overinterpretation, avoid evaluative exaggeration, and avoid drifting into design defence before the evidence has been properly presented.

### 3.1 Opening Orientation

The chapter should open with a short orienting paragraph that explains what evidence is being reported and how the chapter is organised. This should be written as a proper narrative paragraph, not as a list. It should briefly state that the chapter reports participant background, system-experience responses, knowledge-check findings, observed progression through the lesson modes, and focused open-ended feedback themes. It should also make clear that interpretation is deferred to Chapter 7.

### 3.2 Participant Background and Study Context

This section should remain descriptive and should establish the participant cohort as varied rather than uniform. The current methodology already anticipates variation in prior VR experience, prior architecture exposure, and pre-session confidence. Chapter 6 should now report those distributions clearly using the actual study data.

At minimum, this section should include:

- the final participant count;
- prior exposure to computer architecture or computer organisation;
- prior VR experience;
- pre-session confidence;
- pre-session difficulty perceptions; and
- any useful descriptive note on previously studied topics if this helps contextualise later performance.

The prose here should not infer too much from these distributions. Its task is to establish starting conditions and to remind the reader that participants did not enter the study from the same baseline.

### 3.3 System-Experience Results

This section should no longer rely on general language such as "responses were broadly positive" without evidential support. Instead, the actual response distributions should be reported. Frequencies and percentages should be given, and where useful, means or medians may also be reported as descriptive aids. If a figure or table helps compress and clarify the response patterns, it should be included.

The section should likely be organised into three subsections or internal clusters:

- usability and clarity;
- engagement; and
- perceived learning support.

This structure would align well both with the questionnaire content and with Gareth's feedback. The important thing is that the reporting stays grounded in the actual items rather than in abstract summary adjectives. If certain items are visibly stronger or weaker than others, that pattern should be shown rather than blurred away.

### 3.4 Knowledge-Check Results

This section requires especially careful wording. It must explicitly acknowledge the study structure: participants completed the baseline subset, then received a verbal refresher, then completed the VR session, then answered the post-session knowledge items. The section should therefore present the repeated-question pattern as descriptive evidence following the overall session, not as causal proof of the VR system's isolated effect.

The baseline subset should be named clearly and consistently. The repeated baseline probe uses Questions 11--14 and 25, and this needs to be stated plainly in the chapter. These questions cover foundational ideas such as the program counter, fetch, the instruction register, and stage ordering, and therefore act as a modest checkpoint on initial conceptual orientation.

The section should then report:

- repeated baseline-item performance;
- related post-session performance on those same items;
- overall post-session knowledge performance;
- commonly missed or difficult questions; and
- any visible pattern between foundational items and later instruction-specific items.

The goal is not to squeeze a large inferential story out of a small sample, but to show where the evidence suggests stronger understanding, weaker understanding, or persistent difficulty.

### 3.5 Observed Progression Through Learning, Practice, and Test

This is one of the most important additions required by Gareth's feedback. The report currently acknowledges that progression could vary, but the Results chapter must now show this clearly using the actual session evidence.

This section should document:

- how many participants completed only Learning mode;
- how many progressed to Practice mode;
- how many reached Test mode;
- where participants hesitated;
- where researcher support was required; and
- what kinds of support were most commonly needed.

A compact table would likely be the clearest first move here. That table could summarise participant progression, mode exposure, and intervention level. Short prose should then follow to identify the most meaningful patterns. The key point is to make visible that not all participants received identical exposure, and that this variation is itself part of what the evidence means.

### 3.6 Open-Ended Feedback Themes

This section should remain focused and restrained. It should not attempt a large or overly formal thematic-analysis performance. Gareth's advice here is correct: the report would be stronger with a small number of well-supported themes than with a sprawling coding structure.

The likely themes already suggested by the data and by the current study design are:

- what helped participants understand the lesson content;
- what remained confusing;
- where interface or navigation friction affected the experience; and
- how participants positioned the VR system relative to lectures, diagrams, tutorials, or other conventional learning supports.

This section should also be written so that it connects back to earlier quantitative patterns where appropriate. For example, if participants describe clearer execution flow but also mention movement or interface friction, that relationship should be visible in the prose rather than treated as disconnected evidence.

### 3.7 Close of Chapter 6

The Results chapter does not need a large concluding section, but it would benefit from a very short closing paragraph. That paragraph should simply state that the findings offer a bounded descriptive account of the study, that the evidence shows both encouraging patterns and visible limitations or points of strain, and that the meaning of those patterns is taken up in Chapter 7.

## 4. Chapter 7 Revision Plan: Discussion

Chapter 7 must do more than restate the reported findings. Its task is to answer the research question carefully, relate the evidence back to the literature and design rationale, and explain what kind of contribution the dissertation can reasonably claim.

The most useful internal pattern for each discussion section will be:

finding -> interpretation -> relationship to previous literature -> implication for the research question and design.

### 4.1 Returning to the Research Question

The chapter should begin by restating the research question briefly and then moving into a cautious but direct answer. The answer does not need to be timid, but it must remain bounded. The safest general direction is that the prototype appears to show some supportive value within the scope of this exploratory study, especially as a supplementary aid for helping learners follow selected datapath concepts, while the evidence remains limited and non-definitive.

This section should avoid all causal language implying that the prototype proved improved learning in any broad or controlled sense.

### 4.2 Usability, Clarity, and Route Legibility

This section should draw together the system-experience items and the observation notes to ask whether the environment was sufficiently readable, understandable, and manageable for its educational purpose to matter. It should not become a general UX critique. Its focus is educational usability: whether the route, panels, interactions, and teaching structure were clear enough to support the intended conceptual work.

The current Design and Implementation chapters give good material to return to here, especially around route structure, local panels, staged visibility, and selective interaction. This section should therefore test whether those design choices appear to have worked in practice.

### 4.3 Conceptual Support and Guided Progression

This section is likely to be the strongest intellectual core of the Discussion chapter. It should pull together the knowledge-check findings, progression evidence, observation notes, and open-ended feedback to ask whether the staged lesson structure appears to have supported understanding of selected datapath concepts.

This is also the place to foreground one of the dissertation's strongest contributions: the translation of hidden and effectively concurrent datapath processes into sequential teaching phases that learners can follow, act upon, and revisit. That line is already latent in the current Implementation chapter and should now become explicit in the final argument.

The literature from Chapter 2 on staged guidance, conceptual visualisation, and gradual reduction of support should be brought back here. In particular, the chapter should connect the mode structure and route design to the broader idea that architecture learning benefits from help that turns static or compact representations into more guided conceptual sequences.

### 4.4 Supplementary-Tool Value

This section should reinforce the intended educational position of the prototype. The system was not designed to replace lectures, tutorials, diagrams, or simulators. The stronger and more defensible claim is that it may serve as a useful supplement at the point where learners struggle to convert those conventional representations into a coherent mental model of execution.

This section should therefore connect participant positioning of the system with the original educational problem established in Chapters 1 and 2. If the evidence suggests that the system works best as a bridge, reinforcement layer, or guided support tool, that should be stated directly.

### 4.5 Limitations and Interpretive Boundaries

This section is non-negotiable. It should be explicit and unambiguous about the dissertation's limits. These include:

- small sample size;
- absence of a control condition;
- no long-term retention measurement;
- refresher support before VR exposure;
- uneven progression and uneven support exposure across participants; and
- the bounded scope of the implemented instruction set and lesson design.

This section protects the credibility of the dissertation. It should therefore not be treated as an apologetic afterthought, but as the necessary boundary-setting that lets the rest of the interpretation remain trustworthy.

### 4.6 Closing Chapter 7

The chapter should close by returning to the bounded answer to the research question. The closing should be positive where the evidence supports positivity, but still clearly exploratory. It should prepare the Conclusion chapter by establishing that the prototype shows promise within scope, while broader claims remain outside what this dissertation can currently support.

## 5. Chapter 8 Revision Plan: Conclusion

Chapter 8 should remain short, sober, and direct. It should not reopen the whole dissertation, and it should not read like a second discussion chapter.

### 5.1 Return to the Original Educational Problem

The opening should briefly return to the original difficulty identified at the start of the dissertation: learners may be able to recognise datapath components without yet being able to explain instruction execution as a connected process, especially when hidden internal behaviour must be mentally reconstructed from static diagrams or partial representations.

### 5.2 Brief Dissertation Recap

The report should then briefly recap what the dissertation did: identified the educational problem, designed a bounded VR learning environment, implemented that environment around selected single-cycle MIPS datapath concepts, and evaluated it through a small-scale exploratory study.

### 5.3 Main Contribution

The strongest contribution should be stated in a form stronger than "a VR application was built." The conclusion should foreground that the work explored how selected single-cycle datapath processes can be translated into staged, visible, learner-followable teaching interactions in VR, and that this pedagogical translation was then examined through participant evidence.

### 5.4 Final Answer to the Research Question

The conclusion should then give a direct but bounded answer. It should state that the evidence is suggestive rather than definitive, that the prototype appears most convincing as a supplementary educational aid rather than a replacement for conventional teaching, and that the study supports cautious optimism rather than broad proof.

### 5.5 Future Work

The final future-work section should follow naturally from the actual limits of the study and build. These likely include:

- larger and more comparative studies;
- longer-term retention measures;
- interface and navigation refinement;
- broader instruction coverage; and
- possible extension beyond the current selected single-cycle scope into larger or more complex architecture content.

## 6. Interaction with Chapters 2, 3, 4, and 5

Gareth's feedback does not require abandonment of earlier chapters, but it does change how they should now be treated.

Chapter 2 does not currently need substantial expansion. However, a mild and strategic increase later could still be justified if the finished Discussion chapter reveals a clear gap in literature support. If that happens, the most plausible areas for targeted addition would be staged guidance, gradual reduction of support, conceptual visualisation, or cognitive and visual overload in immersive learning design. The key point is that any Chapter 2 expansion should now be reactive to the needs of the final Discussion, not proactive filler.

Chapter 3 may later need a small reframing pass if the hypothesis structure is revised into evaluation dimensions or interpretive aspects. That decision should probably be made only after Chapters 6 and 7 are fully drafted, since those chapters will reveal whether the hypotheses still function naturally or whether Gareth's suggested restructuring is cleaner.

Chapters 4 and 5 should not be meaningfully expanded right now. If anything, once the back-end chapters are complete, there may be a case for slight reduction or relocation of some lower-level detail into appendices if space, focus, or proportional balance becomes an issue.

## 7. Recommended Immediate Writing Sequence

The next writing pass should proceed in this order:

1. Finalise the processed study outputs and descriptive materials needed for reporting.
2. Write Chapter 6 fully from evidence, including one compact progression table and any necessary result tables or figures.
3. Draft Chapter 7 directly from the finished Chapter 6 findings, using the literature only where it supports interpretation.
4. Write a concise Chapter 8 once the argumentative shape of Chapter 7 is clear.
5. Revisit Chapter 3 only if the hypothesis framing now feels structurally awkward.
6. Revisit Chapter 2 only if the Discussion clearly needs one or two targeted additional sources.
7. Perform a final consistency pass later across terminology, capitalization, figure/table references, citations, and the naming of Learning, Practice, and Test modes.

## 8. Final Planning Note

The core challenge now is not generating more material, but controlling the shape of the argument. The dissertation already has a workable foundation. What it now needs is a disciplined transition from design-and-build report to research report. Chapters 6--8 are the point at which that transition either succeeds or fails. The evidence should therefore be reported clearly, interpreted cautiously, and tied back to the educational problem and pedagogical contribution without drifting into claims that the study design cannot support.
