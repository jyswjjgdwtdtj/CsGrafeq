using System;
using CsGrafeq.Interval;
using CsGrafeq.Numeric;
using CsGrafeq.Shapes;
using CsGrafeq.Variables;
using CsGrafeqApplication.Addons;
using CsGrafeqApplication.Addons.FunctionPad;
using ReactiveUI;

namespace CsGrafeqApplication.Function;

public class ImplicitFunction : InteractiveObject
{
    public readonly Renderable RenderTarget = new();

    public Action<ImplicitFunction>? FuncChanged;

    public ImplicitFunction(string expression, FunctionPad owner)
    {
        Owner = owner;
        Description = "ImplicitFunction";
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsCorrect) || e.PropertyName == nameof(IsDeleted))
                RefreshIsActive();
        };
        VarRecorder.Instance.CharValueChanged += CharValueChanged;
        Opacity = Setting.Instance.DefaultOpacity;
        Expression = expression;
    }

    public bool IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public FunctionPad Owner { get; init; }

    public byte Opacity
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeChanged();
        }
    }

    public bool ShowFormula
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool IsCorrect
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public string LastError
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    public bool NeedCheckPixel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public HasReferenceIntervalSetFunc<IntervalSet> Function
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = new((x, y) => Def.FF, VariablesEnum.None);

    public Func<double,double,double,double, bool> MsFunction=static (_,_,_,_)=>false;

    public string Expression
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            var last = Function;
            var res = IntervalCompiler.TryCompile(field, Setting.Instance.EnableExpressionSimplification);
            res.Match(
                success =>
                {
                    Function = success;
                    MsFunction = IntervalCompiler.GetMarchingSquaresFunc(field);
                    LastError = "No Error";
                    IsCorrect = true;
                    last.Dispose();
                },
                failure =>
                {
                    IsCorrect = false;
                    LastError = failure.Message;
                }
            );

            InvokeChanged();
            Description = Expression;
        }
    }

    public void SetExpression(string expression)
    {
        Expression = expression;
    }

    public void RefreshExpression()
    {
    }

    public void RefreshIsActive()
    {
        RenderTarget.IsActive = IsCorrect && !IsDeleted;
    }

    public override void Dispose()
    {
        VarRecorder.Instance.CharValueChanged -= CharValueChanged;
        RenderTarget.Dispose();
        GC.SuppressFinalize(this);
    }

    private void CharValueChanged(VariablesEnum variablesEnum)
    {
        if (IsCorrect && !IsDeleted)
            if (Function.References.HasFlag(variablesEnum))
                InvokeChanged();
    }

    public override void InvokeChanged()
    {
        FuncChanged?.Invoke(this);
    }
}