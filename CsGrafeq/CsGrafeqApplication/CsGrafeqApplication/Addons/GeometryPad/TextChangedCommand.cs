using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsGrafeqApplication.Addons.GeometryPad
{
    public class TextChangedCommand : CommandManager.Command
    {
        public readonly string PreviousText;

        public TextChangedCommand(string previous, string current, ExpNumber target, TextBox tb)
            : base(new object(), _ => { }, _ => { }, _ => { })
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
}
