namespace CsGrafeq.Debug;

public static class Debug
{
    static Debug()
    {
#if DEBUG
        Keyboard = false;
        Pointer = false;
        Error = false;
#else
        Keyboard = false;
        Pointer = false;
        Error = false;
#endif
    }

    public static bool Keyboard { get; set; } = true;
    public static bool Pointer { get; set; } = true;
    public static bool Error { get; set; } = true;

}