using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SL2Bonfire.Helpers;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace SL2Bonfire.ViewModels;

public partial class ContainerViewModel : ObservableObject
{
    private bool _isInitialized;

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<INavigationControl> _navigationItems = new();

    [ObservableProperty]
    private ObservableCollection<INavigationControl> _navigationFooter = new();

    [ObservableProperty]
    private ObservableCollection<MenuItem> _trayMenuItems = new();

    public ContainerViewModel(INavigationService navigationService)
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        ApplicationTitle = AppInfo.Name;

        NavigationItems = new ObservableCollection<INavigationControl>
        {
            new NavigationItem()
            {
                Content = "Bonfire",
                PageTag = "homepage",
                Icon = SymbolRegular.Fire24,
                PageType = typeof(Views.Pages.BonfirePage)
            }
        };

        NavigationFooter = new ObservableCollection<INavigationControl>
        {
            new NavigationItem()
            {
                Content = "About",
                PageTag = "about",
                Icon = SymbolRegular.QuestionCircle24,
                PageType = typeof(Views.Pages.AboutPage)
            }
        };

        _isInitialized = true;
    }
}