namespace CsGrafeq.Setting;

public static class OS
{
    public static OSType GetOSType()
    {
        if (OperatingSystem.IsWindows()) return OSType.Windows;
        if (OperatingSystem.IsLinux()) return OSType.Linux;
        if (OperatingSystem.IsMacOS()) return OSType.MacOS;
        if (OperatingSystem.IsBrowser()) return OSType.Browser;
        if (OperatingSystem.IsIOS()) return OSType.IOS;
        if (OperatingSystem.IsAndroid()) return OSType.Android;
        throw new NotSupportedException("Unsupported OS");
    }
}

public enum OSType
{
    Windows,
    Linux,
    MacOS,
    IOS,
    Android,
    Browser
}