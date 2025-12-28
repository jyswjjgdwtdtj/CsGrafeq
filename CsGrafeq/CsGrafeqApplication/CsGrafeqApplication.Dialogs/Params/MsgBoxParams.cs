using Avalonia.Controls;
using Avalonia.Media;
using CsGrafeqApplication.Core.ViewModel;
using CsGrafeqApplication.Dialogs.Interfaces;
using CsGrafeqApplication.Dialogs.Models;

namespace CsGrafeqApplication.Dialogs.Params;

public class MsgBoxParams:ViewModelBase,IMakeDialogResult<string>
{
    /// <summary>
    /// Title of window in title bar
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Minimal width of window
    /// </summary>
    public double MinWidth { get; set; } = 300;

    /// <summary>
    /// Max width of window (default 600px to ensure text wrapping works correctly)
    /// </summary>
    public double MaxWidth { get; set; } = 800;

    /// <summary>
    /// Actual width of window
    /// </summary>
    public double Width { get; set; } = double.NaN;

    /// <summary>
    /// Minimum height of window (default 100)
    /// </summary>
    public double MinHeight { get; set; } = 150;

    /// <summary>
    /// Max height of window
    /// </summary>
    public double MaxHeight { get; set; } = double.PositiveInfinity;

    /// <summary>
    /// Actual height of window
    /// </summary>
    public double Height { get; set; } = double.NaN;

    /// <summary>
    /// Position window
    /// </summary>
    public WindowStartupLocation WindowStartupLocation { get; set; } = WindowStartupLocation.CenterOwner;

    /// <summary>
    /// Window under all windows
    /// </summary>
    public bool Topmost { get; set; } = false;


    public object? Content { get; set; }
    
    public ButtonDefinitions ButtonDefinitions { get; set; } = new();
    private IDialogResult<string>? DialogResultTarget { get; set; }
    public void MakeDialogResult(IDialogResult<string> target)
    {
        DialogResultTarget = target;
    }
}