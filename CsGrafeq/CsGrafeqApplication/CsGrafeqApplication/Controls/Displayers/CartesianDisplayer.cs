using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using CsGrafeq.Numeric;
using CsGrafeqApplication.Events;
using SkiaSharp;
using static CsGrafeqApplication.Core.Utils.PointRectHelper;
using static CsGrafeqApplication.Core.Utils.StaticSkiaResources;
using BigPoint= CsGrafeq.PointBase<CsGrafeq.Numeric.BigNumber<long,double>>;


namespace CsGrafeqApplication.Controls.Displayers;

public class CartesianDisplayer : Displayer
{
    private Lock LockTargetForPreviousBuffer { get; } = new();
    /// <summary>
    /// 记录距离上次Zoom的时间
    /// </summary>
    private Stopwatch WheelingStopWatch { get; } = new();
    /// <summary>
    /// 循环检测距离上次Zoom是否超过一定时间 如果超过则重绘
    /// </summary>
    private Timer WheelingTimer { get; init; }
    /// <summary>
    /// 记录上一次重绘的Buffer
    /// </summary>
    private SKBitmap PreviousBuffer { get; set; } = new(1, 1);
    /// <summary>
    /// 代表上一次Zoom时单位长度，用于优化滚轮缩放和拖动时的连续绘制
    /// </summary>
    private double PreviousZoomUnitLength { get; set; }
    /// <summary>
    /// 代表上一次Zoom时原点位置，用于优化滚轮缩放和拖动时的连续绘制
    /// </summary>
    private BigPoint PreviousZoomZero { get; set; }

    public CartesianDisplayer()
    {
        AxisY=AxisX = [];
        AxisPaint1 = AxisPaint2 =AxisPaintMain = new SKPaint();
        ZeroPos = new(){ X = 500, Y = 250 };
        UnitLength = 20;
        WheelingTimer = new Timer(TimerElapsed, null, 0, 500);
        Application.Current?.ActualThemeVariantChanged += (_,_) =>
        {
            RefreshPaint();
            ForceToRender(CancellationToken.None);
        };
        RefreshPaint();
    }

    /// <summary>
    ///     数学坐标原点的像素位置
    /// </summary>
    public BigPoint ZeroPos
    {
        get => field;
        set
        {
            field = value;
            AxisX = GetAxisXs().ToArray();
            AxisY = GetAxisYs().ToArray();
        }
    }

    /// <summary>
    ///     背景
    /// </summary>
    public SKColor AxisBackground { get; private set; }

    /// <summary>
    ///     主轴
    /// </summary>
    public SKPaint AxisPaintMain { get; private set; }

    /// <summary>
    ///     副轴
    /// </summary>
    public SKPaint AxisPaint1 { get; private set; }

    /// <summary>
    ///     普通线
    /// </summary>
    public SKPaint AxisPaint2 { get; private set; }

    /// <summary>
    ///     单位长度（每单位数学长度对应的像素长度）
    /// </summary>
    public double UnitLength
    {
        get => field;
        set
        {
            field = value;
            AxisX = GetAxisXs().ToArray();
            AxisY = GetAxisYs().ToArray();
        }
        // ReSharper disable once MemberInitializerValueIgnored
    } = 20.0001d;

    /// <summary>
    ///     横向坐标轴线的位置和类型
    /// </summary>
    public (double, AxisType)[] AxisX { get; private set; }

    /// <summary>
    ///     纵向坐标轴线的位置和类型
    /// </summary>
    public (double, AxisType)[] AxisY { get; private set; }

    /// <summary>
    ///     重新刷新绘制用的画笔
    /// </summary>
    protected void RefreshPaint()
    {
        if (Application.Current?.ActualThemeVariant == ThemeVariant.Light)
        {
            AxisBackground = SKColors.White;
            AxisPaintMain = new SKPaint { Color = SKColors.Black };
            AxisPaint1 = new SKPaint { Color = new SKColor(190, 190, 190) };
            AxisPaint2 = new SKPaint { Color = new SKColor(120, 120, 120) };
        }
        else
        {
            AxisBackground = new SKColor(30, 30, 30);
            AxisPaintMain = new SKPaint { Color = new SKColor(240, 240, 240) };
            AxisPaint1 = new SKPaint { Color = new SKColor(120, 120, 120) };
            AxisPaint2 = new SKPaint { Color = new SKColor(170, 170, 170) };
        }
    }

