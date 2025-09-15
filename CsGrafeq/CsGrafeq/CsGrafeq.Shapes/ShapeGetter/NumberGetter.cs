using CsGrafeq;
using CsGrafeq.Compiler;
using CsGrafeq.Numeric;
using ReactiveUI;

namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class NumberGetter:Getter
{
    public abstract bool IsReadOnly { get; }
    public abstract double Number { get; }
    public abstract bool TryLateParse(string str);
    public string LastValidString="0";
}
public class NumberGetter_Direct:NumberGetter
{
    public sealed override bool IsReadOnly => false;
    public override bool TryLateParse(string str)
    {
        if (double.TryParse(str, out double result))
        {
            SetNumber(result);
            return true;
        }
        return false;
    }
    protected double Value=0;
    public NumberGetter_Direct(double value=1)
    {
        Value = value;
    }
    public override double Number => Value;
    public void SetNumber(double number)
    {
        if (number != Value)
        {
            Value= number;  
            this.RaisePropertyChanged(nameof(Number));
            this.RaiseAndSetIfChanged(ref LastValidString, number.ToString(), nameof(LastValidString));
        }
    }
}
public class NumberGetter_FromExpression : NumberGetter
{
    public sealed override bool IsReadOnly => false;
    protected Function0<DoubleNumber> Func;
    protected bool[] UsedVars=new bool['z'-'a'+1];
    public NumberGetter_FromExpression()
    {
        Func = DefaultFunc;
        EnglishChar.Instance.PropertyChanged += (s, e) =>
        {
            CallChanged();
        };
        this.RaiseAndSetIfChanged(ref LastValidString, "1", nameof(LastValidString));
    }
    private static DoubleNumber DefaultFunc() => new DoubleNumber(1);
    public override double Number => Func().Value;
    public void CallChanged()
    {
        this.RaisePropertyChanged(nameof(Number));
    }
    public override bool TryLateParse(string str)
    {
        if(str==LastValidString)
            return true;
        try
        {
            Func = Compiler.Compiler.Compile0<DoubleNumber>(str, out var usedVars);
            for (int i = 0; i < 'z' - 'a' + 1; i++)
            {
                if (UsedVars[i])
                {
                    EnglishChar.Instance.RemoveReference((char)('a' + i));
                }
            }
            for (int i = 0; i < usedVars.Length; i++)
            {
                if (usedVars[i])
                {
                    EnglishChar.Instance.AddReference((char)('a' + i));
                }
            }
            UsedVars = usedVars;
            CallChanged();
            this.RaiseAndSetIfChanged(ref LastValidString, str, nameof(LastValidString));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}