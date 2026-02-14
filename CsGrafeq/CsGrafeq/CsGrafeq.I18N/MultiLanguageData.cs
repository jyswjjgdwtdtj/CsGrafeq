using ReactiveUI;

namespace CsGrafeq.I18N;

public partial class MultiLanguageData : ReactiveObject
{
    public static MultiLanguageData Empty { get; } = new MultiLanguageData();
    public MultiLanguageData()
    {
        Languages.LanguageChanged += () => { Data = GetData(Languages.CurrentLanguage);};
        PropertyChanged += (s, e) =>
        {
            if(e.PropertyName!=nameof(Data))    
                Data=GetData(Languages.CurrentLanguage);
        };
    }

    public string Data
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    public override string ToString() => Data;
}