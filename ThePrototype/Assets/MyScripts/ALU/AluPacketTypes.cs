/// <summary>
/// Logical packet kinds that can move between datapath stages in the current
/// prototype.
/// </summary>
public enum DataPacketRole
{
    None,
    ReadData1,
    ReadData2,
    Immediate,
    AluResult,
    MemoryData,
}
