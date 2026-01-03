using System;
using Avalonia.Media;
using CsGrafeq.CSharpMath.Editor;
using CsGrafeq.Interval;
using CsGrafeqApplication;
using CsGrafeqApplication.Addons;
using CSharpMath.Atom;
using ReactiveUI;
using SkiaSharp;

namespace CsGrafeq.Shapes;

public class ImplicitFunction : Shape
{
    public readonly Renderable RenderTarget = new();

    public ImplicitFunction(MathList ml)
    {
        TypeName=MultiLanguageResources.ImplicitFunctionText;
        PropertyChanged += (s, e) =>
        {
            RefreshIsActive();
            if (e.PropertyName == nameof(PropertyToReceiveMathListChanged) || e.PropertyName == nameof(MathList))
                RefreshExpression();
            else if (e.PropertyName == nameof(IsCorrect)) BorderBrush = IsCorrect ? Brushes.Blue : Brushes.Red;
        };
        Description = "ImplicitFunction";
        MathList = ml.Clone(false);
        EnglishChar.Instance.CharValueChanged += CharValueChanged;
        RenderTarget.OnRender += RenderTarget_OnRender;
        Opacity = Static.Instance.DefaultOpacity;
        BorderBrush = IsCorrect ? Brushes.Blue : Brushes.Red;
    }

    public bool PropertyToReceiveMathListChanged
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
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

    public IBrush BorderBrush
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = Brushes.Blue;

    public HasReferenceIntervalSetFunc<IntervalSet> Function
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = new((x, y) => Def.FF, EnglishCharEnum.None);

    public MathList MathList
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string Expression
    {
        get => field;
        private set
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

    private void RefreshExpression()
    {
        var res = MathList.Parse();
        res.Match(exp => { Expression = exp; }, ex => { IsCorrect = false; });
    }

    public void RefreshIsActive()
    {
        RenderTarget.IsActive = IsCorrect && !IsDeleted;
    }

    private void RenderTarget_OnRender(SKCanvas dc, SKRect rect)
    {
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