using BonfireCore.Helpers;
using BonfireCore.Helpers.MemoryPatcher;
using BonfireCore.Models.BND4;
using System.Text;
using System.Text.Json;
using static BonfireCore.Helpers.IoHelpers;

namespace BonfireCore.Models;

public class GameProfile
{
    private const string Magic = @"B7R?Gg9AP?-9D%v[E$5a*S\8UTojHN4!wMoUna(gNB3?'[<1eZap+UXO%B']}NC55an^w";
    private const string UnknownSteamId = "unknown";

    /// <summary>
    /// A status reporter.
    /// </summary>
    public SimpleStatusReporter Reporter { get; set; } = new();

    /// <summary>
    /// The title of a game.
    /// </summary>
    public string GameTitle { get; private set; } = "";

    /// <summary>
    /// A default path to a Save Data folder.
    /// </summary>
    public string SaveFolderPath { get; private set; } = "";

    /// <summary>
    /// A file name for the '*.sl2' archive.
    /// </summary>
    public string FileName { get; private set; } = "";

    /// <summary>
    /// An Encryption Key used to de/encrypt '*.sl2' archive. 
    /// </summary>
    public byte[]? SaveEncryptionKey { get; private set; }

    /// <summary>
    /// Bnd4Header Signature of length of 8. f.ex. 00000001.
    /// </summary>
    public ulong Bnd4HeaderSignature { get; private set; }

    /// <summary>
    /// The name of a process.
    /// </summary>
    public string? ProcessName { get; private set; }

    /// <summary>
    /// An instance of AobPatcher.
    /// </summary>
    public SimpleAobPatcher? AobPatcher { get; private set; }

    /// <summary>
    /// An int array with Available options
    /// </summary>
    public int[] AvailableActions { get; private set; } = Array.Empty<int>();

    /// <summary>
    /// A structure of UserData file.
    /// </summary>
    public int UserDataFileStructure { get; set; }

    /// <summary>
    /// A number of UserData file which contains info about ownership (SteamID) and Save Game Slots.
    /// </summary>
    public int UserDataFileNumber { get; set; }

    /// <summary>
    /// An offset where the Save Game Slots data begin. First n bytes is equal to the Game Slots. 01 stands for occupied.
    /// </summary>
    public int? SlotsOccupancyOffset { get; set; }

    /// <summary>
    /// A length of one slot in bytes.
    /// </summary>
    public int? SlotLength { get; set; }

    /// <summary>
    /// The gap between slots occupancy bytes and the first slot data expressed in bytes.
    /// </summary>
    public int? SlotsDataOffset { get; set; }

    /// <summary>
    /// An offset where the 8-bytes length SteamID begins.
    /// </summary>
    public int? SteamIdOffsetInUserDataFile { get; set; }

    /// <summary>
    /// A maximum length of character's name in chars.
    /// </summary>
    public int? MaxCharacterNameLength { get; set; }

    /// <summary>
    /// True if the loaded configuration is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// If true then save file names are encoded with unicode.
    /// </summary>
    public bool IsUnicode { get; set; } = true;

    /// <summary>
    /// True if game's memory is patched.
    /// </summary>
    public bool IsMemoryPatched { get; set; }
    
    /// <summary>
    /// Shared instance of Bnd4File.
    /// </summary>
    public Bnd4File Bnd4 { get; set; } = new();

    /// <summary>
    /// An array containing all character slots from the currently loaded SaveData.
    /// </summary>
    public CharacterSlot[] CharacterSlots { get; set; } = Array.Empty<CharacterSlot>();

    /// <summary>
    /// Tries to load profile from the input file.
    /// </summary>
    /// <param name="filePath"></param>
    public void LoadProfile(string filePath) => IsValid = LoadProfileInternal(filePath);

