using ReactiveUI;
using CsGrafeq.TupperInterval;
using CsGrafeq.TupperInterval.Compiler;

namespace CsGrafeq.Shapes;

public class ImplicitFunction : Shape
{
    public ImplicitFunction()
    {
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Expression))
            {
                try
                {
                    Function = Compiler.CompileAndTest<IntervalSet>(Expression==null?string.Empty:Expression,typeof(IntervalSetMath));
                    IsCorrect = true;
                }
                catch (Exception ex)
                {
                    IsCorrect = false;
                }
            }
        };
    }

    public bool IsCorrect
    {
        get=>field; 
        private set=>this.RaiseAndSetIfChanged(ref field, value,nameof(IsCorrect));
    }

    public TupperIntervalHandler<IntervalSet> Function
    {
        get=>field;
        private set=>this.RaiseAndSetIfChanged(ref field, value,nameof(Function));
    }
    public string Expression
    {
        get => field;
        set=>this.RaiseAndSetIfChanged(ref field, value, nameof(Expression));
    }

    protected override string TypeName => "ImplicitFunction";
    public override string Description => Expression;
}