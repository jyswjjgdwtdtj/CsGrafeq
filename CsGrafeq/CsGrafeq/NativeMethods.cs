﻿using System.Runtime.InteropServices;

namespace CsGrafeq;

public static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(int hWnd, string text, string caption, uint type);
}