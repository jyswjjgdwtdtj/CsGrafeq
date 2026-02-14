namespace CsGrafeq.Utilities;

public static class CancellationHelper
{
    private static CancellationTokenSource _tokenSource = new();
    public static CancellationToken CancellationHelperToken => _tokenSource.Token;

    public static void CancelAndRenew()
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource();
    }
}