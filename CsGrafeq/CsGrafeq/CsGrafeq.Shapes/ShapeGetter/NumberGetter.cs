using ReactiveUI;

namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class NumberGetter:Getter
{
    public abstract bool IsReadOnly { get; }
    public abstract double Number { get; }
}
public abstract class NumberGetter_Setable : NumberGetter
{
    public abstract void SetNumber(double number);
    public sealed override bool IsReadOnly => false;
}
public class NumberGetter_Direct:NumberGetter_Setable
{
    protected double Value=0;
    public NumberGetter_Direct(double value)
    {
        Value = value;
    }
    public override double Number => Value;
    public override void SetNumber(double number)
    {
        if (number != Value)
        {
            Value= number;  
            this.RaisePropertyChanged(nameof(Number));
        }
    }
}
public abstract class NumberGetter_Unsetable : NumberGetter
{
    public override bool IsReadOnly => true;
}

public class NumberGetter_Constant : NumberGetter_Unsetable
{
    protected double Value=0;
    public NumberGetter_Constant(double number)
    {
        Value= number;
    }
    public override double Number => Number;
}
public class NumberGetter_FromExpression : NumberGetter_Unsetable
{
    protected readonly Func<double> Func;
    public NumberGetter_FromExpression(Func<double> func)
    {
        Func = func;
    }
    public override double Number => Func();
    public void CallChanged()
    {
        this.RaisePropertyChanged(nameof(Number));
    }
}