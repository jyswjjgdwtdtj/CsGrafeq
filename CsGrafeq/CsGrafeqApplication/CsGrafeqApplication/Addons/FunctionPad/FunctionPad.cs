using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Styling;
using CsGrafeq.Bitmap;
using CsGrafeq.I18N;
using CsGrafeq.Interval;
using CsGrafeq.Numeric;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.Function;
using CsGrafeqApplication.Utilities;
using CSharpMath.Rendering.Text;
using SkiaSharp;
using Range = CsGrafeq.Interval.Range;

namespace CsGrafeqApplication.Addons.FunctionPad;

public class FunctionPad : Addon
{
    public FunctionPad()
    {
        AddonName = MultiLanguageResources.Instance.FunctionPadText;
        var host = new Control();
        object? obj;
        host.Resources.MergedDictionaries.Add(
            new ResourceInclude(new Uri("avares://CsGrafeqApplication/"))
            {
                Source = new Uri("avares://CsGrafeqApplication/Addons/FunctionPad/FunctionPadResources.axaml")
            });
        host.TryFindResource("FunctionPadViewTemplate", out obj);
        MainTemplate = (IDataTemplate)obj;
        host.TryFindResource("FunctionPadInfoTemplate", out obj);
        InfoTemplate = (IDataTemplate)obj;
        Functions.CollectionChanged += (s, e) =>
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
        DeleteFunction(Functions.Where(f=>f.IsSelected==true).ToArray());
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

    private ImplicitFunction CreateAndAddFunctionCore(string exp)
    {
        var func = new ImplicitFunction(exp, this);
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

    public ImplicitFunction CreateAndAddFunction(string exp)
    {
        var func = CreateAndAddFunctionCore(exp);
        CommandHelper.CommandManager.Do(
            null,
            _ => { },
            o => { func.IsDeleted = false; },
            o => { func.IsDeleted = true; },
            o => { DeleteFunctionCore(func); }, true
        );
        return func;
    }

    public void DeleteFunction(ImplicitFunction[] funcs)
    {
        var fs = funcs.Where(f => f.Owner == this).ToArray();
        CommandHelper.CommandManager.Do(
            null,
            _ => {
                foreach (var f in fs)
                {
                    f.IsDeleted = true;
                    f.IsSelected = false;
                }
            },
            o => { 
                foreach (var f in fs)
                {
                    f.IsDeleted = true;
                } },
            o => { 
                foreach (var f in fs)
                {
                    f.IsDeleted = false;
                } },
            o => {
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
            o => { func.IsDeleted = true; },
            o => { func.IsDeleted = false; },
            o => { func.IsDeleted = false; }
        );
    }

    private void DeleteFunctionCore(ImplicitFunction func)
    {
        Functions.Remove(func);
        Layers.Remove(func.RenderTarget);
        func.Dispose();
    }

    private void RenderFunction(PixelBitmap dc, SKRectI rect, ImplicitFunction impFunc,CancellationToken ct)
    {
        Console.WriteLine(rect);
        if (!impFunc.IsCorrect)
            return;
        if (impFunc.IsDeleted)
            return;
        var rectToCalc = new ConcurrentBag<SKRectI> { rect };
        var pointColor=new SKColor(impFunc.Color).WithAlpha(impFunc.Opacity).ToUint();
        var func = impFunc.Function.Function;
        dc.Color=pointColor;
        Func<double,double,double,double,bool> msFunc =impFunc.NeedCheckPixel?impFunc.MsFunction: static (_,_,_,_ )=> true;
        do
        {
            var rs = rectToCalc.ToArray();
            rectToCalc.Clear();
            var len = rs.Length;
            for(var i=0;i<len;i+=100)
            {
                for(var j=i;j<Min(i+100,len);j++)
                {
                    if (ct.IsCancellationRequested)
                        return;
                    RenderAction(rs[j]);
                }
                if(ct.IsCancellationRequested)
                    return;
            }
            dc.Flush();
            continue;
            void RenderAction(SKRectI r)
            {
                RenderRectIntervalSet(r, rectToCalc, func, msFunc, dc, pointColor);
            }   
        } while (rectToCalc.Count != 0);
    }

    private void RenderRectIntervalSet(SKRectI r, ConcurrentBag<SKRectI> rectToCalc, IntervalHandler<IntervalSet> func,Func<double,double,double,double,bool> msFunc,PixelBitmap bmp,uint color)
    {
        if (r.Height == 0 || r.Width == 0)
            return;
        int xtimes = 2, ytimes = 2;
        if (r.Width > r.Height)
            ytimes = 1;
        else if (r.Width < r.Height)
            xtimes = 1;
        var dx = (int)Ceiling((double)r.Width / xtimes);
        var dy = (int)Ceiling((double)r.Height / ytimes);
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
                        bmp.SetPixel_Buffered(i,j);
                    else
                        bmp.SetRectangle(i, j, dx, dy, color);
                }
                else if (result == Def.FT)
                {
                    if (isPixel)
                    {
                        bmp.SetPixel_Buffered(i,j);
                    }
                    else
                    {
                        //bmp.SetRectangle(i, j, dx, dy, ((uint)Random.Shared.NextInt64(0xFFFFFF))|0xFF000000);
                        rectToCalc.Add(new SKRectI(i, j, Min(i + dx, r.Right),
                            Min(j + dy, r.Bottom)));
                    }
                }
            }
        }
    }
}