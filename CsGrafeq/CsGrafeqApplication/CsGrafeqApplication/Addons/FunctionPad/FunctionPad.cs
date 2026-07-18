using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Styling;
using CsGrafeq.Bitmap;
using CsGrafeq.I18N;
using CsGrafeq.Interval;
using CsGrafeq.Setting;
using CsGrafeqApplication.Function;
using CsGrafeqApplication.Utilities;
using SkiaSharp;
using Range = CsGrafeq.Interval.Range;

namespace CsGrafeqApplication.Addons.FunctionPad;

public class FunctionPad : Addon
{
    public FunctionPad()
    {
        AddonName = MultiLanguageResources.Instance.FunctionPadText;
        var host = new Control();
        host.Resources.MergedDictionaries.Add(
            new ResourceInclude(new Uri("avares://CsGrafeqApplication/"))
            {
                Source = new Uri("avares://CsGrafeqApplication/Addons/FunctionPad/FunctionPadResources.axaml")
            });
        host.TryFindResource("FunctionPadViewTemplate", out var obj);
        MainTemplate = (IDataTemplate)obj!;
        host.TryFindResource("FunctionPadInfoTemplate", out obj);
        InfoTemplate = (IDataTemplate)obj!;
        Functions.CollectionChanged += (_,_) =>
        {
            Setting.Instance.MoveOptimizationUserEnabled =
                Setting.Instance.ZoomOptimizationUserEnabled = Functions.Count == 0;
            if (Functions.Count != 0) Setting.Instance.MoveOptimization = Setting.Instance.ZoomOptimization = true;
        };
    }

    /// <summary>
    ///     请勿自行添加或删除此列表中的元素，应使用CreateAndAddFunction和DeleteFunction方法
    /// </summary>
    public AvaloniaList<ImplicitFunction> Functions { get; } = new();

    public override void Delete()
    {
        DeleteFunction(Functions.Where(f=>f.IsSelected).ToArray());
    }

    public override void SelectAll()
    {
        foreach (var implicitFunction in Functions)
        {
            implicitFunction.IsSelected = true;
        }
    }

    public override void DeselectAll()
    {
        foreach (var implicitFunction in Functions)
        {
            implicitFunction.IsSelected = false;
        }
    }

