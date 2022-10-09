namespace BonfireCore.Helpers;

public class SimpleStatusReporter
{
    public string Title { get; private set; } = "";

    public string Message { get; private set; } = "";

    public InfoType Type { get; private set; }

    public enum InfoType
    {
        Information,
        Warning,
        Error,
        Success
    }

    public void Information(string message)
    {
        Title = "Information";
        Message = message;
        Type = InfoType.Information;
    }

    public void Warning(string message)
    {
        Title = "Warning";
        Message = message;
        Type = InfoType.Warning;
    }

    public void Error(string message)
    {
        Title = "Failure";
        Message = message;
        Type = InfoType.Error;
    }

    public void Success(string message)
    {
        Title = "Success";
        Message = message;
        Type = InfoType.Success;
    }
}