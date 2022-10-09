using BonfireCore.Helpers;
using System.Runtime.InteropServices;

namespace BonfireCore.Models.BND4;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/BND4Header.cs
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
public class Bnd4Header
{
    /// <summary>
    /// BND4 EntryHeader. This should be the ANSI for "BND4", or 0x3444_4E42.
    /// </summary>
    public uint Type { get; set; } = 0x3444_4E42;

    /// <summary>
    /// Don't know what's here.
    /// </summary>
    public uint Magic1 { get; set; } = 0U;
    public uint Magic2 { get; set; } = 0x0001_0000U;

    /// <summary>
    /// Number of files/entries contained in the BND4 file.
    /// </summary>
    public uint FileCount { get; set; }

    /// <summary>
    /// Don't know what's here.
    /// </summary>
    public uint Magic3 { get; set; } = 0x0000_0040U;
    public uint Magic4 { get; set; } = 0U;

    /// <summary>
    /// Signature 00000001 = 0x3130_3030_3030_3030.
    /// </summary>
    public ulong Signature { get; set; } = 0x3130_3030_3030_3030;

    /// <summary>
    /// Size of the BND4 entry headers. Should be 32.
    /// </summary>
    public uint EntryHeaderSize { get; set; } = 0x20U;

    /// <summary>
    /// Don't know what's here.
    /// </summary>
    public uint Magic5 { get; set; } = 0U;

    /// <summary>
    /// Position in the file at which entry EntryData begins.
    /// </summary>
    public uint DataOffset { get; set; }

    /// <summary>
    /// Don't know what's here.
    /// </summary>
    public uint Magic6 { get; set; } = 0U;

    /// <summary>
    /// True if the entry names are Unicode wide chars
    /// </summary>
    [MarshalAs(UnmanagedType.I1)]
    public bool IsUnicode;

    /// <summary>
    /// I don't know what's here
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[]? Magic7 = "200000".HexStringToBytes();

    /// <summary>
    /// Create a parameter-less BND4Header.
    /// </summary>
    public Bnd4Header() { }

    /// <summary>
    /// Create a BND4Header with given parameters.
    /// </summary>
    /// <param name="fileCount"></param>
    /// <param name="dataOffset"></param>
    /// <param name="isUnicode"></param>
    public Bnd4Header(uint fileCount, uint dataOffset, bool isUnicode)
    {
        FileCount = fileCount;
        DataOffset = dataOffset;
        IsUnicode = isUnicode;
    }

    /// <summary>
    /// Returns false if this <see cref="Type"/> does not make sense.
    /// </summary>
    /// <returns></returns>
    public bool CheckIntegrity()
    {
        return Type == 0x3444_4E42;
    }
}