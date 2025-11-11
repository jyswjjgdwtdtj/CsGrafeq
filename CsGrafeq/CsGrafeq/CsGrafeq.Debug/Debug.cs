namespace CsGrafeq.Debug;

public static class Debug
{
    public static bool Keyboard { get; set; } = true;
    public static bool Pointer { get; set; } = true;
    public static bool Error { get; set; } = true;

    static Debug()
    {
        #if DEBUG
        Keyboard = true;
        Pointer = true;
        Error = false;
        #else
        Keyboard = false;
        Pointer = false;
        Error = false;
        #endif
    }
    

    public static void LogKeyboard(string message)
    {
        if (Keyboard)
        {
            Console.WriteLine(message);
        }
    }
    public static void LogPointer(string message)
    {
        if (Pointer)
        {
            Console.WriteLine(message);
        }
    }
    public static void LogError(string message)
    {
        if (Error)
        {
            Console.WriteLine(message);
        }
    }
}