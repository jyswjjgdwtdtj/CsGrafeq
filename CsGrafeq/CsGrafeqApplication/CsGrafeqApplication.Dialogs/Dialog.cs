using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CsGrafeqApplication.Dialogs.Interfaces;
using CsGrafeqApplication.Dialogs.Windows;
using DialogHostAvalonia;

namespace CsGrafeqApplication.Dialogs;

public class Dialog<V, VM, T> : IDialog<T> where V : UserControl, IClosable, IDialogResult<T>
    where VM : IMakeDialogResult<T>
{
    private readonly V _view;
    private readonly VM _viewModel;

    private readonly string ClickAwayParam = "MsBoxIdentifier_Cancel";

    public Dialog(V view, VM viewModel)
    {
        _view = view;
        _viewModel = viewModel;
    }

    /// <summary>
    ///     Show messagebox depending on the type of application
    ///     If application is SingleViewApplicationLifetime (Mobile or Browses) then show messagebox as popup
    ///     If application is ClassicDesktopStyleApplicationLifetime (Desktop) then show messagebox as window
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public Task<T> ShowAsync()
    {
        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return ShowWindowAsync();

        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
            return ShowAsPopupAsync(lifetime.MainView as ContentControl);

        throw new NotSupportedException("ApplicationLifetime is not supported");
    }

    /// <summary>
    ///     Show messagebox as window
    /// </summary>
    /// <returns></returns>
    public Task<T> ShowWindowAsync()
    {
        _viewModel.MakeDialogResult(_view);
        var window = new MsgBoxWindow
        {
            Content = _view,
            DataContext = _viewModel
        };

        window.Closed += _view.CloseWindow;
        var tcs = new TaskCompletionSource<T>();

        _view.Closing += (sender, args) =>
        {
            tcs.TrySetResult(_view.DialogResult);
            window.Close();
        };

        window.Show();
        return tcs.Task;
    }

    /// <summary>
    ///     Show messagebox as window with owner
    /// </summary>
    /// <param name="owner">Window owner </param>
    /// <returns></returns>
    public async Task<T> ShowWindowDialogAsync(Window owner)
    {
        _viewModel.MakeDialogResult(_view);
        var window = new MsgBoxWindow
        {
            Content = _view,
            DataContext = _viewModel
        };
        window.Closed += _view.CloseWindow;
        var tcs = new TaskCompletionSource<T>();

        _view.Closing += (_, _) =>
        {
            tcs.TrySetResult(_view.DialogResult);
            window.Close();
        };

        await window.ShowDialog(owner);
        return await tcs.Task;
    }

    /// <summary>
    ///     Show messagebox as popup
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public Task<T> ShowAsPopupAsync(ContentControl owner)
    {
        DialogHostStyles style = null;
        if (!owner.Styles.OfType<DialogHostStyles>().Any())
        {
            style = [];
            owner.Styles.Add(style);
        }


        var parentContent = owner.Content;
        var dh = new DialogHost
        {
            Identifier = "MsBoxIdentifier" + Guid.NewGuid()
        };
        _viewModel.MakeDialogResult(_view);
        owner.Content = null;
        dh.Content = parentContent;

        dh.CloseOnClickAway = true;
        dh.CloseOnClickAwayParameter = ClickAwayParam;
        dh.DialogClosing += (ss, ee) =>
        {
            if (ee.Parameter?.ToString() == ClickAwayParam) _view.Close();
        };

        owner.Content = dh;
        var tcs = new TaskCompletionSource<T>();
        _view.Closing += (_, _) =>
        {
            var r = _view.DialogResult;

            if (dh.CurrentSession != null && !dh.CurrentSession.IsEnded) DialogHost.Close(dh.Identifier);

            owner.Content = null;
            dh.Content = null;
            owner.Content = parentContent;
            if (style != null) owner.Styles.Remove(style);
            tcs.TrySetResult(r);
        };
        DialogHost.Show(_view, dh.Identifier);
        return tcs.Task;
    }

    /// <summary>
    ///     Show messagebox as popup with owner
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public Task<T> ShowAsPopupAsync(Window owner)
    {
        return ShowAsPopupAsync(owner as ContentControl);
    }
}