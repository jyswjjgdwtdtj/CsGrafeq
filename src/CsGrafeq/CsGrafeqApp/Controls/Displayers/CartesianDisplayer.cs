using Avalonia.Input;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static CsGrafeqApp.Controls.SkiaEx;
using AvaPoint = Avalonia.Point;
using AvaSize = Avalonia.Size;

namespace CsGrafeqApp.Controls.Displayers
{
    public class CartesianDisplayer : Displayer
    {
        private double _UnitLength = 20.0001d;
        public bool DrawAxisGrid = true;
        public bool DrawAxisNumber = true;
        public bool DrawAxisLine = true;
        protected bool MouseOnYAxis, MouseOnXAxis;
        public CartesianDisplayer()
        {
            WheelingTimer = new Timer(TimerElapsed, null, 0, 500);
        }
        public double UnitLength
        {
            get => _UnitLength;
            set { _UnitLength = value; }
        }
        protected PointL _Zero = new PointL() { X = 500, Y = 250 };
        public override double MathToPixelX(double d)=> _Zero.X + d * _UnitLength;
        public override double MathToPixelY(double d)=> _Zero.Y + -d * _UnitLength;
        public override double PixelToMathX(double d)=> (d - _Zero.X) / _UnitLength;
        public override double PixelToMathY(double d)=> -(d - _Zero.Y) / _UnitLength;
        
