using BonfireCore.Helpers;
using Wpf.Ui.Controls;

namespace SL2Bonfire.Controls;

public class InfoBarFeeder
{
    public string Title { get; private set; }

    public bool IsOpen { get; private set; }

    public string Message { get; private set; }

    public InfoBarSeverity Severity { get; private set; }

    public InfoBarFeeder() => Reset();

    public void Consume(SimpleStatusReporter status)
    {
        Title = status.Title;
        Message = status.Message;
        Severity = status.Type switch
        {
            SimpleStatusReporter.InfoType.Information => InfoBarSeverity.Informational,
            SimpleStatusReporter.InfoType.Warning => InfoBarSeverity.Warning,
            SimpleStatusReporter.InfoType.Error => InfoBarSeverity.Error,
            SimpleStatusReporter.InfoType.Success => InfoBarSeverity.Success,
            _ => Severity
        };
        IsOpen = true;
    }

    public void Reset()
    {
        Title = "Info";
        Message = string.Empty;
        Severity = InfoBarSeverity.Informational;
        IsOpen = false;
    }
}