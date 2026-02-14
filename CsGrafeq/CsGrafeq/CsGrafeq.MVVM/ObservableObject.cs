using System.Text.Json.Serialization;
using CsGrafeq.I18N;
using ReactiveUI;

namespace CsGrafeq.MVVM;

public class ObservableObject : ReactiveObject
{
    [JsonIgnore]
    public MultiLanguageResources MultiLanguageResources { get; } = MultiLanguageResources.Instance;
}