using Wpf.Ui.Common.Interfaces;

namespace SL2Bonfire.Views.Pages;

/// <summary>
/// Interaction logic for BonfirePage.xaml
/// </summary>
public partial class BonfirePage : INavigableView<ViewModels.BonfireViewModel>
{
    public ViewModels.BonfireViewModel ViewModel { get; }

    public BonfirePage(ViewModels.BonfireViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}