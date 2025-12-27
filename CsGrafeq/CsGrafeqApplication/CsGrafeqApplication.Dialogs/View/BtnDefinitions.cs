
using Avalonia.Collections;
using Avalonia.Utilities;
using System;
using System.Collections.Generic;
namespace CsGrafeqApplication.Core.Dialogs;

public class BtnDefinitions: AvaloniaList<string>
{
    private const string Splitters = "|;/,& ";
    public BtnDefinitions()
    {
        
    }

    public BtnDefinitions(IList<string> list)
    {
        AddRange(list);
    }

    public BtnDefinitions(params string[] list)
    {
        AddRange(list);
    }
    public static BtnDefinitions Parse(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return new();
        foreach (var splitter in Splitters)
        {
            if (str.Contains(splitter))
            {
                return new(str.Split(splitter));
            }
        }
        return new([str]);
    }

}