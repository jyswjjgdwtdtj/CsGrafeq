using ReactiveUI;
using CsGrafeq.Interval;
using CsGrafeq.Interval.Interface;
using CsGrafeq.Interval.Compiler;

namespace CsGrafeq.Shapes;
[Obsolete("Do not use (although it works)",true)]
public class ImplicitFunction : Shape
{
    public ImplicitFunction()
    {
    }

    public bool IsCorrect
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value, nameof(IsCorrect));
    } = false;

    public IntervalHandler<IntervalSet> Function
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value, nameof(Function));
    } = (x, y) => Def.FF;

    public string Expression
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value, nameof(Expression));
            try
            {
                Function = Compiler.CompileAndTest<IntervalSet>(Expression);
                IsCorrect = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsCorrect = false;
            }
        } 
    } = "";

    protected override string TypeName => "ImplicitFunction";
    public override string Description => Expression;
}