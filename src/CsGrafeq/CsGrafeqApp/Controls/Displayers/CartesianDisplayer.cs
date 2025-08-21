using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using SkiaSharp;
using static CsGrafeqApp.Controls.SkiaEx;

namespace CsGrafeqApp.Controls.Displayers;

public class CartesianDisplayer : Displayer
{
    private readonly Stopwatch WheelingStopWatch = new();
    private readonly Timer WheelingTimer;

    protected PointL _Zero = new() { X = 500, Y = 250 };
    public bool DrawAxisGrid = true;
    public bool DrawAxisLine = true;
    public bool DrawAxisNumber = true;
    protected bool MouseOnYAxis, MouseOnXAxis;
    private SKBitmap PreviousBuffer = new();

    private double PreviousUnitLength;
    private PointL PreviousZero;

    public CartesianDisplayer()
    {
        WheelingTimer = new Timer(TimerElapsed, null, 0, 500);
        App.Current.ActualThemeVariantChanged += (s, e) =>
        {
            RefreshPaint();
            InvalidateBuffer();
        };
        Console.WriteLine(App.Current.ActualThemeVariant);
        RefreshPaint();
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

    public double UnitLength { get; set; } = 20.0001d;

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
        return _Zero.X + d * UnitLength;
    }

    public override double MathToPixelY(double d)
    {
        return _Zero.Y + -d * UnitLength;
    }

    public override double PixelToMathX(double d)
    {
        return (d - _Zero.X) / UnitLength;
    }

    public override double PixelToMathY(double d)
    {
        return -(d - _Zero.Y) / UnitLength;
    }

    //不敢动………………
    protected void RenderAxisLine(SKCanvas dc)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        //y
        if (RangeIn(ValidRect.Left, ValidRect.Right, _Zero.X))
            dc.DrawLine(new SKPoint(_Zero.X, (float)ValidRect.Top),
                new SKPoint(_Zero.X, (float)ValidRect.Bottom), AxisPaintMain);

        if (RangeIn(0, height, _Zero.Y))
            dc.DrawLine(new SKPoint((float)ValidRect.Left, _Zero.Y),
                new SKPoint((float)ValidRect.Right, _Zero.Y), AxisPaintMain);

        if (!DrawAxisGrid)
            return;
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = Pow(10D, zsX);
        var addnumY = Pow(10D, zsY);
        var addnumDX = Pow(10M, zsX);
        var addnumDY = Pow(10M, zsY);
        var p = RangeTo(-3, height - TextFont.Size + 1, _Zero.Y);
        SKPaint targetPen;
        for (var i = Min(_Zero.X - addnumX * UnitLength,
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

        for (var i = Max(_Zero.X + addnumX * UnitLength,
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

        for (var i = Min(_Zero.Y - addnumY * UnitLength,
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

        for (var i = Max(_Zero.Y + addnumY * UnitLength,
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

    protected void RenderAxisNumber(SKCanvas dc)
    {
        if (!DrawAxisNumber)
            return;
        var width = Bounds.Width;
        var height = Bounds.Height;
        var zsX = (int)Floor(Log(350 / UnitLength, 10));
        var zsY = (int)Floor(Log(350 / UnitLength, 10));
        var addnumX = Pow(10D, zsX);
        var addnumY = Pow(10D, zsY);
        var addnumDX = Pow(10M, zsX);
        var addnumDY = Pow(10M, zsY);
        var p = RangeTo(-3, height - TextFont.Size + 1, _Zero.Y);
        var fff = 1f / 4f * TextFont.Size;
        for (var i = Min(_Zero.X - addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX)));
             i > ValidRect.Left;
             i -= addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            dc.DrawText(num.ToString(),
                new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + TextFont.Size),
                SKTextAlign.Left, TextFont, AxisPaintMain);
        }

        for (var i = Max(_Zero.X + addnumX * UnitLength,
                 MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX)));
             i < ValidRect.Right;
             i += addnumX * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathX(i), -zsX);
            dc.DrawText(num.ToString(),
                new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + TextFont.Size),
                SKTextAlign.Left, TextFont, AxisPaintMain);
        }

        for (var i = Min(_Zero.Y - addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY)));
             i > ValidRect.Top;
             i -= addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (ValidRect.Left + 3 > _Zero.X)
                dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else if (_Zero.X + num.ToString().Length * TextFont.Size / 2 > ValidRect.Right - 3)
                dc.DrawText(num.ToString(),
                    new SKPoint((float)width - num.ToString().Length * TextFont.Size / 2 - 5, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else
                dc.DrawText(num.ToString(), new SKPoint(_Zero.X, (float)(i + 4)), SKTextAlign.Left, TextFont,
                    AxisPaintMain);
        }

        for (var i = Max(_Zero.Y + addnumY * UnitLength,
                 MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY)));
             i < ValidRect.Bottom;
             i += addnumY * UnitLength)
        {
            var num = RoundTen((decimal)PixelToMathY(i), -zsY);
            if (ValidRect.Left + 3 > _Zero.X)
                dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else if (_Zero.X + num.ToString().Length * TextFont.Size / 2 > ValidRect.Right - 3)
                dc.DrawText(num.ToString(),
                    new SKPoint((float)width - num.ToString().Length * TextFont.Size / 2 - 5, (float)(i + 4)),
                    SKTextAlign.Left, TextFont, AxisPaintMain);
            else
                dc.DrawText(num.ToString(), new SKPoint(_Zero.X, (float)(i + 4)), SKTextAlign.Left, TextFont,
                    AxisPaintMain);
        }

