using Avalonia.Controls;
using CsGrafeqApplication.Dialogs.Interfaces;
using CsGrafeqApplication.Dialogs.Params;
using CsGrafeqApplication.Dialogs.Views;

namespace CsGrafeqApplication.Dialogs;

public static class MessageBoxManager
{
    public static IDialog<string> GetMessageBoxStandard(MsgBoxParams @params)
    {
        var msBoxStandardViewModel = @params;
        var msBoxStandardView = new MsgBoxView()
        {
            DataContext = msBoxStandardViewModel
        };
        return new Dialog<MsgBoxView, MsgBoxParams, string>(msBoxStandardView,
            msBoxStandardViewModel);
    }
}