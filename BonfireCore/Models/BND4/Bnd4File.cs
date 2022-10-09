using BonfireCore.Helpers;
using System.Runtime.InteropServices;

namespace BonfireCore.Models.BND4;

public class Bnd4File
{
    /// <summary>
    /// Bnd4 file EntryHeader.
    /// </summary>
    public Bnd4Header Header { get; set; } = new();

    /// <summary>
    /// The EntriesNames data section.
    /// </summary>
    private byte[] EntriesNames { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The EntriesNames offset.
    /// </summary>
    private uint EntriesNamesOffset { get; set; }

    /// <summary>
    /// The entries of the Bnd4 file.
    /// </summary>
    public Bnd4Entry[] Entries { get; set; } = Array.Empty<Bnd4Entry>();

    /// <summary>
    /// A list containing paths to the entries.
    /// </summary>
    public string[] EntriesPaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// A paths to '*.sl2' archive.
    /// </summary>
    private string Sl2Path { get; set; } = "";

    /// <summary>
    /// Create an empty Bnd4File class.
    /// </summary>
    public Bnd4File()
    { }

    /// <summary>
    /// Load an '*.sl2' archive into the existing object, overwriting existing <see cref="Entries"/>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool LoadSl2File(string filePath)
    {
        Sl2Path = filePath;
        FileStream stream;
        try
        {
            stream = File.OpenRead(Sl2Path);
        }
        catch { return false; }

        using BinReader br = new(stream);
        try
        {
            // try to load header data into the Header
            Header = br.ReadStruct<Bnd4Header>() ?? throw new NullReferenceException();
        }
        catch { return false; }
        if (!Header.CheckIntegrity()) return false; 

        // overwrite Entries collection
        Entries = new Bnd4Entry[Header.FileCount];
        for (var i = 0; i < Header.FileCount; i++)
        {
            Bnd4EntryHeader entryHeader;
            try
            {
                entryHeader = br.ReadStruct<Bnd4EntryHeader>() ?? throw new NullReferenceException();
            }
            catch { return false; }
            if (!entryHeader.CheckIntegrity()) return false;

            br.StepInto(entryHeader.EntryNameOffset);
            var eName = Header.IsUnicode ? br.ReadWideString() : br.ReadShiftJis();
            br.StepOut();

            br.StepInto(entryHeader.EntryDataOffset);
            var eData = br.ReadBytes((int)entryHeader.EntrySize);
            br.StepOut();

            Entries[i] = new Bnd4Entry(eName, eData)
            {
                EntryHeader = entryHeader
            };

            Entries[i].UnsignEntryData();
        }
        return true;
    }

    /// <summary>
    /// Load files (entries) into the existing object, overwriting existing <see cref="Entries"/>.
    /// </summary>
    /// <returns></returns>
    public bool LoadEntries()
    {
        // check if EntriesPaths array is empty
        if (EntriesPaths.Length == 0) return false;

        // set file count
        Header.FileCount = (uint)EntriesPaths.Length;

        // create Entries collection
        Entries = new Bnd4Entry[EntriesPaths.Length];
        for (var i = 0; i < EntriesPaths.Length; i++)
        {
            // set entry Name
            var eName = Path.GetFileName(EntriesPaths[i]);
            // read entry Data
            var eData = File.ReadAllBytes(EntriesPaths[i]);
            Entries[i] = new Bnd4Entry(eName, eData);
        }
        return true;
    }

    /// <summary>
    /// Recalculate the Bnd4Headers of the elements in the <see cref="Entries"/> array.
    /// </summary>
    public void RecalculateHeaders()
    {
        // Create NAMES section
        // get an offset of EntryNames Section
        EntriesNamesOffset = (uint)(Marshal.SizeOf<Bnd4Header>() + Entries.Length * Marshal.SizeOf<Bnd4EntryHeader>());
        // create an EntriesNames section
        using (var ms = new MemoryStream())
        using (var bw = new BinWriter(ms))
        {
            foreach (var t in Entries)
            {
                // update an EntryNameOffset
                t.EntryHeader.EntryNameOffset = (uint)(EntriesNamesOffset + ms.Position);
                if (Header.IsUnicode)
                {
                    bw.WriteWideString(t.EntryName);
                }
                else
                {
                    bw.WriteShiftJis(t.EntryName);
                }
            }
            var entryDataNames = ms.ToArray();
            Array.Resize(ref entryDataNames, entryDataNames.Length + (16 - entryDataNames.Length % 16));
            EntriesNames = entryDataNames;
        }
        
        // Recalculate HEADERS
        // update DataOffset
        var currentDataOffset = (uint)(EntriesNamesOffset + EntriesNames.Length);
        Header.DataOffset = currentDataOffset;

        // update each EntryDataOffset
        foreach (var entry in Entries)
        {
            entry.SignEntryData();
            entry.EntryHeader.EntryDataOffset = currentDataOffset;
            currentDataOffset += entry.EntryHeader.EntrySize + entry.EntryHeader.EntryFooterLength;
        }
    }

    /// <summary>
    /// Update Bnd4Headers and save the '*.sl2' archive.
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveSl2File(string filePath)
    {
        RecalculateHeaders();
        
        using BinWriter bw = new(File.OpenWrite(filePath));
        //Write Bnd4 HEADER
        bw.WriteStruct(Header);
        //Write Bnd4 ENTRIES NAMES
        bw.StepInto(EntriesNamesOffset);
        bw.Write(EntriesNames);
        bw.StepOut();

        foreach (var entry in Entries)
        {
            // Write ENTRY HEADER
            bw.WriteStruct(entry.EntryHeader);
            // Write ENTRY DATA
            bw.StepInto(entry.EntryHeader.EntryDataOffset);
            bw.Write(entry.EntryData);
            bw.StepOut();
        }
    }
}