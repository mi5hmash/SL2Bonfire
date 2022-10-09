using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Common.Interfaces;

namespace SL2Bonfire.ViewModels;

public partial class AboutViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        AppVersion = $"{GetAssemblyVersion()}";
        _isInitialized = true;
    }

    /// <summary>
    /// Gets application version.
    /// </summary>
    /// <returns></returns>
    private static string GetAssemblyVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
}