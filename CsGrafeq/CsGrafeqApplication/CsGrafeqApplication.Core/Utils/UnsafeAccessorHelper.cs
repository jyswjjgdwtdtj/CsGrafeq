using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace CsGrafeqApplication.Core.Utils;

public static class UnsafeAccessorHelper
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PseudoClasses")]
    public static extern IPseudoClasses UnsafeGetPseudoClasses(this StyledElement styledElement);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleResized")]
    public static extern void HandleResized(this Window window, Size clientSize, WindowResizeReason reason);


    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetFocusedElement")]
    public static extern IInputElement? GetFocusedElement(this FocusManager fm);
    
    
    
}