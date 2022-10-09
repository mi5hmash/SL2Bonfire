using System.Runtime.InteropServices;

namespace BonfireCore.Models.BND4;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/BND4EntryHeader.cs
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
public class Bnd4EntryHeader
{
    /// <summary>
    /// Padding EntryHeader (50 00 00 00 FF FF FF FF in little-endian)
    /// </summary>
    public ulong PaddingHeader { get; set; } = 0xFFFF_FFFF_0000_0050UL;

    /// <summary>
    /// Size (in bytes) of the entry EntryData.
    /// </summary>
    public uint EntrySize { get; set; }

    /// <summary>
    /// Don't know what's here.
    /// </summary>
    public uint Magic1 { get; set; } = 0U;

    /// <summary>
    /// Offset of the entry EntryData in the BND4 file.
    /// </summary>
    public uint EntryDataOffset { get; set; }

    /// <summary>
    /// Offset of the entry EntryName in the BND4 file.
    /// </summary>
    public uint EntryNameOffset { get; set; }

    /// <summary>
    /// Footer length.
    /// </summary>
    public uint EntryFooterLength { get; set; }

    /// <summary>
    /// Don't know what's here.
    /// </summary>
    public uint Magic2 { get; set; } = 0U;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Bnd4EntryHeader() { }

    /// <summary>
    /// Returns false if <see cref="PaddingHeader"/> does not make sense.
    /// </summary>
    /// <returns></returns>
    public bool CheckIntegrity()
    {
        return PaddingHeader == 0xFFFF_FFFF_0000_0050UL;
    }
}