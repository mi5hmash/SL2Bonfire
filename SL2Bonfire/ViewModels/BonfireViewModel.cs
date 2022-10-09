using BonfireCore.Helpers;
using BonfireCore.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SL2Bonfire.Controls;
using SL2Bonfire.Helpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;
using static BonfireCore.Models.GameProfile;
using static Wpf.Ui.Common.ControlAppearance;
using Clipboard = System.Windows.Clipboard;
using Path = System.IO.Path;

namespace SL2Bonfire.ViewModels;

public partial class BonfireViewModel : ObservableObject, INavigationAware
{
    // Paths
    private string _unpackedFilesPath = Path.Combine(AppInfo.RootPath, AppInfo.UnpackedFilesFolder);
    private string _packedFilesPath = Path.Combine(AppInfo.RootPath, AppInfo.PackedFilesFolder);
    // SimpleStatusReporter
    [ObservableProperty] private InfoBarFeeder _infoBarFeeder = new();
    // Unicode State
    [ObservableProperty] private bool _isUnicode = true;
    // Unpacking & Packing
    [ObservableProperty] private ControlAppearance _appearanceUnpacking = Primary;
    [ObservableProperty] private ControlAppearance _appearanceUnpackingExtended = Secondary;
    [ObservableProperty] private Visibility _unpackingExtendedButtonsVisibility;
    // Game Profile
    [ObservableProperty] private ProfileFiles _gameProfiles = new();
    [ObservableProperty] private GameProfile _currentGameProfile = new();
    [ObservableProperty] private int _selectedGameProfile;
    [ObservableProperty] private ValidationIndicator _validIndicator = new("Profile is valid", "Profile is invalid");
    // Load Unpacked Files
    [ObservableProperty] private ValidationIndicator _validIndicator3 = new("Files are Loaded", "Files are not Loaded", "Reload", "Load");
    // Memory Patching
    [ObservableProperty] private bool _isMemoryPatchingAllowed;
    [ObservableProperty] private ValidationIndicator _validIndicator4 = new("Memory is Patched", "Memory is not Patched", "Unpatch", "Patch");
    // Character Renaming
    private bool _isRenamingAllowed;
    [ObservableProperty] private Visibility _renamingVisibility;
    [ObservableProperty] private ObservableCollection<CharacterSlot> _characterSlots = new();
    // Resign
    [ObservableProperty] private bool _isResignAllowed;
    [ObservableProperty] private string _steamIdInternal = "";
    [ObservableProperty] private string _steamIdExternal = "";
    [ObservableProperty] private ValidationIndicator _validIndicator2 = new("SteamID is valid", "SteamID is invalid");
    [ObservableProperty] private bool _isResignButtonActive;
    
