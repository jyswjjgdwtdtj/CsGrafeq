using System;
using System.Collections.Concurrent;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Styling;
using CsGrafeq.I18N;
using CsGrafeq.Interval;
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
        throw new NotImplementedException();
    }

    public override void SelectAll()
    {
        throw new NotImplementedException();
    }

    public override void DeselectAll()
    {
        throw new NotImplementedException();
    }

    private ImplicitFunction CreateAndAddFunctionCore(string exp)
    {
        var func = new ImplicitFunction(exp, this);
        Functions.Add(func);
        Layers.Add(func.RenderTarget);
        func.RenderTarget.RenderTargetSize =
            new SKSizeI((int)(Owner?.Bounds.Size.Width ?? 1), (int)(Owner?.Bounds.Size.Height ?? 1));
        func.RenderTarget.OnRender += (dc, rect) => RenderFunction(dc,
            new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom), func);
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

    private void RenderFunction(SKCanvas dc, SKRectI rect, ImplicitFunction impFunc)
    {
        if (!impFunc.IsCorrect)
            return;
        if (impFunc.IsDeleted)
            return;
        lock (IntervalCompiler.SyncObjForIntervalSetCalc)
        {
            var RectToCalc = new ConcurrentBag<SKRectI> { rect };
            var Points = new ConcurrentBag<SKPoint>();
            var Rects = new ConcurrentBag<SKRectI>();
            var paint = new SKPaint { Color = new SKColor(impFunc.Color).WithAlpha(impFunc.Opacity) };
            var func = impFunc.Function.Function;
            do
            {
                var rs = RectToCalc.ToArray();
                RectToCalc.Clear();
                Action<int> atn = idx => { RenderRectIntervalSet(rs[idx], RectToCalc, func, false, Points, Rects); };
                for (var i = 0; i < rs.Length; i += 100)
                {
                    //此处不应使用并行
                    //IntervalSet计算是完全线程不安全的
                    for (var j = i; j < Min(i + 100, rs.Length); j++) atn(j);
                    foreach (var rectToDraw in Rects) dc.DrawRect(rectToDraw, paint);
                    Rects.Clear();
                    dc.DrawPoints(SKPointMode.Points, Points.ToArray(), paint);
                    Points.Clear();
                }
            } while (RectToCalc.Count != 0);

            dc.Flush();
        }
    }

    private void RenderRectIntervalSet(SKRectI r, ConcurrentBag<SKRectI> RectToCalc, IntervalHandler<IntervalSet> func,
        bool checkpixel, ConcurrentBag<SKPoint> Points, ConcurrentBag<SKRectI> Rects)
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
            var di = i;
            var xmin = PixelToMathX(i);
            var xmax = PixelToMathX(i + dx);
            var xi = IntervalSet.Create([new Range(xmin, xmax)], Def.TT);
            for (var j = r.Top; j < r.Bottom; j += dy)
            {
                var dj = j;
                var ymin = PixelToMathY(j);
                var ymax = PixelToMathY(j + dy);
                var yi = IntervalSet.Create([new Range(ymin, ymax)], Def.TT);
                var result = func(xi, yi);
                if (result == Def.TT)
                {
                    if (isPixel)
                        Points.Add(new SKPoint(di, dj));
                    else
                        Rects.Add(new SKRectI(di, dj, di + dx, dj + dy));
                }
                else if (result == Def.FT)
                {
                    if (isPixel)
                        Points.Add(new SKPoint(di, dj));
                    else
                        RectToCalc.Add(new SKRectI(i, j, Min(i + dx, r.Right),
                            Min(j + dy, r.Bottom)));
                }
            }
        }
    }
}