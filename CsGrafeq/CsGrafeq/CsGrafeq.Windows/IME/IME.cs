using System.Diagnostics;
using Avalonia.Controls;

namespace CsGrafeq.Windows.IME;

public static class IME
{
    /// <summary>
    ///     Disable the IME of a WIN32 control
    /// </summary>
    /// <param name="handle">the hwnd of a control</param>
    public static void DisableIme(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero)
            return;
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;
        if (IsOpen(handle)) SetOpenStatus(false, handle);
        ImePInvoke.ImmAssociateContext(handle, IntPtr.Zero);
    }

    private static void SetOpenStatus(bool open, IntPtr handle)
    {
        var inputContext = ImePInvoke.ImmGetContext(handle);

        if (inputContext != IntPtr.Zero)
        {
            var succeeded = ImePInvoke.ImmSetOpenStatus(inputContext, open ? 1 : 0);
            Debug.Assert(succeeded, "Could not set the IME open status.");

            if (succeeded)
            {
                succeeded = ImePInvoke.ImmReleaseContext(handle, inputContext);
                Debug.Assert(succeeded, "Could not release IME context.");
            }
        }
    }

    internal static bool IsOpen(IntPtr handle)
    {
        var inputContext = ImePInvoke.ImmGetContext(handle);

        var retval = false;

        if (inputContext != IntPtr.Zero)
        {
            retval = ImePInvoke.ImmGetOpenStatus(inputContext);
            ImePInvoke.ImmReleaseContext(handle, inputContext);
        }

        return retval;
    }
}