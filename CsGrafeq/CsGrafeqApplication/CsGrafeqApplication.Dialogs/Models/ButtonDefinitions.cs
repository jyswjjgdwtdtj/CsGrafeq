using Avalonia.Collections;

namespace CsGrafeqApplication.Dialogs.Models;

public class ButtonDefinitions : AvaloniaList<string>
{
    private const string Splitters = "|;/,& ";

    public IList<ButtonDefinition> BtnDefs
    {
        get { return this.Select(o => new ButtonDefinition { Name = o }).ToList(); }
    }

    public override void Add(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return;
        foreach (var splitter in Splitters)
            if (str.Contains(splitter))
            {
                base.AddRange(str.Split(splitter));
                return;
            }

        base.Add(str);
    }
}