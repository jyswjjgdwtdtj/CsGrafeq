using System;
using System.Globalization;
using System.Threading;

namespace CsGrafeqApplication;

public class Languages
{
    public static string[] AllowedLanguages => ["en-us", "zh-hans"];
    public static string CurrentLanguage { get; private set; }
    public static int CurrentLanguageIndex { get; private set; }

    public static void SetLanguage(string language)
    {
        if (AllowedLanguages.Contains(language))
        {
            CurrentLanguageIndex = Array.IndexOf(AllowedLanguages, language);
            CurrentLanguage = language;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            LanguageChanged?.Invoke();
        }
    }

    public static event Action LanguageChanged;
}