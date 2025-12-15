global using UnmanagedBOOL = int;
using System.Runtime.InteropServices;

namespace CsGrafeq.Windows.IME;

/// <content>
///     Contains extern methods from "IMM32.dll".
/// </content>
internal static class ImePInvoke
{
	/// <summary>The ImmAssociateContext function (immdev.h) associates the specified input context with the specified window.</summary>
	/// <returns>Returns the handle to the input context previously associated with the window.</returns>
	/// <remarks>
	///     When associating an input context with a window, an application must remove the association before destroying
	///     the input context. One way to do this is to save the handle and reassociate it to the default input context with
	///     the window.
	/// </remarks>
	[DllImport("IMM32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint ImmAssociateContext(nint param0, nint param1);

	/// <summary>The ImmGetContext function (immdev.h) returns the input context associated with the specified window.</summary>
	/// <returns>Returns the handle to the input context.</returns>
	/// <remarks>
	///     <para>
	///         An application should routinely use this function to retrieve the current input context before attempting to
	///         access information in the context. The application must call
	///         <a href="https://docs.microsoft.com/windows/desktop/api/imm/nf-imm-immreleasecontext">ImmReleaseContext</a>
	///         when it is finished with the input context.
	///     </para>
	///     <para>
	///         <see href="https://learn.microsoft.com/windows/win32/api/immdev/nf-immdev-immgetcontext#">
	///             Read more on
	///             docs.microsoft.com
	///         </see>
	///         .
	///     </para>
	/// </remarks>
	[DllImport("IMM32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint ImmGetContext(nint param0);

	/// <summary>The ImmGetOpenStatus function (immdev.h) determines whether the IME is open or closed.</summary>
	/// <returns>Returns a nonzero value if the IME is open, or 0 otherwise.</returns>
	/// <remarks>
	///     <para>
	///         <see href="https://learn.microsoft.com/windows/win32/api/immdev/nf-immdev-immgetopenstatus">
	///             Learn more about
	///             this API from docs.microsoft.com
	///         </see>
	///         .
	///     </para>
	/// </remarks>
	[DllImport("IMM32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ImmGetOpenStatus(nint param0);

	/// <summary>
	///     The ImmReleaseContext function (immdev.h) releases the input context and unlocks the memory associated in the
	///     input context.
	/// </summary>
	/// <returns>Returns a nonzero value if successful, or 0 otherwise.</returns>
	/// <remarks>
	///     <para>
	///         <see href="https://learn.microsoft.com/windows/win32/api/immdev/nf-immdev-immreleasecontext">
	///             Learn more about
	///             this API from docs.microsoft.com
	///         </see>
	///         .
	///     </para>
	/// </remarks>
	[DllImport("IMM32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ImmReleaseContext(nint param0, nint param1);

	/// <summary>The ImmSetOpenStatus function (immdev.h) opens or closes the IME.</summary>
	/// <returns>Returns a nonzero value if successful, or 0 otherwise.</returns>
	/// <remarks>
	///     This function causes an
	///     <a href="https://docs.microsoft.com/windows/desktop/Intl/imn-setopenstatus">IMN_SETOPENSTATUS</a> command to be
	///     sent to the application.
	/// </remarks>
	[DllImport("IMM32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ImmSetOpenStatus(nint param0, UnmanagedBOOL param1);
}