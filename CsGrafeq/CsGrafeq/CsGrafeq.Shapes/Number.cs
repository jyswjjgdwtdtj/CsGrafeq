using CsGrafeq.Shapes.ShapeGetter;

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
        };
    }

    public string NumberString
    {
        get=> Value.ToString();
    }

    public bool ParseString(string str)
    {
        if (double.TryParse(str, out double result))
        {
            //未来将使用通用的Expression 经表达式树编译
            (NumberGetter as NumberGetter_Direct)?.SetNumber(result);
            return true;
        }
        return false;
    }
    
    
}