        //不敢动………………
        protected void RenderAxisLine(SKCanvas dc)
        {
            double width = Bounds.Width;
            double height = Bounds.Height;
            //y
            if (RangeIn(ValidRect.Left, ValidRect.Right, _Zero.X))
            {
                if (DrawAxisGrid && !DrawAxisLine)
                    dc.DrawLine(new SKPoint(_Zero.X, (float)ValidRect.Top), new SKPoint(_Zero.X, (float)ValidRect.Bottom), FilledGray1);
                else if (DrawAxisLine)
                    dc.DrawLine(new SKPoint(_Zero.X, (float)ValidRect.Top), new SKPoint(_Zero.X, (float)ValidRect.Bottom), MouseOnYAxis ? FilledBlue : FilledBlack);
            }
            if (RangeIn(0, height, _Zero.Y))
            {
                if (DrawAxisGrid && !DrawAxisLine)
                    dc.DrawLine(new SKPoint((float)ValidRect.Left, _Zero.Y), new SKPoint((float)ValidRect.Right, _Zero.Y), FilledGray1);
                else if (DrawAxisLine)
                    dc.DrawLine(new SKPoint((float)ValidRect.Left, _Zero.Y), new SKPoint((float)ValidRect.Right, _Zero.Y), MouseOnYAxis ? FilledBlue : FilledBlack);
            }
            if (!DrawAxisGrid)
                return;
            int zsX = (int)Floor(Log(350 / _UnitLength, 10));
            int zsY = (int)Floor(Log(350 / _UnitLength, 10));
            double addnumX = Pow(10D, zsX);
            double addnumY = Pow(10D, zsY);
            decimal addnumDX = Pow(10M, zsX);
            decimal addnumDY = Pow(10M, zsY);
            double p = RangeTo(-3, height - TextFont.Size + 1, _Zero.Y);
            SKPaint targetPen;
            for (double i = Min(_Zero.X - addnumX * _UnitLength, MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX))); i > ValidRect.Left; i -= addnumX * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathX(i), -zsX);
                if (num % (10 * addnumDX) == 0) { targetPen = FilledGray2; } else { targetPen = FilledGray1; }
                dc.DrawLine(
                    (float)i, 0,
                    (float)i, (float)height,
                    targetPen
                );
            }
            for (double i = Max(_Zero.X + addnumX * _UnitLength, MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX))); i < ValidRect.Right; i += addnumX * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathX(i), -zsX);
                if (num % (10 * addnumDX) == 0) { targetPen = FilledGray2; } else { targetPen = FilledGray1; }
                dc.DrawLine(
                    (float)i, 0,
                    (float)i, (float)height,
                    targetPen
                );
            }
            for (double i = Min(_Zero.Y - addnumY * _UnitLength, MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY))); i > ValidRect.Top; i -= addnumY * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathY(i), -zsY);
                if (num % (10 * addnumDY) == 0) { targetPen = FilledGray2; } else { targetPen = FilledGray1; }
                dc.DrawLine(
                    0, (float)i,
                    (float)width, (float)i,
                    targetPen
                );
            }
            for (double i =Max(_Zero.Y + addnumY * _UnitLength, MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY))); i < ValidRect.Bottom; i += addnumY * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathY(i), -zsY);
                if (num % (10 * addnumDY) == 0) { targetPen = FilledGray2; } else { targetPen = FilledGray1; }
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
            double width = Bounds.Width;
            double height = Bounds.Height;
            int zsX = (int)Floor(Log(350 / _UnitLength, 10));
            int zsY = (int)Floor(Log(350 / _UnitLength, 10));
            double addnumX = Pow(10D, zsX);
            double addnumY = Pow(10D, zsY);
            decimal addnumDX = Pow(10M, zsX);
            decimal addnumDY = Pow(10M, zsY);
            double p = RangeTo(-3, height - TextFont.Size + 1, _Zero.Y);
            float fff = 1f / 4f * TextFont.Size;
            for (double i = Min(_Zero.X - addnumX * _UnitLength, MathToPixelX(RoundTen(PixelToMathX(ValidRect.Right), -zsX))); i > ValidRect.Left; i -= addnumX * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathX(i), -zsX);
                dc.DrawText(num.ToString(), new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + TextFont.Size), SKTextAlign.Left, TextFont, FilledBlack);
            }
            for (double i = Max(_Zero.X + addnumX * _UnitLength, MathToPixelX(RoundTen(PixelToMathX(ValidRect.Left), -zsX))); i < ValidRect.Right; i += addnumX * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathX(i), -zsX);
                dc.DrawText(num.ToString(), new SKPoint((float)(i - num.ToString().Length * fff - 2), (float)p + TextFont.Size), SKTextAlign.Left, TextFont, FilledBlack);
            }
            for (double i = Min(_Zero.Y - addnumY * _UnitLength, MathToPixelY(RoundTen(PixelToMathY(ValidRect.Bottom), -zsY))); i > ValidRect.Top; i -= addnumY * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathY(i), -zsY);
                if (ValidRect.Left+ 3 > _Zero.X)
                {
                    dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)), SKTextAlign.Left, TextFont, FilledBlack);
                }
                else if (_Zero.X + num.ToString().Length * TextFont.Size / 2 > ValidRect.Right - 3)
                {
                    dc.DrawText(num.ToString(), new SKPoint((float)width - num.ToString().Length * TextFont.Size / 2 - 5, (float)(i + 4)), SKTextAlign.Left, TextFont, FilledBlack);
                }
                else
                {
                    dc.DrawText(num.ToString(), new SKPoint(_Zero.X, (float)(i + 4)), SKTextAlign.Left, TextFont, FilledBlack);
                }
            }
            for (double i =Max(_Zero.Y + addnumY * _UnitLength, MathToPixelY(RoundTen(PixelToMathY(ValidRect.Top), -zsY))); i < ValidRect.Bottom; i += addnumY * _UnitLength)
            {
                decimal num = RoundTen((decimal)PixelToMathY(i), -zsY);
                if (ValidRect.Left + 3 > _Zero.X)
                {
                    dc.DrawText(num.ToString(), new SKPoint((float)ValidRect.Left + 3, (float)(i + 4)), SKTextAlign.Left, TextFont, FilledBlack);
                }
                else if (_Zero.X + num.ToString().Length * TextFont.Size / 2 > ValidRect.Right - 3)
                {
                    dc.DrawText(num.ToString(), new SKPoint((float)width - num.ToString().Length * TextFont.Size / 2 - 5, (float)(i + 4)), SKTextAlign.Left, TextFont, FilledBlack);
                }
                else
                {
                    dc.DrawText(num.ToString(), new SKPoint(_Zero.X, (float)(i + 4)), SKTextAlign.Left, TextFont, FilledBlack);
                }
            }
            dc.DrawText("0", new SKPoint(_Zero.X + 3, _Zero.Y + TextFont.Size), SKTextAlign.Left, TextFont, FilledBlack);
        }
        public override void CompoundBuffer()
        {
            lock (TotalBuffer)
            {
                using (var dc = new SKCanvas(TotalBuffer))
                {
                    dc.Clear(SKColors.White);
                    RenderAxisLine(dc);
                    foreach (var i in Addons)
                    {
                        dc.DrawBitmap(i.Bitmap, new SKPoint(0,0));
                    }
                    RenderAxisNumber(dc);
                }
            }
        }
        private readonly Stopwatch WheelingStopWatch = new Stopwatch();
        private readonly Timer WheelingTimer;
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
        private double PreviousUnitLength;
        private PointL PreviousZero;
        private SKBitmap PreviousBuffer=new SKBitmap();
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (CallAddonPointerWheeled(e) == DoNext)
            {
                if (!WheelingStopWatch.IsRunning)
                {
                    PreviousUnitLength = _UnitLength;
                    PreviousUnitLength = _UnitLength;
                    PreviousZero = _Zero;
                    WheelingStopWatch.Restart(); 
                    PreviousBuffer = TotalBuffer.Copy();
                }
                var (x, y) = e.GetPosition(this);
                var bzero = _Zero;
                double times_x = (_Zero.X - x) / _UnitLength;
                double times_y = (_Zero.Y - y) / _UnitLength;
                double delta = Pow(1.04, e.Delta.Y);
                _UnitLength *= delta;
                _UnitLength *= delta;
                _UnitLength = RangeTo(0.01, 1000000, _UnitLength);
                _UnitLength = RangeTo(0.01, 1000000, _UnitLength);
                double ratioX = _UnitLength / PreviousUnitLength;
                double ratioY = _UnitLength / PreviousUnitLength;
                _Zero = new PointL()
                {
                    X = (long)((long)x - ((long)x - PreviousZero.X) * ratioX),
                    Y = (long)((long)y - ((long)y - PreviousZero.Y) * ratioY),
                };
                if (ratioX > 2 || ratioX < 0.5|| ratioY > 2 || ratioY < 0.5)
                {
                    WheelingStopWatch.Stop();
                    WheelingStopWatch.Reset();
                    PreviousUnitLength = _UnitLength;
                    PreviousUnitLength = _UnitLength;
                    PreviousZero = _Zero;
                    Invalidate();
                    return;
                }
                lock (TotalBuffer)
                {
                    using (var dc = new SKCanvas(TotalBuffer))
                    {
                        dc.Clear(SKColors.White);
                        dc.DrawBitmap(PreviousBuffer, CreateSKRectWH(
                            (float)(_Zero.X - PreviousZero.X * ratioX),
                            (float)(_Zero.Y - PreviousZero.Y * ratioY),
                            (float)(ratioX * PreviousBuffer.Width),
                            (float)(ratioY * PreviousBuffer.Height)),AntiAlias);
                    }
                }
                InvalidateVisual();
            }
        }
    }
}
