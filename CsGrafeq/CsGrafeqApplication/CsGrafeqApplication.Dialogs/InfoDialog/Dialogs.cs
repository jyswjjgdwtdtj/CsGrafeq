using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CsGrafeqApplication.Dialogs.Interfaces;

namespace CsGrafeqApplication.Dialogs.InfoDialog;

public static class Dialogs
{
    public static void Info(this Visual target, Control content, InfoType infotype)
    {
        var anc = target.FindAncestorOfType<IInfoDialog>(true);
        anc?.Info(content, infotype);
    }
}