using Avalonia.Controls;
using CsGrafeq.CSharpMath.Editor;
using CsGrafeqApplication.Core.Controls;

namespace CsGrafeqApplication.Core.Utils;

public static class InputHelper
{
    public static bool Input(this TopLevel top, CgMathKeyboardInput input)
    {
        var f = top.FocusManager?.GetFocusedElement();
        if (f is MathBox mb)
        {
            mb.PressKey(input);
            return true;
        }

        return false;
    }
}