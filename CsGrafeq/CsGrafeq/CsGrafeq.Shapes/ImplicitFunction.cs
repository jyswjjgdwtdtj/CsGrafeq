using CsGrafeq.Interval;
using CsGrafeq.Interval.Compiler;
using ReactiveUI;

namespace CsGrafeq.Shapes;

[Obsolete("Do not use (although it works)", true)]
public class ImplicitFunction : Shape
{
    public bool IsCorrect
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public IntervalHandler<IntervalSet> Function
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = (x, y) => Def.FF;

    public string Expression
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            try
            {
                Function = Compiler.CompileAndTest<IntervalSet>(Expression);
                IsCorrect = true;
            }
            catch (Exception ex)
            {
                IsCorrect = false;
            }

            Description = Expression;
        }
    } = "";

    public override string TypeName => "ImplicitFunction";
}