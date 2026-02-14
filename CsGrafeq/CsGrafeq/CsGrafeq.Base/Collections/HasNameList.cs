using System.Collections.ObjectModel;

namespace CsGrafeq.Collections;

public class HasNameList<T>(string name) : ObservableCollection<T>
{
    public string Name { get; init; } = name;
}

public class HasNameStrList(string name) : HasNameList<string>(name);