    /// <summary>
    /// Tries to load profile from the input file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Success status flag.</returns>
    private bool LoadProfileInternal(string filePath)
    {
        // Try to read file.
        GameProfileJson gpJson;
        try
        {
            var jsonData = ReadFile(filePath).Decrypto(Magic);
            gpJson = JsonSerializer.Deserialize<GameProfileJson>(jsonData)!;
        }
        catch
        {
            Reporter.Error("Game profile couldn't be loaded. Invalid file.");
            return false;
        }

        var errorMsg = "Game profile couldn't be loaded. Invalid values.";
        // Check if any of important properties is null.
        if (gpJson.AvailableActions is null || gpJson.Bnd4HeaderSignature is null || gpJson.FileName is null || gpJson.GameTitle is null || gpJson.SaveFolderPath is null || gpJson.UserDataFileNumber is null || gpJson.UserDataFileStructure is null)
        { Reporter.Error(errorMsg); return false; }

        var isDeencryptingAllowed = gpJson.AvailableActions.Contains((int)ActionsEnum.Deencrypting);
        if (isDeencryptingAllowed)
            if (gpJson.SaveEncryptionKey is null) { Reporter.Error(errorMsg); return false; }

        var isResigningAllowed = gpJson.AvailableActions.Contains((int)ActionsEnum.ResigningSaveFile);
        if (isResigningAllowed)
            if (gpJson.SteamIdOffsetInUserDataFile is null) { Reporter.Error(errorMsg); return false; }

        var isPatchingMemoryAllowed = gpJson.AvailableActions.Contains((int) ActionsEnum.PatchingGameMemory);
        if (isPatchingMemoryAllowed)
            if (gpJson.Patches is null || gpJson.ProcessName is null) { Reporter.Error(errorMsg); return false; }

        var isCharacterRenamingAllowed = gpJson.AvailableActions.Contains((int)ActionsEnum.CharacterRenaming);
        if (isCharacterRenamingAllowed)
            if (gpJson.MaxCharacterNameLength is null || gpJson.SlotsOccupancyOffset is null || gpJson.SlotLength is null || gpJson.SlotsDataOffset is null) { Reporter.Error(errorMsg); return false; }

        // Load rest of the config to the current object.
        AvailableActions = gpJson.AvailableActions;
        Bnd4HeaderSignature = gpJson.Bnd4HeaderSignature == "" ? 0 : BitConverter.ToUInt64(gpJson.Bnd4HeaderSignature.FromAsciiString());
        FileName = gpJson.FileName;
        GameTitle = gpJson.GameTitle;
        SaveFolderPath = Environment.ExpandEnvironmentVariables(gpJson.SaveFolderPath);
        UserDataFileNumber = (int)gpJson.UserDataFileNumber;
        UserDataFileStructure = (int)gpJson.UserDataFileStructure;
        if (isDeencryptingAllowed)
        {
            SaveEncryptionKey = (gpJson.SaveEncryptionKey ?? string.Empty).HexStringToBytes();
        }
        if (isResigningAllowed)
        {
            SteamIdOffsetInUserDataFile = gpJson.SteamIdOffsetInUserDataFile;
        }
        if (isPatchingMemoryAllowed)
        {
            // Load config part related with memory patching.
            AobPatcher = new SimpleAobPatcher();
            ProcessName = gpJson.ProcessName ?? string.Empty;
            foreach (var patchStr in gpJson.Patches!)
            {
                var arr = patchStr.Split("|");
                if (arr.Length != 4) continue; // check if the number of arguments is valid
                AobPatcher.AddPatch(arr[0], arr[1], arr[2].HexStringToBytes(), Convert.ToInt32(arr[3]));
            }
        }
        if (isCharacterRenamingAllowed)
        {
            MaxCharacterNameLength = gpJson.MaxCharacterNameLength;
            SlotsOccupancyOffset = gpJson.SlotsOccupancyOffset;
            SlotLength = gpJson.SlotLength;
            SlotsDataOffset = gpJson.SlotsDataOffset;
        }
        Reporter.Success("Game profile successfully loaded."); 
        return true;
    }

    /// <summary>
    /// Enumeration of all available actions.
    /// </summary>
    public enum ActionsEnum
    {
        Repacking = 1,
        Deencrypting = 2,
        ResigningSaveFile = 3,
        PatchingGameMemory = 4,
        CharacterRenaming = 5
    }

    /// <summary>
    /// Enumeration of all possible UserData file structures.
    /// </summary>
    public enum UserDataFileStructures
    {
        Unified = 0,
        Ds2 = 1
    }

