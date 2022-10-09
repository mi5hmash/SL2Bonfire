namespace BonfireCore.Models;

public class GameProfileJson
{
    /// <summary>
    /// An int array with Available options
    /// </summary>
    public int[]? AvailableActions { get; set; }

    /// <summary>
    /// The title of a game.
    /// </summary>
    public string? GameTitle { get; set; }

    /// <summary>
    /// A default path to a SaveData folder.
    /// </summary>
    public string? SaveFolderPath { get; set; }

    /// <summary>
    /// A file name for the '*.sl2' archive.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// An Encryption Key used to de/encrypt an '*.sl2' archive. 
    /// </summary>
    public string? SaveEncryptionKey { get; set; }

    /// <summary>
    /// Bnd4Header Signature of length of 8.
    /// </summary>
    public string? Bnd4HeaderSignature { get; set; }

    /// <summary>
    /// The name of a process.
    /// </summary>
    public string? ProcessName { get; set; }

    /// <summary>
    /// A string representation of the patches to apply.
    /// </summary>
    public string[]? Patches { get; set; }

    /// <summary>
    /// A structure of UserData file.
    /// </summary>
    public int? UserDataFileStructure { get; set; }

    /// <summary>
    /// A number of UserData file which contains info about ownership (SteamID) and Save Game Slots.
    /// </summary>
    public int? UserDataFileNumber { get; set; }

    /// <summary>
    /// An offset where the 8-bytes length SteamID begins.
    /// </summary>
    public int? SteamIdOffsetInUserDataFile { get; set; }

    /// <summary>
    /// An offset where the SaveGame Slots Occupancy data begins. First n bytes is equal to the Game Slots. 01 stands for occupied.
    /// </summary>
    public int? SlotsOccupancyOffset { get; set; }

    /// <summary>
    /// An offset where the SaveGame Slots data begins.
    /// </summary>
    public int? SlotsDataOffset { get; set; }

    /// <summary>
    /// A length of one slot in bytes.
    /// </summary>
    public int? SlotLength { get; set; }

    /// <summary>
    /// A maximum length of character's name in chars.
    /// </summary>
    public int? MaxCharacterNameLength { get; set; }
}