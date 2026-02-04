using System;
using CsGrafeq.Interval;
using CsGrafeq.Shapes;
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
        EnglishChar.Instance.CharValueChanged += CharValueChanged;
        Opacity = Setting.Instance.DefaultOpacity;
        Expression = expression;
    }

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
    }

    public HasReferenceIntervalSetFunc<IntervalSet> Function
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = new((x, y) => Def.FF, EnglishCharEnum.None);

    public string Expression
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            var last = Function;
            var res = IntervalCompiler.TryCompile(Expression, Setting.Instance.EnableExpressionSimplification);
            res.Match(
                success =>
                {
                    Function = success;
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
        EnglishChar.Instance.CharValueChanged -= CharValueChanged;
        RenderTarget.Dispose();
        GC.SuppressFinalize(this);
    }

    private void CharValueChanged(EnglishCharEnum englishCharEnum)
    {
        if (IsCorrect && !IsDeleted)
            if (Function.Reference.HasFlag(englishCharEnum))
                InvokeChanged();
    }

    public override void InvokeChanged()
    {
        FuncChanged?.Invoke(this);
    }
}