        dc.DrawText("0", new SKPoint(_Zero.X + 3, _Zero.Y + TextFont.Size), SKTextAlign.Left, TextFont,
            AxisPaintMain);
    }

    public override void CompoundBuffer()
    {
        lock (TotalBuffer)
        {
            using (var dc = new SKCanvas(TotalBuffer))
            {
                dc.Clear(AxisBackground);
                RenderAxisLine(dc);
                foreach (var i in Addons) dc.DrawBitmap(i.Bitmap, new SKPoint(0, 0));

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
            Dispatcher.UIThread.InvokeAsync(Invalidate);
        }
    }

    protected void StopWheeling()
    {
        if (WheelingStopWatch.IsRunning)
        {
            WheelingStopWatch.Stop();
            WheelingStopWatch.Reset();
            Invalidate();
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (CallAddonPointerWheeled(e) == DoNext)
        {
            if (!WheelingStopWatch.IsRunning)
            {
                PreviousUnitLength = UnitLength;
                PreviousUnitLength = UnitLength;
                PreviousZero = _Zero;
                WheelingStopWatch.Restart();
                PreviousBuffer = TotalBuffer.Copy();
            }

            var (x, y) = e.GetPosition(this);
            var bzero = _Zero;
            var times_x = (_Zero.X - x) / UnitLength;
            var times_y = (_Zero.Y - y) / UnitLength;
            var delta = Pow(1.04, e.Delta.Y);
            UnitLength *= delta;
            UnitLength *= delta;
            UnitLength = RangeTo(0.01, 1000000, UnitLength);
            UnitLength = RangeTo(0.01, 1000000, UnitLength);
            var ratioX = UnitLength / PreviousUnitLength;
            var ratioY = UnitLength / PreviousUnitLength;
            _Zero = new PointL
            {
                X = (long)((long)x - ((long)x - PreviousZero.X) * ratioX),
                Y = (long)((long)y - ((long)y - PreviousZero.Y) * ratioY)
            };
            if (ratioX > 2 || ratioX < 0.5 || ratioY > 2 || ratioY < 0.5)
            {
                WheelingStopWatch.Stop();
                WheelingStopWatch.Reset();
                PreviousUnitLength = UnitLength;
                PreviousUnitLength = UnitLength;
                PreviousZero = _Zero;
                Invalidate();
                return;
            }

            lock (TotalBuffer)
            {
                using (var dc = new SKCanvas(TotalBuffer))
                {
                    dc.Clear(AxisBackground);
                    dc.DrawBitmap(PreviousBuffer, CreateSKRectWH(
                        (float)(_Zero.X - PreviousZero.X * ratioX),
                        (float)(_Zero.Y - PreviousZero.Y * ratioY),
                        (float)(ratioX * PreviousBuffer.Width),
                        (float)(ratioY * PreviousBuffer.Height)), AntiAlias);
                }
            }

            InvalidateVisual();
        }
    }
}