using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;

namespace CsGrafeqApplication.Core.Utils;

public static class UnsafeAccessorHelper
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PseudoClasses")]
    public static extern IPseudoClasses UnsafeGetPseudoClasses(this StyledElement styledElement);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleResized")]
    public static extern void HandleResized(this Window window, Size clientSize, WindowResizeReason reason);
}