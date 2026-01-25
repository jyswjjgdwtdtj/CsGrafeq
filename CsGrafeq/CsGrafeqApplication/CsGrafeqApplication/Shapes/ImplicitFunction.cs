using System;
using CsGrafeq.Interval;
using CsGrafeqApplication.Addons;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public class ImplicitFunction : Shape
{
#if DEBUG
    public static ImplicitFunction DebugFunc = new("y=sin(x)");
#endif
    public readonly Renderable RenderTarget = new();

    public ImplicitFunction(string expression)
    {
        TypeName = MultiLanguageResources.ImplicitFunctionText;
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

    public byte Opacity
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeShapeChanged();
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
            try
            {
                var last = Function;
                Function = IntervalCompiler.Compile(Expression);
                IsCorrect = true;
                last.Dispose();
            }
            catch (Exception ex)
            {
                IsCorrect = false;
            }

            InvokeShapeChanged();
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
                InvokeShapeChanged();
    }
}