    public override double MathToPixelX(double d)
    {
        return ZeroPos.X + d * UnitLength;
    }

    public override double MathToPixelY(double d)
    {
        return ZeroPos.Y + -d * UnitLength;
    }

    public override double PixelToMathX(double d)
    {
        return (d - ZeroPos.X) / UnitLength;
    }

    public override double PixelToMathY(double d)
    {
        return -(d - ZeroPos.Y) / UnitLength;
    }

    /// <summary>
    ///     获取横向坐标轴线的位置和类型
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(double, AxisType)> GetAxisXs()
    {
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = Pow(10D, zsX);
        var addnumDx = SpecialPow(10M, zsX);
        for (var i = Min(ZeroPos.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (num % (10 * addnumDx) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        for (var i = Max(ZeroPos.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (num % (10 * addnumDx) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        if (RangeIn(ValidRect.Left, ValidRect.Right, ZeroPos.X))
            yield return (ZeroPos.X, AxisType.Axes);
    }

    /// <summary>
    ///     获取纵向坐标轴线的位置和类型
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(double, AxisType)> GetAxisYs()
    {
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumY = Pow(10D, zsY);
        var addnumDy = SpecialPow(10M, zsY);
        for (var i = Min(ZeroPos.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Right), -zsY)));
             i > ValidRect.Left;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (num % (10 * addnumDy) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        for (var i = Max(ZeroPos.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Left), -zsY)));
             i < ValidRect.Right;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (num % (10 * addnumDy) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        if (RangeIn(ValidRect.Left, ValidRect.Right, ZeroPos.Y))
            yield return (ZeroPos.Y, AxisType.Axes);
    }

    /// <summary>
    ///     绘制坐标轴线
    /// </summary>
    /// <param name="dc"></param>
    protected void RenderAxes(SKCanvas dc)
    {
        var setting = Setting.Instance;
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (!setting.ShowAxes)
            return;
        if (setting is { ShowAxesMajorGrid: false, ShowAxesMinorGrid: false })
            return;
        //y
        if (RangeIn(ValidRect.Left, ValidRect.Right, ZeroPos.X))
            dc.DrawLine(new SKPoint((float)ZeroPos.X.ToDecimal(), (float)ValidRect.Top),
                new SKPoint((float)ZeroPos.X.ToDecimal(), (float)ValidRect.Bottom), AxisPaintMain);

        if (RangeIn(0, height, ZeroPos.Y))
            dc.DrawLine(new SKPoint((float)ValidRect.Left, (float)ZeroPos.Y.ToDecimal()),
                new SKPoint((float)ValidRect.Right, (float)ZeroPos.Y.ToDecimal()), AxisPaintMain);
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = SpecialPow(10D, zsX);
        var addnumY = SpecialPow(10D, zsY);
        var addnumDx = SpecialPow(10M, zsX);
        var addnumDy = SpecialPow(10M, zsY);
        for (var i = Min(ZeroPos.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (setting.ShowAxesMajorGrid)
                dc.DrawLine(
                    (float)i, 0,
                    (float)i, (float)height,
                    AxisPaint1
                );

            if (setting.ShowAxesMinorGrid && num % (10 * addnumDx) == 0)
                dc.DrawLine(
                    (float)i, 0,
                    (float)i, (float)height,
                    AxisPaint2
                );
        }

        for (var i = Max(ZeroPos.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (setting.ShowAxesMajorGrid)
                dc.DrawLine(
                    (float)i, 0,
                    (float)i, (float)height,
                    AxisPaint1
                );

            if (setting.ShowAxesMinorGrid && num % (10 * addnumDx) == 0)
                dc.DrawLine(
                    (float)i, 0,
                    (float)i, (float)height,
                    AxisPaint2
                );
        }

        for (var i = Min(ZeroPos.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY)));
             i > ValidRect.Top;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (setting.ShowAxesMajorGrid)
                dc.DrawLine(
                    0, (float)i,
                    (float)width, (float)i,
                    AxisPaint1
                );
            if (setting.ShowAxesMinorGrid && num % (10 * addnumDy) == 0)
                dc.DrawLine(
                    0, (float)i,
                    (float)width, (float)i,
                    AxisPaint2
                );
        }

        for (var i = Max(ZeroPos.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY)));
             i < ValidRect.Bottom;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (setting.ShowAxesMajorGrid)
                dc.DrawLine(
                    0, (float)i,
                    (float)width, (float)i,
                    AxisPaint1
                );
            if (setting.ShowAxesMinorGrid && num % (10 * addnumDy) == 0)
                dc.DrawLine(
                    0, (float)i,
                    (float)width, (float)i,
                    AxisPaint2
                );
        }
    }

    /// <summary>
    ///     绘制坐标轴数字
    /// </summary>
    /// <param name="dc"></param>
    protected void RenderAxesNumber(SKCanvas dc)
    {
        if (!Setting.Instance.ShowAxesNumber)
            return;
        var textFont = MapleMono;
        var width = Bounds.Width;
        var height = Bounds.Height;
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = SpecialPow(10D, zsX);
        var addnumY = SpecialPow(10D, zsY);
        var p = RangeTo(1, height - textFont.Size - 2, ZeroPos.Y);
        var fff = 1f / 4f * textFont.Size;
        for (var i = Min(ZeroPos.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            dc.DrawText(num.ToString(),
                new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + textFont.Size),
                SKTextAlign.Left, textFont, AxisPaintMain);
        }

        for (var i = Max(ZeroPos.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            dc.DrawText(num.ToString(),
                new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + textFont.Size),
                SKTextAlign.Left, textFont, AxisPaintMain);
        }

        for (var i = Min(ZeroPos.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY)));
             i > ValidRect.Top;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (ValidRect.Left + 3 > ZeroPos.X)
                dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)),
                    SKTextAlign.Left, textFont, AxisPaintMain);
            else if (ZeroPos.X + num.ToString().Length * textFont.Size / 2 > ValidRect.Right - 3)
                dc.DrawText(num.ToString(),
                    new SKPoint((float)width - num.ToString().Length * textFont.Size / 2 - 5, (float)(i + 4)),
                    SKTextAlign.Left, textFont, AxisPaintMain);
            else
                dc.DrawText(num.ToString(), new SKPoint((float)ZeroPos.X.ToDecimal(), (float)(i + 4)), SKTextAlign.Left, textFont,
                    AxisPaintMain);
        }

        for (var i = Max(ZeroPos.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY)));
             i < ValidRect.Bottom;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (ValidRect.Left + 3 > ZeroPos.X)
                dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)),
                    SKTextAlign.Left, textFont, AxisPaintMain);
            else if (ZeroPos.X + num.ToString().Length * textFont.Size / 2 > ValidRect.Right - 3)
                dc.DrawText(num.ToString(),
                    new SKPoint((float)width - num.ToString().Length * textFont.Size / 2 - 5, (float)(i + 4)),
                    SKTextAlign.Left, textFont, AxisPaintMain);
            else
                dc.DrawText(num.ToString(), new SKPoint((float)ZeroPos.X.ToDecimal(), (float)(i + 4)), SKTextAlign.Left, textFont,
                    AxisPaintMain);
        }

        dc.DrawText("0", new SKPoint((float)ZeroPos.X.ToDecimal() + 3, (float)ZeroPos.Y .ToDecimal()+ textFont.Size), SKTextAlign.Left, textFont,
            AxisPaintMain);
    }

    public override void CompoundBuffers()
    {
        if(ContainerViewModel is {} vm)
            lock (TotalBufferLock)
            {
                using var dc = new SKCanvas(TotalBuffer);
                dc.Clear(AxisBackground);
                RenderAxes(dc);
                for (var i = 0; i < Addons.Count; i++)
                {
                    if(i!=vm.AddonIndex)
                        foreach (var layer in Addons[i].Layers)
                            layer.DrawRenderTargetTo(dc, 0, 0);
                }
                foreach (var layer in Addons[vm.AddonIndex].Layers)
                    layer.DrawRenderTargetTo(dc, 0, 0);
                RenderAxesNumber(dc);
            }
    }
    /// <summary>
    /// 如距离上次Zoom过去一定时间 则重绘
    /// </summary>
    /// <param name="arg"></param>
    private void TimerElapsed(object? arg)
    {
        if (WheelingStopWatch.IsRunning && WheelingStopWatch.ElapsedMilliseconds > 150)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            Dispatcher.UIThread.InvokeAsync((() => ForceToRender(CancellationToken.None)));
        }
    }
    /// <summary>
    /// 用于在Zoom过后 当鼠标移动则重绘
    /// </summary>
    protected void StopWheeling()
    {
        if (WheelingStopWatch.IsRunning)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            ForceToRender(CancellationToken.None);
        }
    }
    /// <summary>
    ///     newvalue=unitlength*delta
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="point"></param>
    public override void Zoom(double delta, Point point)
    {
        if (!WheelingStopWatch.IsRunning)
        {
            PreviousZoomUnitLength = UnitLength;
            PreviousZoomUnitLength = UnitLength;
            PreviousZoomZero = ZeroPos;
            WheelingStopWatch.Restart();
            if (Setting.Instance.ZoomOptimization)
                lock (LockTargetForPreviousBuffer)
                {
                    lock (TotalBufferLock)
                    {
                        if (PreviousBuffer.Width != TotalBuffer.Width || PreviousBuffer.Height != TotalBuffer.Height)
                        {
                            PreviousBuffer.Dispose();
                            PreviousBuffer = new SKBitmap(TotalBuffer.Width, TotalBuffer.Height);
                        }

                        TotalBuffer.TryRealCopyTo(PreviousBuffer);
                    }
                }
        }

        var (dx, dy) = point;
        var x = new BigNumber<long, double>(0,dx);
        var y = new BigNumber<long, double>(0,dy);
        UnitLength *= delta;
        UnitLength *= delta;
        UnitLength = RangeTo(0.01, 1000000, UnitLength);
        UnitLength = RangeTo(0.01, 1000000, UnitLength);
        var ratioX = UnitLength / PreviousZoomUnitLength;
        var ratioY = UnitLength / PreviousZoomUnitLength;
        ZeroPos = new()
        {
            X = (x - (x - PreviousZoomZero.X) * ratioX),
            Y = (y - (y - PreviousZoomZero.Y) * ratioY)
        };
        if (!Setting.Instance.ZoomOptimization || ratioX > 2 || ratioX < 0.5 || ratioY > 2 || ratioY < 0.5)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            PreviousZoomUnitLength = UnitLength;
            PreviousZoomUnitLength = UnitLength;
            PreviousZoomZero = ZeroPos;
            ForceToRender(CancellationToken.None);
            return;
        }

        lock (TotalBufferLock)
        {
            using var dc = new SKCanvas(TotalBuffer);
            dc.Clear(AxisBackground);
            dc.DrawBitmap(PreviousBuffer, CreateSKRectWH(
                (float)(ZeroPos.X - PreviousZoomZero.X * ratioX),
                (float)(ZeroPos.Y - PreviousZoomZero.Y * ratioY),
                (float)(ratioX * PreviousBuffer.Width),
                (float)(ratioY * PreviousBuffer.Height)), SkiaHelper.CompoundBufferPaint);
        }

        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (CallPointerWheeled(e.Cast(this)) == DoNext)
            Zoom(Pow(1.04, e.Delta.Y), e.GetPosition(this));
        else
            AskForRender();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        AxisX = GetAxisXs().ToArray();
        AxisY = GetAxisYs().ToArray();
    }
}

public enum AxisType
{
    Axes,
    Major,
    Minor
}