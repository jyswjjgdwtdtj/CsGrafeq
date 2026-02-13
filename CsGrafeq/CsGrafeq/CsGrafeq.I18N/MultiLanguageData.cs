using ReactiveUI;

namespace CsGrafeq.I18N;

public class MultiLanguageData : ReactiveObject
{
    public static MultiLanguageData Empty { get; } = new MultiLanguageData();
    public MultiLanguageData()
    {
        Languages.LanguageChanged += () => { Data = Languages.CurrentLanguage == "en-us" ? English : Chinese; };
        PropertyChanged += (s, e) => Data = Languages.CurrentLanguage == "en-us" ? English : Chinese;
    }

    public string English
    {
        get;
        init => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    public string Chinese
    {
        get;
        init => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    public string Data
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    public override string ToString()
    {
        return Data;
    }
}