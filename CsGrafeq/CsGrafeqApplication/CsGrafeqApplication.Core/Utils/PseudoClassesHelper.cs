using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;

namespace CsGrafeqApplication.Core.Utils;

public static class PseudoClassesHelper
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PseudoClasses")]
    public static extern IPseudoClasses UnsafeGetPseudoClasses(this StyledElement styledElement);
}