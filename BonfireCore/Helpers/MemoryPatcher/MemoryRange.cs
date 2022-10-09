namespace BonfireCore.Helpers.MemoryPatcher;

/// <summary>
/// Stores the fist and the last byte positions of a new memory range.
/// </summary>
public class MemoryRange
{
    /// <summary>
    /// Index number of the first byte of MemoryRange
    /// </summary>
    public long StartPosition { get; set; } = 0;

    /// <summary>
    /// Index number of the last byte of MemoryRange
    /// </summary>
    public long EndPosition { get; set; } = 0;
}