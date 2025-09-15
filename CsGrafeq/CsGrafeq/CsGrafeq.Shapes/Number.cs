using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public sealed class Number:Shape
{
    public override string TypeName=>"Number";
    public NumberGetter NumberGetter { private set; get; }
    public double Value => NumberGetter.Number;
    public event ShapeChangedHandler NumberChanged;
    public Number(NumberGetter numberGetter)
    {
        NumberGetter = numberGetter;
        Visible = false;
        NumberGetter.PropertyChanged += (s, e) =>
        {
            NumberChanged?.Invoke();
            if (e.PropertyName == nameof(NumberGetter.LastValidString))
            {
                this.RaisePropertyChanged(nameof(NumberString));
            }
        };
    }

    public Number SetNumber(double number)
    {
        if (NumberGetter is NumberGetter_FromExpression)
        {
            var newng = new NumberGetter_Direct();
            newng.SetNumber(number);
            return ChangeGetter(newng);
        }
        else
        {
            (NumberGetter as NumberGetter_Direct)!.SetNumber(number);
            return this;
        }
    }

    public string NumberString
    {
        get=> NumberGetter.LastValidString;
    }
    public Number ChangeGetter(NumberGetter numberGetter)
    {
        var newNumber = new Number(numberGetter);
        foreach (var i in this.NumberChanged?.GetInvocationList()??[])
        {
            newNumber.NumberChanged += (i as ShapeChangedHandler);
            NumberChanged -= (i as ShapeChangedHandler);
        }
        return newNumber;
    }

}