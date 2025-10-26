using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CsGrafeq.Interval;
using CsGrafeqApplication;
using CsGrafeqApplication.Addons;
using CsGrafeqApplication.Addons.GeometryPad;
using CsGrafeqApplication.Controls.Displayers;
using ReactiveUI;
using SkiaSharp;

namespace CsGrafeq.Shapes;

public class ImplicitFunction : Shape
{
    public readonly Renderable RenderTarget=new();
    public ImplicitFunction(string expression = "y=x+1")
    {
        Description = "ImplicitFunction";
        Expression = expression;
        EnglishChar.Instance.CharValueChanged += CharValueChanged;
        RenderTarget.OnRender += RenderTarget_OnRender;
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
    public bool IsCorrect
    {
        get => field;
        private set
        {
            this.RaiseAndSetIfChanged(ref field, value);
        }
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
            InvokeEvent();
            Description = Expression;
        }
    }

    public override string TypeName => "ImplicitFunction";
    private void CharValueChanged(EnglishCharEnum englishCharEnum)
    {
        if (IsCorrect && !IsDeleted)
        {
            if (Function.Reference.HasFlag(englishCharEnum))
            {
                InvokeEvent();
            }
        }
    }
    
    
}