    private ImplicitFunction CreateAndAddFunctionCore(string exp,bool needPixelCheck=false)
    {
        var func = new ImplicitFunction(exp, this);
        func.NeedPixelCheck = needPixelCheck;
        Functions.Add(func);
        Layers.Add(func.RenderTarget);
        func.RenderTarget.RenderTargetSize =
            new SKSizeI((int)(Owner?.Bounds.Size.Width ?? 1), (int)(Owner?.Bounds.Size.Height ?? 1));
        func.RenderTarget.OnRender += (dc, rect,ct) => RenderFunction(dc,
            new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom), func,ct);
        func.FuncChanged += f =>
        {
            f.RenderTarget.Changed = true;
            Owner?.AskForRender();
        };
        return func;
    }

    public ImplicitFunction CreateAndAddFunction(string exp,bool needCheck)
    {
        var func = CreateAndAddFunctionCore(exp);
        CommandHelper.CommandManager.Do(
            null,
            _ => { },
            _ => { func.IsDeleted = false; },
            _ => { func.IsDeleted = true; },
            _ => { DeleteFunctionCore(func); }, true
        );
        return func;
    }

    private void DeleteFunction(ImplicitFunction[] functions)
    {
        var fs = functions.Where(f => f.Owner == this).ToArray();
        CommandHelper.CommandManager.Do(
            null,
            _ => {
                foreach (var f in fs)
                {
                    f.IsDeleted = true;
                    f.IsSelected = false;
                }
            },
            _ => { 
                foreach (var f in fs)
                {
                    f.IsDeleted = true;
                } },
            _ => { 
                foreach (var f in fs)
                {
                    f.IsDeleted = false;
                } },
            _ => {
                foreach (var f in fs)
                {
                    f.IsDeleted = false;
                } }
        );
    }
    public void DeleteFunction(ImplicitFunction func)
    {
        if (func.Owner != this)
            return;
        CommandHelper.CommandManager.Do(
            null,
            _ => { func.IsDeleted = true; },
            _ => { func.IsDeleted = true; },
            _ => { func.IsDeleted = false; }, 
            _ => { func.IsDeleted = false; }
        );
    }

    private void DeleteFunctionCore(ImplicitFunction func)
    {
        Functions.Remove(func);
        Layers.Remove(func.RenderTarget);
        func.Dispose();
    }

    private void RenderFunction(SKCanvas dc, SKRectI rect, ImplicitFunction impFunc,CancellationToken ct)
    {
        if (!impFunc.IsCorrect)
            return;
        if (impFunc.IsDeleted)
            return;
        using var paint = new SKPaint();
        paint.IsAntialias = false;
        paint.Style = SKPaintStyle.Fill;
        paint.Color = new SKColor(impFunc.Color).WithAlpha(impFunc.Opacity);
        paint.BlendMode = SKBlendMode.Src;
        var rectToCalc = new ConcurrentBag<SKRectI> { rect };
        var rectToRender = new ConcurrentBag<SKRectI>();
        var pixelToRender = new ConcurrentBag<SKPoint>();
        var pointColor=new SKColor(impFunc.Color).WithAlpha(impFunc.Opacity).ToUint();
        var func = impFunc.Function.Function;
        Func<double,double,double,double,bool> msFunc =impFunc.NeedPixelCheck?impFunc.MsFunction: static (_,_,_,_ )=> true;
        do
        {
            var rs = rectToCalc.ToArray();
            rectToCalc.Clear();
            var len = rs.Length;
            for(var i=0;i<len;i+=100)
            {
                /*for(var j=i;j<Min(i+100,len);j++)
                {
                    RenderAction(rs[j]);
                }
                */
                Parallel.For(i, Min(i + 100, len), (j) =>
                {
                    RenderAction(rs[j]);
                });
                dc.DrawPoints(SKPointMode.Points, pixelToRender.ToArray(),paint);
                foreach (var r in rectToRender)
                {
                    dc.DrawRect(r, paint);
                }
                pixelToRender.Clear();
                rectToRender.Clear();
            }
            dc.Flush();
            continue;
            void RenderAction(SKRectI r)
            {
                RenderRectIntervalSet(r, rectToCalc,pixelToRender,rectToRender, func, msFunc);
            }   
        } while (rectToCalc.Count != 0);
    }

    private void RenderRectIntervalSet(SKRectI r, ConcurrentBag<SKRectI> rectToCalc,ConcurrentBag<SKPoint> pixelsToRender,ConcurrentBag<SKRectI> rectToRender, IntervalHandler<IntervalSet> func,Func<double,double,double,double,bool> msFunc)
    {
        if (r.Height == 0 || r.Width == 0)
            return;
        int xTimes = 2, yTimes = 2;
        if (r.Width > r.Height)
            yTimes = 1;
        else if (r.Width < r.Height)
            xTimes = 1;
        var dx = (int)Ceiling((double)r.Width / xTimes);
        var dy = (int)Ceiling((double)r.Height / yTimes);
        var isPixel = dx == 1 && dy == 1;
        for (var i = r.Left; i < r.Right; i += dx)
        {
            var xMin = PixelToMathX(i);
            var xMax = PixelToMathX(i + dx);
            var xi = IntervalSet.Create([new Range(xMin, xMax)], Def.TT);
            for (var j = r.Top; j < r.Bottom; j += dy)
            {
                var yMin = PixelToMathY(j);
                var yMax = PixelToMathY(j + dy);
                var yi = IntervalSet.Create([new Range(yMin, yMax)], Def.TT);
                var result = func(xi, yi);
                if (result == Def.TT)
                {
                    if (isPixel)
                        pixelsToRender.Add(new(i,j));
                    else
                        rectToRender.Add(new(i, j, i+dx, j+dy));
                }
                else if (result == Def.FT)
                {
                    if (isPixel)
                    {
                        if(msFunc(xMin,yMin,xMax,yMax))
                            pixelsToRender.Add(new(i,j));
                    }
                    else
                    {
                        rectToCalc.Add(new SKRectI(i, j, Min(i + dx, r.Right),
                            Min(j + dy, r.Bottom)));
                    }
                }
            }
        }
    }
}