    /// <summary>
    /// Saves entry from SL2 archive into a new file.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="outputDir"></param>
    public static void SaveEntry(Bnd4Entry entry, string outputDir)
        => File.WriteAllBytes(Path.Combine(outputDir, entry.EntryName), entry.EntryData);

    /// <summary>
    /// Loads entry files from <paramref name="inputDir"/> folder.
    /// </summary>
    /// <param name="inputDir"></param>
    public bool LoadEntries(string inputDir)
    {
        Directory.CreateDirectory(inputDir);
        // Load Entry Files
        Bnd4 = new Bnd4File
        {
            Header =
            {
                IsUnicode = IsUnicode,
                Signature = Bnd4HeaderSignature
            },
            EntriesPaths = Directory.GetFiles(inputDir)
        };
        if (Bnd4.EntriesPaths.Length == 0)
        {
            Reporter.Error("No files in the input directory.");
            return false;
        }
        Bnd4.LoadEntries();

        Reporter.Success("Entries successfully loaded.");
        return true;
    }

    /// <summary>
    /// Unpacks files from an '*.sl2' archive into specified folder.
    /// </summary>
    /// <param name="inputFile"></param>
    /// <param name="outputDir"></param>
    /// <param name="decrypt"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Unpack(string inputFile, string outputDir, bool decrypt)
    {
        // recreate output directory
        if (!RecreateDirectory(outputDir) || !File.Exists(inputFile)) { Reporter.Error("Couldn't recreate output directory."); return false; }

        // LoadSl2File
        Bnd4 = new Bnd4File();
        if (!Bnd4.LoadSl2File(inputFile)) { Reporter.Error("Couldn't load SL2 file."); return false; }

        IsUnicode = Bnd4.Header.IsUnicode; // update IsUnicode flag

        //Unpack DATA
        foreach (var entry in Bnd4.Entries)
        {
            if (decrypt) entry.DecryptEntryData(SaveEncryptionKey ?? throw new ArgumentNullException(nameof(SaveEncryptionKey), "Value can't be null!")); // decrypt if needed
            // write EntryData into a file for each entry
            SaveEntry(entry, outputDir);
        }

        Reporter.Success("SL2 file successfully unpacked.");
        return true;
    }
    
    /// <summary>
    /// Makes an '*.sl2' archive based on the loaded entry files.
    /// </summary>
    /// <param name="outputDir"></param>
    /// <param name="encrypt"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Pack(string outputDir, bool encrypt)
    {
        if (!RecreateDirectory(outputDir)) { Reporter.Error("Couldn't recreate output directory."); return false; }
        if (Bnd4.Entries.Length == 0) { Reporter.Error("Couldn't create SL2 archive. Entries are empty."); return false; }

        // Pack DATA
        if (encrypt)
        {
            foreach (var entry in Bnd4.Entries)
            {
                entry.EncryptEntryData(SaveEncryptionKey ?? throw new ArgumentNullException(nameof(SaveEncryptionKey), "Value can't be null!"));
            }
        }
        // Save Sl2File
        Bnd4.SaveSl2File(Path.Combine(outputDir, FileName));
        Reporter.Success("Entries successfully packed into SL2 file.");
        return true;
    }

    /// <summary>
    /// Patches or Unpatches game's memory.
    /// </summary>
    /// <returns></returns>
    public async Task PatchMemory()
    {
        if (AobPatcher is null || ProcessName is null || !AobPatcher.OpenProcess(ProcessName)) { Reporter.Error("Failed to open the game process."); return; }
        AobPatcher.Reporter = Reporter;
        IsMemoryPatched = IsMemoryPatched ? !AobPatcher.Unpatch() : await AobPatcher.Patch();
    }

