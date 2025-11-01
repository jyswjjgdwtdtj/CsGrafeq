namespace CsGrafeq;

public class ExitController(Action exit) : IDisposable
{
    public void Dispose()
    {
        exit?.Invoke();
    }
}