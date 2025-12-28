namespace CsGrafeqApplication.Dialogs.Interfaces;

public interface IMakeDialogResult<T>
{
    void MakeDialogResult(IDialogResult<T> fullApi);
}