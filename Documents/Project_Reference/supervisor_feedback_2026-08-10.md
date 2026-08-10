Gareth Young
15:47 (21 minutes ago)
to me

Hi Priyansh,
Thanks for sending this through. I have had a read through the draft. Overall, I think the writing is moving in a good direction, and the first five chapters give you a solid basis for the dissertation. The framing of the educational problem and the rationale behind the design are now clearer.

At this stage, though, I would prioritize completing the analysis and Chapters 6 and 8 rather than continuing to polish the earlier chapters. At present, the Results, Discussion, and Conclusion are still largely outlines, so these will determine how convincing the dissertation ultimately is.

I have broken down my main comments as follows...


Research question and scope...
I think the revised research question works well: “To what extent can an immersive virtual reality learning environment support students’ understanding of selected single-cycle MIPS datapath concepts as a supplementary educational tool?” That's much better.
The scope is much clearer now, particularly because you explain that “to what extent” is intended in an exploratory rather than controlled experimental sense. I would keep the RQ as it stands.
You will need to be careful about what the study allows you to claim. “Support students’ understanding” could easily become a claim that the VR intervention caused improved learning. Your study is not designed to demonstrate that conclusively, given the small sample, absence of a control condition, and limited pre/post comparison. The language in the Results, Discussion, and Conclusion should therefore remain cautious.


Hypotheses...
I would reconsider whether H1 to H4 need to be presented as formal hypotheses. At the moment, they read more as evaluation dimensions or sub-questions: usability and clarity, conceptual support, guided-to-independent progression, and supplementary value.
This would also fit better with your methodology, which you describe as exploratory and mainly descriptive. For example, “participants would tend to describe the system as a helpful supplementary learning tool” is difficult to treat as a conventional testable hypothesis.
My preference would be to retain the main RQ and introduce these four areas as aspects through which you evaluate it. This would also give you a very clean structure for the Results and Discussion. If you do retain them as hypotheses, you need to be explicit about what evidence would constitute support or non-support for each one.
Something like...
RQ: To what extent can an immersive virtual reality learning environment support students’ understanding of selected single-cycle MIPS datapath concepts as a supplementary educational tool?
The evaluation considers four aspects of this question:
usability and clarity;
perceived conceptual support;
progression from guided to less-guided interaction; and
perceived value as a supplement to conventional teaching.


Methodology...
The methodological framing is good. I particularly like that you are explicit about this being a small-scale exploratory evaluation rather than attempting to present it as a controlled educational trial.
There is one methodological issue that you need to address more directly: the tailored refresher. Participants complete the baseline questions, receive a verbal refresher, and then undertake the VR experience. This means that any improvement between the baseline and post-session assessment cannot be attributed solely to the VR system. The refresher itself may have contributed to that improvement.
This does not undermine the study, but it changes how you should interpret the evidence. I would describe baseline/post-session differences as evidence of performance following the overall learning session rather than evidence that VR itself caused an improvement.
There is a similar issue with the progression through Learning, Practice, and Test modes. Not everyone appears to have received exactly the same exposure, as progression depended on background, available time, and how comfortable participants appeared. You need to report this clearly, including how many participants completed each mode and how many required researcher intervention/support.


Results...
This should now be your main priority. You already have a structure for Chapter 6, but it needs to be populated with the actual study data.
For Participant Background and Study Context, report the final participant number and describe prior architecture experience, prior VR experience, and initial confidence/difficulty. Keep this descriptive.
For System-Experience Results, report the actual response distributions rather than describing responses simply as “positive”, “mixed”, etc. Frequencies/percentages and appropriate descriptive statistics would be useful here, and a figure or table may help make the response patterns easier to understand.
For the Knowledge-Check section, report performance on the five repeated baseline questions, overall post-session performance, and particularly difficult or commonly missed questions. Be cautious about interpreting baseline/post-session differences for the reasons above.
For Observed Progression, I would like to see clear evidence of how participants moved through Learning, Practice, and Test modes. How many reached each stage? Where did participants hesitate? How often was intervention required? A compact table might work particularly well here.
For the open-ended responses, keep the analysis focused. A small number of well-supported themes or categories would be preferable to a large thematic structure. For example, what helped understanding, what remained confusing, interface/navigation friction, and how participants positioned the VR experience relative to lectures, diagrams, tutorials, etc.
Most importantly, make sure the different forms of evidence speak to one another rather than appearing as unrelated datasets.


