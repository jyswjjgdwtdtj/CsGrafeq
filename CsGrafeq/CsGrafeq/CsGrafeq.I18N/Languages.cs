using System.Globalization;

namespace CsGrafeq.I18N;

public static class Languages
{
    public static int InitialLanguageIndex { get; set; }
    public static LanguagesEnum CurrentLanguage { get; private set; }

    public static void SetLanguage(LanguagesEnum language)
    {
        CurrentLanguage = language;
        InitialLanguageIndex = (int)CurrentLanguage;
        LanguageChanged?.Invoke();
    }

    public static event Action? LanguageChanged;
}