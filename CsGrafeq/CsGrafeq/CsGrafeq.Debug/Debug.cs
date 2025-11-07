namespace CsGrafeq.Debug;

public static class Debug
{
    public static bool Keyboard { get; set; } = true;
    public static bool Pointer { get; set; } = true;

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
}