using CsGrafeq.I18N;
using ReactiveUI;

namespace CsGrafeq.MVVM;

public class ObservableObject: ReactiveObject
{
    public MultiLanguageResources MultiLanguageResources { get; } = MultiLanguageResources.Instance;
}