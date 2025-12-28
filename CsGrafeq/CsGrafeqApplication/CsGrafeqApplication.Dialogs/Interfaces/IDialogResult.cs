namespace CsGrafeqApplication.Dialogs.Interfaces;

public interface IDialogResult<T> : IClose
{
    T DialogResult { get; set; }
}