    /// <summary>
    /// Reads SteamID64 from currently loaded UserData files using adequate method.
    /// </summary>
    /// <returns>SteamID64 from UserData file.</returns>
    public string GetSteamId64()
    {
        var isResigningAllowed = AvailableActions.Contains((int)ActionsEnum.ResigningSaveFile);
        if (SteamIdOffsetInUserDataFile == null || !isResigningAllowed || Bnd4.Entries.Length < 1) return UnknownSteamId;
        var answer = (UserDataFileStructures) UserDataFileStructure switch
        {
            UserDataFileStructures.Unified => GetSteamId64Unified(),
            UserDataFileStructures.Ds2 => GetSteamId64Ds2(),
            _ => string.Empty
        };
        return answer;
    }
    private string GetSteamId64Unified()
    {
        using MemoryStream ms = new();
        ms.Write(Bnd4.Entries[UserDataFileNumber].EntryData, (int)SteamIdOffsetInUserDataFile!, 8);
        return BitConverter.ToInt64(ms.ToArray()).ToString();
    }
    private string GetSteamId64Ds2()
    {
        using MemoryStream ms = new();
        ms.Write(Bnd4.Entries[UserDataFileNumber].EntryData, (int)SteamIdOffsetInUserDataFile!, 16);
        var bytes = Encoding.ASCII.GetString(ms.ToArray()).HexStringToBytes().Reverse().ToArray();
        return BitConverter.ToInt64(bytes).ToString();
    }

    /// <summary>
    /// Patches SteamID in currently loaded UserData file using adequate method.
    /// </summary>
    /// <param name="steamId64"></param>
    /// <param name="outputDir"></param>
    /// <returns></returns>
    public async Task<bool> PatchSteamIdInUnpackedFiles(string steamId64, string outputDir)
    {
        // validate crucial variables
        if (SteamIdOffsetInUserDataFile is null) { Reporter.Error("Invalid SteamID offset."); return false; }
        if (steamId64.Length > 17) { Reporter.Error("Invalid SteamID length."); return false; }
        // validate external SteamID
        var steamIdExternal = GetSteamId64();
        if (steamIdExternal == UnknownSteamId) { Reporter.Error("Invalid SteamID in UserData File."); return false; }
        var answer = (UserDataFileStructures)UserDataFileStructure switch
        {
            UserDataFileStructures.Unified => await PatchSteamIdInUnpackedFilesUnified(steamId64, outputDir, steamIdExternal),
            UserDataFileStructures.Ds2 => await Task.Run(() => PatchSteamIdInUnpackedFilesDs2(steamId64, outputDir)),
            _ => false
        };
        return answer;
    } 
    private async Task<bool> PatchSteamIdInUnpackedFilesUnified(string steamId64, string outputDir, string steamIdExternal)
    {
        // create pattern
        // I'm pretty sure that in this scenario, steamID is always surrounded by one 00 byte on both sides.
        // This minimizes the risk of matching false positives.
        var steamIdExternalBytes = BitConverter.GetBytes(Convert.ToInt64(steamIdExternal));
        using MemoryStream ms = new();
        ms.Write(new byte[1]);
        ms.Write(steamIdExternalBytes, 0, steamIdExternalBytes.Length);
        ms.Write(new byte[1]);
        var pattern = ms.ToArray();

        // patch other dataEntries before userDataEntry
        var steamIdInternal = Convert.ToInt64(steamId64);
        for (var i = 0; i < UserDataFileNumber; i++)
        {
            if (CharacterSlots.Length > 0 && !CharacterSlots[i].IsOccupied) continue;
            var list = await Bnd4.Entries[i].EntryData.FindOffsetAsync(pattern);
            if (list.Count < 1) continue;
            list[0] += 1;
            await PatchEntry(Bnd4.Entries[i].EntryData, new[] { list[0] }, steamIdInternal);
        }

        // patch userDataEntry
        var userDataEntry = Bnd4.Entries[UserDataFileNumber].EntryData;
        await PatchEntry(userDataEntry, new[] { (int)SteamIdOffsetInUserDataFile! }, steamIdInternal);
        var list2 = await userDataEntry.FindOffsetAsync(pattern);
        if (list2.Count > 0)
        {
            list2[0] += 1;
            await PatchEntry(userDataEntry, new[] { list2[0] }, steamIdInternal);
        }

        // save entry files
        if (!RecreateDirectory(outputDir)) { Reporter.Error("Couldn't recreate output directory."); return false; }
        foreach (var entry in Bnd4.Entries)
        {
            SaveEntry(entry, outputDir);
        }
        
        Reporter.Success("Entries successfully resigned.");
        return true;
    }
    private bool PatchSteamIdInUnpackedFilesDs2(string steamId64, string outputDir)
    {
        var steamId64Bytes = Encoding.ASCII.GetBytes(BitConverter.GetBytes(Convert.ToInt64(steamId64)).Reverse().ToArray().BytesToHexString().ToLower());
        using MemoryStream ms = new(Bnd4.Entries[UserDataFileNumber].EntryData);
        using BinWriter bw = new(ms);
        bw.StepInto((int)SteamIdOffsetInUserDataFile!);
        bw.Write(steamId64Bytes);
        bw.StepOut();
        // save entry file
        SaveEntry(Bnd4.Entries[UserDataFileNumber], outputDir);
        return true;
    }

