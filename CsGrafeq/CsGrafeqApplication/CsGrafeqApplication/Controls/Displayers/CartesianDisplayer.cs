using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using SkiaSharp;
using static CsGrafeqApplication.Controls.SkiaEx;

namespace CsGrafeqApplication.Controls.Displayers;

public class CartesianDisplayer : Displayer
{
    private readonly object LockTargetForPreviousBuffer = new();
    private readonly Stopwatch WheelingStopWatch = new();
    private readonly Timer WheelingTimer;
    public bool DrawAxisGrid = true;
    public bool DrawAxisLine = true;
    public bool DrawAxisNumber = true;
    protected bool MouseOnYAxis, MouseOnXAxis;
    private SKBitmap PreviousBuffer = new(1, 1);

    private double PreviousUnitLength;
    private PointL PreviousZero;

    public CartesianDisplayer()
    {
        Zero = new PointL { X = 500, Y = 250 };
        UnitLength = 20;
        WheelingTimer = new Timer(TimerElapsed, null, 0, 500);
        App.Current.ActualThemeVariantChanged += (s, e) =>
        {
            RefreshPaint();
            ForceToRender();
        };
        RefreshPaint();
    }

    /// <summary>
    ///     数学坐标原点的像素位置
    /// </summary>
    public PointL Zero
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
    } = 20.0001d;

    /// <summary>
    ///     横向坐标轴线的位置和类型
    /// </summary>
    public IEnumerable<(double, AxisType)> AxisX { get; private set; }

    /// <summary>
    ///     纵向坐标轴线的位置和类型
    /// </summary>
    public IEnumerable<(double, AxisType)> AxisY { get; private set; }

    /// <summary>
    ///     重新刷新绘制用的画笔
    /// </summary>
    protected void RefreshPaint()
    {
        if (App.Current.ActualThemeVariant == ThemeVariant.Light)
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
        return Zero.X + d * UnitLength;
    }

    public override double MathToPixelY(double d)
    {
        return Zero.Y + -d * UnitLength;
    }

    public override double PixelToMathX(double d)
    {
        return (d - Zero.X) / UnitLength;
    }

    public override double PixelToMathY(double d)
    {
        return -(d - Zero.Y) / UnitLength;
    }

    /// <summary>
    ///     获取横向坐标轴线的位置和类型
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(double, AxisType)> GetAxisXs()
    {
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        ;
        var addnumX = Pow(10D, zsX);
        var addnumDX = SpecialPow(10M, zsX);
        for (var i = Min(Zero.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (num % (10 * addnumDX) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        for (var i = Max(Zero.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (num % (10 * addnumDX) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        if (RangeIn(ValidRect.Left, ValidRect.Right, Zero.X))
            yield return (Zero.X, AxisType.Axes);
    }

    /// <summary>
    ///     获取纵向坐标轴线的位置和类型
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(double, AxisType)> GetAxisYs()
    {
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        ;
        var addnumY = Pow(10D, zsY);
        var addnumDY = SpecialPow(10M, zsY);
        for (var i = Min(Zero.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Right), -zsY)));
             i > ValidRect.Left;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (num % (10 * addnumDY) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        for (var i = Max(Zero.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Left), -zsY)));
             i < ValidRect.Right;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (num % (10 * addnumDY) == 0)
                yield return (i, AxisType.Major);
            else
                yield return (i, AxisType.Minor);
        }

        if (RangeIn(ValidRect.Left, ValidRect.Right, Zero.Y))
            yield return (Zero.Y, AxisType.Axes);
    }

    /// <summary>
    ///     绘制坐标轴线
    /// </summary>
    /// <param name="dc"></param>
    protected void RenderAxisLine(SKCanvas dc)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        //y
        if (RangeIn(ValidRect.Left, ValidRect.Right, Zero.X))
            dc.DrawLine(new SKPoint(Zero.X, (float)ValidRect.Top),
                new SKPoint(Zero.X, (float)ValidRect.Bottom), AxisPaintMain);

        if (RangeIn(0, height, Zero.Y))
            dc.DrawLine(new SKPoint((float)ValidRect.Left, Zero.Y),
                new SKPoint((float)ValidRect.Right, Zero.Y), AxisPaintMain);

        if (!DrawAxisGrid)
            return;
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = SpecialPow(10D, zsX);
        var addnumY = SpecialPow(10D, zsY);
        var addnumDX = SpecialPow(10M, zsX);
        var addnumDY = SpecialPow(10M, zsY);
        SKPaint targetPen;
        for (var i = Min(Zero.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (num % (10 * addnumDX) == 0)
                targetPen = AxisPaint2;
            else
                targetPen = AxisPaint1;

            dc.DrawLine(
                (float)i, 0,
                (float)i, (float)height,
                targetPen
            );
        }

        for (var i = Max(Zero.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            if (num % (10 * addnumDX) == 0)
                targetPen = AxisPaint2;
            else
                targetPen = AxisPaint1;

            dc.DrawLine(
                (float)i, 0,
                (float)i, (float)height,
                targetPen
            );
        }

        for (var i = Min(Zero.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY)));
             i > ValidRect.Top;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (num % (10 * addnumDY) == 0)
                targetPen = AxisPaint2;
            else
                targetPen = AxisPaint1;

            dc.DrawLine(
                0, (float)i,
                (float)width, (float)i,
                targetPen
            );
        }

        for (var i = Max(Zero.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY)));
             i < ValidRect.Bottom;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (num % (10 * addnumDY) == 0)
                targetPen = AxisPaint2;
            else
                targetPen = AxisPaint1;

            dc.DrawLine(
                0, (float)i,
                (float)width, (float)i,
                targetPen
            );
        }
    }

    /// <summary>
    ///     绘制坐标轴数字
    /// </summary>
    /// <param name="dc"></param>
    protected void RenderAxisNumber(SKCanvas dc)
    {
        var TextFont = MapleMono;
        if (!DrawAxisNumber)
            return;
        var width = Bounds.Width;
        var height = Bounds.Height;
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = SpecialPow(10D, zsX);
        var addnumY = SpecialPow(10D, zsY);
        var addnumDX = SpecialPow(10M, zsX);
        var addnumDY = SpecialPow(10M, zsY);
        var p = RangeTo(1, height - TextFont.Size - 2, Zero.Y);
        var fff = 1f / 4f * TextFont.Size;
        for (var i = Min(Zero.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            dc.DrawText(num.ToString(),
                new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + TextFont.Size),
                SKTextAlign.Left, TextFont, AxisPaintMain);
        }

        for (var i = Max(Zero.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            dc.DrawText(num.ToString(),
                new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + TextFont.Size),
                SKTextAlign.Left, TextFont, AxisPaintMain);
        }

        for (var i = Min(Zero.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY)));
             i > ValidRect.Top;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (ValidRect.Left + 3 > Zero.X)
                dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else if (Zero.X + num.ToString().Length * TextFont.Size / 2 > ValidRect.Right - 3)
                dc.DrawText(num.ToString(),
                    new SKPoint((float)width - num.ToString().Length * TextFont.Size / 2 - 5, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else
                dc.DrawText(num.ToString(), new SKPoint(Zero.X, (float)(i + 4)), SKTextAlign.Left, TextFont,
                    AxisPaintMain);
        }

        for (var i = Max(Zero.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY)));
             i < ValidRect.Bottom;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (ValidRect.Left + 3 > Zero.X)
                dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else if (Zero.X + num.ToString().Length * TextFont.Size / 2 > ValidRect.Right - 3)
                dc.DrawText(num.ToString(),
                    new SKPoint((float)width - num.ToString().Length * TextFont.Size / 2 - 5, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else
                dc.DrawText(num.ToString(), new SKPoint(Zero.X, (float)(i + 4)), SKTextAlign.Left, TextFont,
                    AxisPaintMain);
        }

        dc.DrawText("0", new SKPoint(Zero.X + 3, Zero.Y + TextFont.Size), SKTextAlign.Left, TextFont,
            AxisPaintMain);
    }

    public override void CompoundBuffers()
    {
        lock (TotalBufferLock)
        {
            using (var dc = new SKCanvas(TotalBuffer))
            {
                dc.Clear(AxisBackground);
                RenderAxisLine(dc);
                foreach (var i in Addons)
                foreach (var layer in i.Layers)
                    layer.DrawBitmap(dc, 0, 0);
                RenderAxisNumber(dc);
            }
        }
    }

    private void TimerElapsed(object? arg)
    {
        if (WheelingStopWatch.IsRunning && WheelingStopWatch.ElapsedMilliseconds > 150)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            Dispatcher.UIThread.InvokeAsync(ForceToRender);
        }
    }

    protected void StopWheeling()
    {
        if (WheelingStopWatch.IsRunning)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            ForceToRender();
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
            PreviousUnitLength = UnitLength;
            PreviousUnitLength = UnitLength;
            PreviousZero = Zero;
            WheelingStopWatch.Restart();
            if (ZoomingOptimization)
                lock (LockTargetForPreviousBuffer)
                {
                    lock (TotalBufferLock)
                    {
                        PreviousBuffer.Dispose();
                        PreviousBuffer = TotalBuffer.Copy();
                    }
                }
        }

        var (x, y) = point;
        var bzero = Zero;
        var times_x = (Zero.X - x) / UnitLength;
        var times_y = (Zero.Y - y) / UnitLength;
        UnitLength *= delta;
        UnitLength *= delta;
        UnitLength = RangeTo(0.01, 1000000, UnitLength);
        UnitLength = RangeTo(0.01, 1000000, UnitLength);
        var ratioX = UnitLength / PreviousUnitLength;
        var ratioY = UnitLength / PreviousUnitLength;
        Zero = new PointL
        {
            X = (long)((long)x - ((long)x - PreviousZero.X) * ratioX),
            Y = (long)((long)y - ((long)y - PreviousZero.Y) * ratioY)
        };
        if (!ZoomingOptimization || ratioX > 2 || ratioX < 0.5 || ratioY > 2 || ratioY < 0.5)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            PreviousUnitLength = UnitLength;
            PreviousUnitLength = UnitLength;
            PreviousZero = Zero;
            ForceToRender();
            return;
        }

        lock (TotalBufferLock)
        {
            using (var dc = new SKCanvas(TotalBuffer))
            {
                dc.Clear(AxisBackground);
                dc.DrawBitmap(PreviousBuffer, CreateSKRectWH(
                    (float)(Zero.X - PreviousZero.X * ratioX),
                    (float)(Zero.Y - PreviousZero.Y * ratioY),
                    (float)(ratioX * PreviousBuffer.Width),
                    (float)(ratioY * PreviousBuffer.Height)), AntiAlias);
            }
        }

        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (CallAddonPointerWheeled(e) == DoNext)
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