    public void OnNavigatedTo()
    {
        if (GameProfiles.Collection.Count > 0) return;
        LoadGameProfileList();
        OnSteamIdInternalChanged(SteamIdInternal); // trigger OnSteamIdInternalChanged
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void LoadGameProfileList()
    {
        SelectedGameProfile = -1; // trigger OnSelectedGameProfileChanged
        var path = Path.Combine(AppInfo.RootPath, AppInfo.ProfilesFolder);
        Directory.CreateDirectory(path);
        GameProfiles.LoadFiles(path);
    }

    partial void OnSelectedGameProfileChanged(int value)
    {
        // reset values
        CurrentGameProfile = new GameProfile();
        RefreshCharacterSlots();
        InfoBarFeederReset();
        GameProfiles.Selected = value;
        ValidIndicator.State = null;
        if (value >= 0)
        {
            CurrentGameProfile.LoadProfile(GameProfiles.Collection[value].FullPath);
            ValidIndicator.State = CurrentGameProfile.IsValid;
            InfoBarFeederConsume(CurrentGameProfile.Reporter);
            _unpackedFilesPath = Path.Combine(AppInfo.RootPath, CurrentGameProfile.GameTitle, AppInfo.UnpackedFilesFolder);
            _packedFilesPath = Path.Combine(AppInfo.RootPath, CurrentGameProfile.GameTitle, AppInfo.PackedFilesFolder);
        }
        OnPropertyChanged(nameof(ValidIndicator)); // refresh ValidIndicator
        // set visibility and appearance of buttons in Packing & Unpacking section
        if (CurrentGameProfile.AvailableActions.Contains((int)ActionsEnum.Deencrypting))
        {
            AppearanceUnpackingExtended = Primary;
            AppearanceUnpacking = Secondary;
            UnpackingExtendedButtonsVisibility = Visibility.Visible;
        }
        else
        {
            AppearanceUnpacking = Primary;
            UnpackingExtendedButtonsVisibility = Visibility.Collapsed;
        }
        // Is Memory Patching Allowed
        IsMemoryPatchingAllowed = CurrentGameProfile.AvailableActions.Contains((int)ActionsEnum.PatchingGameMemory);
        ValidIndicator4.State = CurrentGameProfile.IsMemoryPatched;
        OnPropertyChanged(nameof(ValidIndicator4));
        // Are the files Loaded
        ValidIndicator3.State = CurrentGameProfile.Bnd4.Entries.Length > 0; 
        OnPropertyChanged(nameof(ValidIndicator3));
        // Is Resigning Allowed
        IsResignAllowed = CurrentGameProfile.AvailableActions.Contains((int)ActionsEnum.ResigningSaveFile);
        SteamIdExternal = CurrentGameProfile.GetSteamId64(); // Get Steam ID
        OnSteamIdInternalChanged(SteamIdInternal); // trigger OnSteamIdInternalChanged
        // Is Character Renaming Allowed
        _isRenamingAllowed = CurrentGameProfile.AvailableActions.Contains((int)ActionsEnum.CharacterRenaming);
    }
    
    partial void OnSteamIdInternalChanged(string value)
    {
        ValidIndicator2.State = Regex.Match(value, @"^[0-9]{17}$").Success;
        OnPropertyChanged(nameof(ValidIndicator2));
        IsResignButtonActive = ValidIndicator2.IsValid && ValidIndicator3.IsValid;
    }
    
    [RelayCommand]
    private static void CopyToClipboard(string str) => Clipboard.SetText(str);
    
    [RelayCommand]
    private void UnpackAndDecrypt()
    {
        InfoBarFeederReset();
        CurrentGameProfile.IsUnicode = IsUnicode;
        var inputPath = PickSl2FileDialog();
        if (inputPath is "") return;
        var status = CurrentGameProfile.Unpack(inputPath, _unpackedFilesPath, true);
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
        if (!status) return;
        IsUnicode = CurrentGameProfile.IsUnicode;
        ReloadFilesInternal();
    }
    
    [RelayCommand]
    private void UnpackOnly()
    {
        InfoBarFeederReset();
        CurrentGameProfile.IsUnicode = IsUnicode;
        var inputPath = PickSl2FileDialog();
        if (inputPath is "") return;
        var status = CurrentGameProfile.Unpack(inputPath, _unpackedFilesPath, false);
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
        if (!status) return;
        IsUnicode = CurrentGameProfile.IsUnicode;
        ReloadFilesInternal();
    }

    [RelayCommand]
    private void PackAndEncrypt()
    {
        InfoBarFeederReset();
        CurrentGameProfile.IsUnicode = IsUnicode;
        var status = CurrentGameProfile.LoadEntries(_unpackedFilesPath) && CurrentGameProfile.Pack(_packedFilesPath, true);
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
    }
    
    [RelayCommand]
    private void PackOnly()
    {
        InfoBarFeederReset();
        CurrentGameProfile.IsUnicode = IsUnicode;
        var status = CurrentGameProfile.LoadEntries(_unpackedFilesPath) && CurrentGameProfile.Pack(_packedFilesPath, false);
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
    }

    [RelayCommand]
    private async Task Resign()
    {
        var status = await CurrentGameProfile.PatchSteamIdInUnpackedFiles(SteamIdInternal, _unpackedFilesPath);
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
        ReloadFilesInternal();
    }
    
    [RelayCommand]
    private async Task PatchGameMemory()
    {
        InfoBarFeederReset();
        await Task.Run(() => CurrentGameProfile.PatchMemory());
        ValidIndicator4.State = CurrentGameProfile.IsMemoryPatched;
        OnPropertyChanged(nameof(ValidIndicator4));
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
    }

    [RelayCommand]
    private async Task ApplyRenaming()
    {
        InfoBarFeederReset();
        var status = await CurrentGameProfile.ChangeCharactersNames(CharacterSlots.ToArray(), _unpackedFilesPath);
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
        ReloadFilesInternal();
    }

    [RelayCommand]
    private void ReloadFiles()
    {
        InfoBarFeederReset();
        ReloadFilesInternal();
        InfoBarFeederConsume(CurrentGameProfile.Reporter);
    }
    private void ReloadFilesInternal()
    {
        CurrentGameProfile.IsUnicode = IsUnicode;
        var status = CurrentGameProfile.LoadEntries(_unpackedFilesPath);
        if (IsResignAllowed && status) SteamIdExternal = CurrentGameProfile.GetSteamId64();
        if (_isRenamingAllowed && status) RefreshCharacterSlots();
        ValidIndicator3.State = status;
        OnPropertyChanged(nameof(ValidIndicator3));
        IsResignButtonActive = ValidIndicator2.IsValid && ValidIndicator3.IsValid;
    }

    // Open Shortcut Commands
    [RelayCommand] private void OpenShortcut1() => IoHelpers.OpenDirectory(_unpackedFilesPath);
    [RelayCommand] private void OpenShortcut2() => IoHelpers.OpenDirectory(_packedFilesPath);
    [RelayCommand] private void OpenShortcut3() => IoHelpers.OpenDirectory(CurrentGameProfile.SaveFolderPath);

    private void RefreshCharacterSlots()
    {
        CharacterSlots.Clear();
        CurrentGameProfile.GetCharacterSlots();
        foreach (var slot in CurrentGameProfile.CharacterSlots)
        {
            CharacterSlots.Add(new CharacterSlot(slot.ID, slot.IsOccupied, slot.Offset)
            {
                CharacterName = slot.CharacterName
            });
        }
        RenamingVisibility = CharacterSlots.Count > 0 && _isRenamingAllowed ? Visibility.Visible : Visibility.Collapsed;
    }

    private string PickSl2FileDialog()
    {
        var inputPath = "";
        OpenFileDialog openFileDialog = new()
        {
            CheckPathExists = true,
            Filter = "SaveLoad2 files (*.sl2)|*.sl2",
            InitialDirectory = CurrentGameProfile.SaveFolderPath,
            FilterIndex = 0,
            Title = "Choose a SaveLoad2 file to load",
        };
        if (openFileDialog.ShowDialog() == true) inputPath = openFileDialog.FileName;
        return inputPath;
    }
    private void InfoBarFeederReset()
    {
        InfoBarFeeder.Reset();
        OnPropertyChanged(nameof(InfoBarFeeder));
    }
    private void InfoBarFeederConsume(SimpleStatusReporter simpleStatusReporter)
    {
        InfoBarFeeder.Consume(simpleStatusReporter);
        OnPropertyChanged(nameof(InfoBarFeeder));
    }
}