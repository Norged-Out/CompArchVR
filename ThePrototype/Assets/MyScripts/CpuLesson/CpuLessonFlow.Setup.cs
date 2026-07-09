using UnityEngine;

public partial class CpuLessonFlow
{
    void ApplyInitialRegisterValues()
    {
        if (m_RegisterBank == null)
            return;

        m_RegisterBank.ResetAllRegisterValues();

        var initialRegisterValues = m_CurrentInstruction?.initialRegisterValues;
        if (initialRegisterValues == null)
            return;

        foreach (var registerValue in initialRegisterValues)
        {
            if (registerValue == null || string.IsNullOrWhiteSpace(registerValue.registerId))
                continue;

            m_RegisterBank.SetRegisterValue(registerValue.registerId, registerValue.value);
        }
    }

    InstructionDefinition LoadDefaultInstruction()
    {
        var loadedInstruction = Resources.Load<InstructionDefinition>(m_DefaultInstructionResourcePath);
        return loadedInstruction != null ? loadedInstruction : InstructionDefaults.CreateFallbackAdd();
    }
}