    /// <summary>
    /// Creates new list of <see cref="CharacterSlots"/>.
    /// </summary>
    public void GetCharacterSlots()
    {
        CharacterSlots = Array.Empty<CharacterSlot>();
        if (Bnd4.Entries.Length < 1 || SlotsOccupancyOffset is null || SlotsDataOffset is null || SlotLength is null) return;
        switch ((UserDataFileStructures)UserDataFileStructure)
        {
            case UserDataFileStructures.Unified:
                GetCharacterSlotsUnified();
                break;
            case UserDataFileStructures.Ds2:
                GetCharacterSlotsDs2();
                break;
        }
    }
    private void GetCharacterSlotsUnified()
    {
        var entryData = Bnd4.Entries[UserDataFileNumber].EntryData;
        using MemoryStream ms = new(entryData);
        using BinReader br = new(ms);
        br.StepInto((int)SlotsOccupancyOffset!);
        var slotsOccupancy = br.ReadBytes(UserDataFileNumber); // get info about slot occupancy
        br.StepOut();
        if (slotsOccupancy.Any(x => x > 1)) return;
        
        CharacterSlots = new CharacterSlot[UserDataFileNumber];
        var i = 0;
        foreach (var slot in slotsOccupancy)
        {
            var isOccupied = Convert.ToBoolean(slot);
            var currentOffset = (int)SlotsDataOffset! + i * (int)SlotLength!;
            CharacterSlots[i] = new CharacterSlot(i + 1, isOccupied, currentOffset);

            br.StepInto(currentOffset);
            var characterName = IsUnicode ? br.ReadWideString() : br.ReadShiftJis();
            br.StepOut();

            if (isOccupied) CharacterSlots[i].CharacterName = characterName;
            i++;
        }
    }
    private void GetCharacterSlotsDs2()
    {
        var entryData = Bnd4.Entries[UserDataFileNumber].EntryData;
        // get info about slot occupancy
        var slotsOccupancy = new bool[10];
        for (var i = 0; i < 10; i++)
        {
            slotsOccupancy[i] = entryData[(int)SlotsOccupancyOffset! + i * (int)SlotLength!] == 0x4c;
        }
        if (slotsOccupancy.All(x => x != true)) return;

        CharacterSlots = new CharacterSlot[slotsOccupancy.Length];
        using MemoryStream ms = new(entryData);
        using BinReader br = new(ms);
        var j = 0;
        foreach (var slot in slotsOccupancy)
        {
            var currentOffset = (int)SlotsDataOffset! + j * (int)SlotLength!;
            CharacterSlots[j] = new CharacterSlot(j + 1, slot, currentOffset);

            br.StepInto(currentOffset);
            var characterName = IsUnicode ? br.ReadWideString() : br.ReadShiftJis();
            br.StepOut();

            if (slot) CharacterSlots[j].CharacterName = characterName;
            j++;
        }
    }

