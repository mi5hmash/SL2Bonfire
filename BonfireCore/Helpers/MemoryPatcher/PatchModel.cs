namespace BonfireCore.Helpers.MemoryPatcher;

/// <summary>
/// A model of a Patch
/// </summary>
public class PatchModel
{
    /// <summary>
    /// A pattern of bytes and question marks for use with AOBScan.
    /// </summary>
    /// <example>"D3 ?? B? 3F"</example>
    public string SearchPattern { get; set; } = "";

    /// <summary>
    /// Stores modified bytes.
    /// </summary>
    public byte[] ModifiedBytes { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// An offset in pattern pointing to the position of the first byte to be modified.
    /// </summary>
    public int PlaceOffset { get; set; }

    /// <summary>
    /// Stores original bytes.
    /// </summary>
    public byte[]? StoredBytes { get; set; }

    /// <summary>
    /// The fist and the last byte positions of a memory range.
    /// </summary>
    public MemoryRange MemRange { get; set; } = new();

    /// <summary>
    /// True if patch is applied
    /// </summary>
    public bool IsApplied { get; set; } = false;

    /// <summary>
    /// Position of the first byte to be modified.
    /// </summary>
    public long Place { get; set; }

    /// <summary>
    /// A name of the module which should be scanned.
    /// </summary>
    public string ModuleName { get; set; } = "";
}