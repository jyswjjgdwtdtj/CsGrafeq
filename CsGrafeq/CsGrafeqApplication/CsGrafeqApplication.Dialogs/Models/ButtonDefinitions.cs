
using Avalonia.Collections;
using Avalonia.Utilities;
using System;
using System.Collections.Generic;
namespace CsGrafeqApplication.Dialogs.Models;

public class ButtonDefinitions: AvaloniaList<string>
{
    private const string Splitters = "|;/,& ";
    public ButtonDefinitions()
    {
    }

    public override void Add(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return;
        foreach (var splitter in Splitters)
        {
            if (str.Contains(splitter))
            {
                base.AddRange(str.Split(splitter));
                return;
            }
        }
        base.Add(str);
    }

    public IList<ButtonDefinition> BtnDefs
    {
        get
        {
            return this.Select(o => new ButtonDefinition() { Name = o }).ToList();
        }
    }

}