Discussion...
The proposed structure for Chapter 7 is good. I would retain the sections on usability/clarity, conceptual support and progression, supplementary-tool value, and limitations.
Try to make each section do more than repeat the Results. A useful pattern would be: finding to interpretation to relationship to previous literature to what this means for the research question/design.
You already have material in the literature review that you can return to here, particularly around staged guidance, gradual reduction of support, conceptual visualization, and the danger of cognitive/visual overload. Bringing that literature back into the Discussion will help connect Chapters 2 and 7.
The Discussion should ultimately give the reader a clear answer to the RQ. That answer can be positive while still being bounded. You do not need to demonstrate that VR is better than conventional teaching for the dissertation to make a useful contribution.


Design and implementation...
I think there is a stronger contribution here than simply “I built a VR application for teaching MIPS”, and I would make this more explicit later in the Discussion and Conclusion.
One of the more interesting design decisions is translating hidden and effectively concurrent datapath processes into sequential teaching phases that learners can see, follow, and interact with. You are deliberately prioritizing the reasoning structure needed by the learner rather than attempting to reproduce processor timing literally. I think this is worth foregrounding as part of the contribution.
Your learning outcomes also provide a good structure for demonstrating this, moving from decoding and identifying component roles through tracing execution and predicting state changes to completing the process with progressively less guidance.
I would not spend much more time expanding the technical chapters at this point. Once Chapters 6 and 7 are complete, you can revisit Chapters 4 and 5 and decide whether any implementation detail can be shortened or moved to an appendix.


Background and related work...
This is generally in good shape. I like that you don't frame conventional teaching as deficient or suggest that VR automatically solves the problem. Instead, you identify a fairly specific difficulty in translating static representations into a mental model of instruction execution and position the prototype as supplementary support at that point.
I would avoid substantially expanding Chapter 2. Once you have completed the analysis, you may find that a particular result requires some additional literature for the Discussion, but otherwise your time is better spent elsewhere.


Conclusion...
Keep the Conclusion short. Return to the original educational problem, briefly recap what you designed and evaluated, state the main contribution, and then provide a direct but bounded answer to the research question.
Your current note that the evidence should be presented as “suggestive rather than definitive” is right. I would also distinguish between what the dissertation demonstrates about the particular prototype and what would need further study before making broader claims about VR and computer architecture education.
Future work can then follow naturally from the limitations: larger and comparative studies, longer-term retention, interface refinement, broader instruction coverage, and potentially multi-cycle or pipelined architectures.


Appendices...
The full questionnaire definitely belongs in the appendix, as you already have it. I would also consider including the Participant Information Leaflet and consent materials, recruitment materials, the five baseline questions, details/script for the verbal refresher, and the researcher observation protocol or template.
Additional detailed results tables can also go into the appendices if they are useful for transparency but would interrupt the flow of Chapter 6. Similarly, lower-level technical implementation material can be moved there if you later need to shorten Chapter 5.
Do not include raw identifiable participant data!


Glossary...
I don't think you need a glossary, but if you think you need one, make a table.
Terms such as MIPS, ALU, PC, VR, datapath, etc. should simply be defined when first introduced.
If you feel there are enough abbreviations to warrant additional support for the reader, a short list of abbreviations would probably be more useful than a glossary.


Final notes...
Obviously, the Abstract and Acknowledgments still need to be completed, but I would leave the Abstract until the Results and Discussion are finished. You cannot really write an effective abstract until you know exactly what the study found and what conclusions you are drawing from it.
Once the writing is complete, do a final consistency pass for terminology, capitalization, figure/table references, citations, and the naming of Learning, Practice, and Test modes.


Overall, I think you have a good foundation here. The main task now is to turn the study data into a clear empirical argument. I would focus your effort on the Results first, then the Discussion, and only after that return to polishing the earlier chapters.
Bests,
Gareth

PS: I will be in Stack B tomorrow from 9 AM. I have a bunch of stuff on, but I should be able to route people to you as they come in. I'll see you at 11:00.
