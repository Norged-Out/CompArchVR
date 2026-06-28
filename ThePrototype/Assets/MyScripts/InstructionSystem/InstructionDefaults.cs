using UnityEngine;

/// <summary>
/// Temporary fallback instruction definitions used when Resources assets are
/// missing or have not been imported yet.
/// The goal is to keep the prototype playable without forcing lesson data to
/// live inside the scene.
/// </summary>
public static class InstructionDefaults
{
    public static InstructionDefinition CreateFallbackAdd()
    {
        var instruction = ScriptableObject.CreateInstance<InstructionDefinition>();
        instruction.displayName = "add";
        instruction.mnemonic = InstructionMnemonic.Add;
        instruction.format = InstructionFormat.RType;
        instruction.category = InstructionCategory.Arithmetic;
        instruction.summary = "Add two registers and write the result into a destination register.";
        instruction.assemblyInstructionText = "add t2, t0, t1";
        instruction.fieldBreakdownText = "opcode: 000000\nrs: t0\nrt: t1\nrd: t2\nfunct: 100000";
        instruction.opcodeBits = "000000";
        instruction.functBits = "100000";
        instruction.expectedRs = "t0";
        instruction.expectedRt = "t1";
        instruction.expectedRd = "t2";
        instruction.touchesDataMemory = false;
        instruction.writesRegisterFile = true;
        instruction.usesImmediate = false;
        instruction.usesDestinationRegister = true;
        instruction.initialRegisterValues = new[]
        {
            new InstructionRegisterValue { registerId = "t0", value = 5 },
            new InstructionRegisterValue { registerId = "t1", value = 7 },
            new InstructionRegisterValue { registerId = "t2", value = 0 },
        };

        instruction.flowSteps = new[]
        {
            new InstructionFlowStep
            {
                stepName = "Fetch",
                highlightedNode = DatapathNodeId.ProgramCounter,
                explanation = "The Program Counter holds the address of the next instruction to fetch. Start the add walkthrough by reading the current PC value.",
                requiredInteraction = InstructionStepInteractionType.ContinueButton,
                blockProgressUntilValidated = true,
            },
            new InstructionFlowStep
            {
                stepName = "Instruction Memory",
                highlightedNode = DatapathNodeId.InstructionMemory,
                explanation = "Instruction Memory uses the PC address to fetch the add instruction. For this first pass, the breakdown is shown in the lesson UI instead of making you manually build the bit pattern.",
                requiredInteraction = InstructionStepInteractionType.ContinueButton,
                blockProgressUntilValidated = true,
            },
            new InstructionFlowStep
            {
                stepName = "Register Selection",
                highlightedNode = DatapathNodeId.Registers,
                explanation = "Choose the operand registers in order. Add reads rs and rt as inputs, and rd is the destination that will receive the ALU result.",
                requiredInteraction = InstructionStepInteractionType.RegisterSelection,
                blockProgressUntilValidated = true,
                requiredRegisterSelections = new[]
                {
                    InstructionRegisterRole.Rs,
                    InstructionRegisterRole.Rt,
                    InstructionRegisterRole.Rd,
                },
            },
            new InstructionFlowStep
            {
                stepName = "ALU Execute",
                highlightedNode = DatapathNodeId.ALU,
                explanation = "The ALU adds the values read from rs and rt. Because add is an R-type instruction, it does not use an immediate value and it does not touch Data Memory.",
                requiredInteraction = InstructionStepInteractionType.AluExecution,
                blockProgressUntilValidated = true,
            },
            new InstructionFlowStep
            {
                stepName = "Write Back",
                highlightedNode = DatapathNodeId.WriteBack,
                explanation = "The ALU result is written back to the destination register rd. Use the lesson guide to confirm the destination and final value before continuing.",
                requiredInteraction = InstructionStepInteractionType.ContinueButton,
                blockProgressUntilValidated = true,
            },
            new InstructionFlowStep
            {
                stepName = "Recap",
                highlightedNode = DatapathNodeId.WriteBack,
                explanation = "Recap: add fetched the instruction, read rs and rt from the register file, used the ALU to add them, skipped Data Memory entirely, and wrote the result back to rd.",
                requiredInteraction = InstructionStepInteractionType.Completion,
                blockProgressUntilValidated = false,
            },
        };

        return instruction;
    }
}
