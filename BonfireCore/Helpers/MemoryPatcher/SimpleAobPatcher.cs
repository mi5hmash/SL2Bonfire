using System.Diagnostics;
using Memory;

namespace BonfireCore.Helpers.MemoryPatcher;

/// <summary>
/// A simple memory patcher which uses AoB scans.
/// </summary>
public class SimpleAobPatcher
{
    /// <summary>
    /// A status reporter.
    /// </summary>
    public SimpleStatusReporter Reporter { get; set; } = new();

    /// <summary>
    /// A memory.dll module
    /// </summary>
    private readonly Mem _m = new();

    /// <summary>
    /// A private list of patches.
    /// </summary>
    private readonly List<PatchModel> _patches = new();
    
    /// <summary>
    /// Tries to open the <paramref name="processName"/> process and returns true if succeed.
    /// </summary>
    /// <param name="processName"></param>
    /// <returns></returns>
    public bool OpenProcess(string processName) => _m.OpenProcess(processName, out var failReason);

    /// <summary>
    /// Closes currently open process.
    /// </summary>
    public void CloseProcess() => _m.CloseProcess();

    /// <summary>
    /// Adds patch to the list.
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="searchPattern"></param>
    /// <param name="modifiedBytes"></param>
    /// <param name="placeOffset"></param>
    public void AddPatch(string moduleName, string searchPattern, byte[] modifiedBytes, int placeOffset = 0)
        => AddPatch(new MemoryRange(), searchPattern, modifiedBytes, moduleName, placeOffset);
    /// <summary>
    /// Adds patch to the list.
    /// </summary>
    /// <param name="memoryRange"></param>
    /// <param name="searchPattern"></param>
    /// <param name="modifiedBytes"></param>
    /// <param name="placeOffset"></param>
    public void AddPatch(MemoryRange memoryRange, string searchPattern, byte[] modifiedBytes, int placeOffset = 0)
        => AddPatch(new MemoryRange(), searchPattern, modifiedBytes, "", placeOffset);
    /// <summary>
    /// Adds patch to the list.
    /// </summary>
    /// <param name="memoryRange"></param>
    /// <param name="searchPattern"></param>
    /// <param name="modifiedBytes"></param>
    /// <param name="moduleName"></param>
    /// <param name="placeOffset"></param>
    private void AddPatch(MemoryRange memoryRange, string searchPattern, byte[] modifiedBytes, string moduleName = "", int placeOffset = 0)
    {
        _patches.Add(new PatchModel()
        {
            MemRange = memoryRange,
            SearchPattern = searchPattern,
            ModifiedBytes = modifiedBytes,
            PlaceOffset = placeOffset,
            ModuleName = moduleName
        });
    }

    /// <summary>
    /// Returns MemoryRange of current module.
    /// </summary>
    /// <returns></returns>
    public MemoryRange CurrentModuleMemoryRange()
    {
        var start = _m.mProc.MainModule.BaseAddress.ToInt64();
        var end = start + _m.mProc.MainModule.ModuleMemorySize;
        return CreateMemoryRange(start, end);
    }

    /// <summary>
    /// Returns MemoryRange of <paramref name="moduleName"/> module.
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public MemoryRange CreateMemoryRange(string moduleName)
    {
        var modules = _m.mProc.Process.Modules;
        var moduleIndex = modules.Cast<ProcessModule>().TakeWhile(module => module.ModuleName != moduleName).Count();
        if (moduleIndex >= modules.Count)
        {
            Reporter.Warning($"A module of the specified name '{moduleName}' could not be found. The range will be memory-spanning.");
            return CreateMemoryRange();
        }

        var start = _m.mProc.Process.Modules[moduleIndex].BaseAddress.ToInt64();
        var end = start + _m.mProc.Process.Modules[moduleIndex].ModuleMemorySize;
        return CreateMemoryRange(start, end);
    }
    /// <summary>
    /// Returns MemoryRange of position specified by <paramref name="startPosition"/> and <paramref name="endPosition"/>.
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <returns></returns>
    private static MemoryRange CreateMemoryRange(long startPosition = 0L, long endPosition = long.MaxValue)
    {
        return new MemoryRange
        {
            StartPosition = startPosition,
            EndPosition = endPosition
        };
    }

    /// <summary>
    /// Recalculates MemoryRange of <paramref name="patch"/>.
    /// </summary>
    /// <param name="patch"></param>
    private void RecalculateMemoryRange(PatchModel patch)
    {
        if ($"{patch.ModuleName}.exe".ToUpper() == _m.mProc.MainModule.ModuleName?.ToUpper()) patch.MemRange = CurrentModuleMemoryRange();
        else patch.MemRange = patch.ModuleName switch
        {
            "" => CreateMemoryRange(),
            _ => CreateMemoryRange(patch.ModuleName)
        };
    }

    /// <summary>
    /// Tries to patch process memory using a set of patches.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Patch()
    {
        var i = 0;
        foreach (var patch in _patches)
        {
            RecalculateMemoryRange(patch);
            
            if (patch.ModifiedBytes.Length == 0) continue; // check for an empty value.
            var aob = (await _m.AoBScan(patch.MemRange.StartPosition, patch.MemRange.EndPosition, patch.SearchPattern, true)).FirstOrDefault();
            if (aob < 1) continue;
            patch.Place = aob + patch.PlaceOffset;
            patch.StoredBytes = _m.ReadBytes(patch.Place.ToString("X"), patch.ModifiedBytes.Length);
            _m.WriteBytes(patch.Place.ToString("X"), patch.ModifiedBytes);
            i++;
            patch.IsApplied = true;
        }

        if (i == 0)
        {
            Reporter.Error("None of the patches has been applied.");
            return false;
        }
        if (i >= _patches.Count) { Reporter.Success("All patches has been applied successfully."); }
        else { Reporter.Warning($"Only {i} out of {_patches.Count} patches has been applied."); }
        return true;
    }

    /// <summary>
    /// Tries to unpatch process memory using a set of patches.
    /// </summary>
    /// <returns></returns>
    public bool Unpatch()
    {
        var i = 0;
        foreach (var patch in _patches.Where(patch => patch.IsApplied))
        {
            if (patch.StoredBytes is null) continue; // check for null value.
            _m.WriteBytes(patch.Place.ToString("X"), patch.StoredBytes);
            i++;
            patch.IsApplied = false;
        }

        if (i == 0)
        {
            Reporter.Error("None of the patches has been reverted.");
            return false;
        }
        if (i >= _patches.Count) { Reporter.Success("All patches has been reverted successfully."); }
        else { Reporter.Warning($"Only {i} out of {_patches.Count} patches has been reverted."); }
        return true;
    }
}