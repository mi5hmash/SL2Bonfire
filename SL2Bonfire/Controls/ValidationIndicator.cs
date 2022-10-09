using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Common;

namespace SL2Bonfire.Controls;
public class ValidationIndicator
{
    private string _validTooltipCaption = "Valid";
    private string _invalidTooltipCaption = "Invalid";
    private string _validContentCaption = "Valid";
    private string _invalidContentCaption = "Invalid";

    private bool? _state;
    /// <summary>
    /// A boolean? state of this indicator. Indicator is hidden when null.
    /// </summary>
    public bool? State
    {
        get => _state; 
        set
        {
            _state = value;
            IsValid = value ?? false;
            switch (value)
            {
                case true:
                    Symbol = SymbolRegular.CheckmarkCircle12;
                    Color = new SolidColorBrush(Colors.PaleGreen);
                    Visibility = Visibility.Visible;
                    StateTooltipCaption = _validTooltipCaption;
                    StateContentCaption = _validContentCaption;
                    break;
                case false:
                    Symbol = SymbolRegular.DismissCircle12;
                    Color = new SolidColorBrush(Colors.PaleVioletRed);
                    Visibility = Visibility.Visible;
                    StateTooltipCaption = _invalidTooltipCaption;
                    StateContentCaption = _invalidContentCaption;
                    break;
                default:
                    Visibility = Visibility.Hidden;
                    StateTooltipCaption = "";
                    StateContentCaption = "";
                    break;
            }
        }
    }

    /// <summary>
    /// A boolean state of this indicator. Valid if true.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Sets the visibility.
    /// </summary>
    public Visibility Visibility { get; private set; } = Visibility.Hidden;
    
    /// <summary>
    /// A symbol which will be displayed as an icon.
    /// </summary>
    public SymbolRegular Symbol { get; private set; } = SymbolRegular.DismissCircle12;

    /// <summary>
    /// A color of a symbol.
    /// </summary>
    public SolidColorBrush Color { get; private set; } = new(Colors.White);

    /// <summary>
    /// A state which can be shown as a Tooltip.
    /// </summary>
    public string StateTooltipCaption { get; private set; } = "";

    /// <summary>
    /// A state which can be shown as a Tooltip.
    /// </summary>
    public string StateContentCaption { get; private set; } = "";

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="validTooltipCaption"></param>
    /// <param name="invalidTooltipCaption"></param>
    /// <param name="validContentCaption"></param>
    /// <param name="invalidContentCaption"></param>
    public ValidationIndicator(string? validTooltipCaption = null, string? invalidTooltipCaption = null, string? validContentCaption = null, string? invalidContentCaption = null)
    {
        UpdateCaptions(validTooltipCaption, invalidTooltipCaption, validContentCaption, invalidContentCaption);
    }

    /// <summary>
    /// Updates valid and invalid captions.
    /// </summary>
    /// <param name="validTooltipCaption"></param>
    /// <param name="invalidTooltipCaption"></param>
    /// <param name="validContentCaption"></param>
    /// <param name="invalidContentCaption"></param>
    public void UpdateCaptions(string? validTooltipCaption = null, string? invalidTooltipCaption = null, string? validContentCaption = null, string? invalidContentCaption = null)
    {
        _validTooltipCaption = validTooltipCaption ?? _validTooltipCaption;
        _invalidTooltipCaption = invalidTooltipCaption ?? _invalidTooltipCaption;
        _validContentCaption = validContentCaption ?? _validContentCaption;
        _invalidContentCaption = invalidContentCaption ?? _invalidContentCaption;
    }
}