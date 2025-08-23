using System.Collections.ObjectModel;

namespace CsGrafeq.Collections;

public class HasNameList<T> : ObservableCollection<T>
{
    public HasNameList(string Name)
    {
        this.Name = Name;
    }

    public string Name { get; init; }
}

public class HasNameStrList : HasNameList<string>
{
    public HasNameStrList(string Name) : base(Name)
    {
    }
}