using CsGrafeqApplication.Dialogs.InfoDialog;

namespace CsGrafeqApplication.Dialogs.Interfaces;

public interface IInfoDialog
{
    void Info(object content, InfoType infotype);
}