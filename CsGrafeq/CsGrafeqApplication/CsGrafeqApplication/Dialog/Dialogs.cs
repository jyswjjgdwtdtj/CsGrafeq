using System;
using Avalonia.Controls;

namespace CsGrafeqApplication.Dialog;

public static class Dialogs
{
    public static Action<Control, InfoType> InfoHandler;

    public static void Info(Control content, InfoType infotype)
    {
        InfoHandler(content, infotype);
    }
}