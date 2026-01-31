namespace CsGrafeq.Command;

public class CommandBase(object? tag,Action<object?>? initFunc, Action<object?>? doFunc, Action<object?>? unDoFunc, Action<object?>? clearFunc)
{
    public object? Tag { get; set; } = tag;
    public Action<object?> Init { get; set; } = initFunc ?? DoNothing;
    public Action<object?> Do { get; set; } = doFunc ?? DoNothing;
    public Action<object?> UnDo { get; set; } = unDoFunc ?? DoNothing;
    public Action<object?> Clear { get; set; } = clearFunc ?? DoNothing;
    private static void DoNothing(object? obj)
    {
    }
}