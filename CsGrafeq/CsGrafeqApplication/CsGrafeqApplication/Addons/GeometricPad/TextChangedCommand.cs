using System;
using Avalonia.Controls;
using CsGrafeq.Command;

namespace CsGrafeqApplication.Addons.GeometricPad;

public class TextChangedCommand : CommandBase
{
    public readonly string PreviousText;

    public TextChangedCommand(string previous, string current, ExpNumber target, TextBox tb)
        : base(null, null, null, null, null)
    {
        PreviousText = previous;
        CurrentText = current;
        Number = target;
        Do = _ =>
        {
            target.ValueStr = current;
            (tb.Tag as GeoPoint)?.RefreshValues();
            if (target.IsError)
                DataValidationErrors.SetError(tb, new Exception());
            else
                DataValidationErrors.ClearErrors(tb);
        };
        UnDo = _ =>
        {
            target.ValueStr = previous;
            (tb.Tag as GeoPoint)?.RefreshValues();
            if (target.IsError)
                DataValidationErrors.SetError(tb, new Exception());
            else
                DataValidationErrors.ClearErrors(tb);
        };
        Clear = _ => { };
    }

    public string CurrentText { get; init; }
    public ExpNumber Number { get; init; }
}