    /// <summary>
    /// Changes character names in currently loaded UserData file using adequate method.
    /// </summary>
    /// <param name="newData"></param>
    /// <param name="outputDir"></param>
    /// <returns></returns>
    public async Task<bool> ChangeCharactersNames(CharacterSlot[] newData, string outputDir)
    {
        if (newData.Length != CharacterSlots.Length) { Reporter.Error("Mismatch in length of arrays."); return false; }
        var answer = (UserDataFileStructures)UserDataFileStructure switch
        {
            UserDataFileStructures.Unified => await ChangeCharactersNamesUnified(newData),
            UserDataFileStructures.Ds2 => await ChangeCharactersNamesDs2(newData),
            _ => false
        };
        if (!answer) return false;
        // save entry files
        if (!RecreateDirectory(outputDir)) { Reporter.Error("Couldn't recreate output directory."); return false; }
        foreach (var entry in Bnd4.Entries)
        {
            SaveEntry(entry, outputDir);
        }
        Reporter.Success("Characters' names successfully changed.");
        return true;
    }
    private async Task<bool> ChangeCharactersNamesUnified(IReadOnlyList<CharacterSlot> newData)
    {
        for (var i = 0; i < newData.Count; i++)
        {
            if (!CharacterSlots[i].IsOccupied) continue;
            if (CharacterSlots[i].CharacterName == newData[i].CharacterName) continue;
            
            var oldName = WriteName(CharacterSlots[i].CharacterName);
            var newName = WriteName(newData[i].CharacterName, (MaxCharacterNameLength+1) * 2);

            var list = await Bnd4.Entries[i].EntryData.FindOffsetAsync(oldName);
            switch (list.Count)
            {
                case < 1:
                    Reporter.Error($"Couldn't find character name in {Bnd4.Entries[i].EntryName}."); return false;
                case > 2:
                    Reporter.Error($"There are too many matches in {Bnd4.Entries[i].EntryName}."); return false;
                default:
                    await PatchEntry(Bnd4.Entries[i].EntryData, list , newName);
                    await PatchEntry(Bnd4.Entries[UserDataFileNumber].EntryData, new[] { CharacterSlots[i].Offset }, newName);
                    break;
            }
        }
        return true;
    }
    private async Task<bool> ChangeCharactersNamesDs2(IReadOnlyList<CharacterSlot> newData)
    {
        for (var i = 0; i < newData.Count; i++)
        {
            if (!CharacterSlots[i].IsOccupied) continue;
            if (CharacterSlots[i].CharacterName == newData[i].CharacterName) continue;

            var oldName = WriteName(CharacterSlots[i].CharacterName);
            var newName = WriteName(newData[i].CharacterName, (MaxCharacterNameLength + 1) * 2);

            var list = await Bnd4.Entries[i + 1].EntryData.FindOffsetAsync(oldName);
            switch (list.Count)
            {
                case < 1:
                    Reporter.Error($"Couldn't find character name in {Bnd4.Entries[i + 1].EntryName}."); return false;
                case > 2:
                    Reporter.Error($"There are too many matches in {Bnd4.Entries[i + 1].EntryName}."); return false;
                default:
                    await PatchEntry(Bnd4.Entries[i + 1].EntryData, new[] { list[0] }, newName);
                    break;
            }
            list = await Bnd4.Entries.Last().EntryData.FindOffsetAsync(oldName);
            switch (list.Count)
            {
                case < 1:
                    Reporter.Error($"Couldn't find character name in {Bnd4.Entries.Last().EntryName}."); return false;
                case > 2:
                    Reporter.Error($"There are too many matches in {Bnd4.Entries.Last().EntryName}."); return false;
                default:
                    await PatchEntry(Bnd4.Entries.Last().EntryData, new[] { list[0] }, newName);
                    await PatchEntry(Bnd4.Entries[UserDataFileNumber].EntryData, new[] { CharacterSlots[i].Offset }, newName);
                    break;
            }
        }
        return true;
    }

    private byte[] WriteName(string characterName, int? len = null)
    {
        using MemoryStream ms = new();
        using BinWriter bw = new(ms);
        if (IsUnicode)
        {
            bw.WriteWideString(characterName);
        }
        else bw.WriteShiftJis(characterName);

        len ??= (int)ms.Length;
        var buff = new byte[(int)len];
        ms.Position = 0;
        var readLength = ms.Read(buff, 0, buff.Length);
        return buff;
    }

    private static async Task PatchEntry(byte[] source, IEnumerable<int> offsets, long value)
    {
        using MemoryStream ms = new(source);
        await using BinWriter bw = new(ms);
        foreach (var offset in offsets)
        {
            bw.StepInto(offset);
            bw.Write(value);
            bw.StepOut();
        }
    }
    private static async Task PatchEntry(byte[] source, IEnumerable<int> offsets, byte[] value)
    {
        using MemoryStream ms = new(source);
        await using BinWriter bw = new(ms);
        foreach (var offset in offsets)
        {
            bw.StepInto(offset);
            bw.Write(value);
            bw.StepOut();
        }
    }
}