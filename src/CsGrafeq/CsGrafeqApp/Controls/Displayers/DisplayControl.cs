using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeqApp.Controls.SkiaEx;
using static CsGrafeqApp.ExtensionMethods;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;

namespace CsGrafeqApp.Controls.Displayers
{
    public delegate void RenderHandler(SKCanvas dc,SKRect bounds);
    public class DisplayControl:CartesianDisplayer
    {
        private PointL MouseDownPos = new PointL() { X = 0, Y = 0 };
        private PointL MouseDownZeroPos = new PointL() { X = 0, Y = 0 };
        private PointL LastZeroPos;
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            StopWheeling();
            if (CallAddonPointerPressed(e) == DoNext)
            {
                if (e.Properties.IsLeftButtonPressed)
                {
                    AvaPoint p = e.GetPosition(this);
                    MouseDownPos = new PointL() { X = (long)p.X, Y = (long)p.Y };
                    MouseDownZeroPos = _Zero;
                }
                LastZeroPos = _Zero;
            }
            else
            {
                CompoundBuffer();
            }
        }
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            StopWheeling();
            if (CallAddonPointerMoved(e) == DoNext)
            {
                AvaPoint current = e.GetPosition(this);
                bool l = MouseOnYAxis, ll = MouseOnXAxis;
                MouseOnYAxis = Math.Abs(current.X - _Zero.X) < 3;
                MouseOnXAxis = Math.Abs(current.Y - _Zero.Y) < 3;
                if (e.Properties.IsLeftButtonPressed)
                {//移动零点
                    var newZero = new PointL()
                    {
                        X = MouseDownZeroPos.X + (long)current.X - MouseDownPos.X,
                        Y = MouseDownZeroPos.Y + (long)current.Y - MouseDownPos.Y
                    };
                    if (newZero != _Zero)
                    {
                        _Zero = newZero;
                        lock (TotalBuffer)
                        {
                            using (var dc = new SKCanvas(TotalBuffer))
                            {
                                dc.Clear(SKColors.White);
                                RenderAxisLine(dc);
                                foreach (var i in Addons)
                                {
                                    dc.DrawBitmap(i.Bitmap, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                                    RenderMovedPlace(dc, i.AddonRender);
                                }
                                RenderAxisNumber(dc);
                            }
                        }
                        InvalidateVisual();
                    }
                }
            }
            else
            {
                CompoundBuffer();
                InvalidateVisual();
            }
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            StopWheeling();
            if (CallAddonPointerReleased(e) == DoNext)
            {
                if (LastZeroPos != _Zero)
                {
                    Invalidate();
                }
                LastZeroPos= _Zero;
            }
            else
            {
                CompoundBuffer();
                InvalidateVisual();
            }
        }
        private void RenderMovedPlace(SKCanvas dc, RenderHandler rm)
        {
            float width = (float)Bounds.Width;
            float height = (float)Bounds.Height;
            if (_Zero.X < LastZeroPos.X)
            {
                rm.Invoke(dc, CreateSKRectWH(width - LastZeroPos.X + _Zero.X, 0, width, height));
                if (_Zero.Y < LastZeroPos.Y)
                {
                    rm.Invoke(dc, CreateSKRectWH(0,height - LastZeroPos.Y + _Zero.Y, width - LastZeroPos.X + _Zero.X, height));

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    rm.Invoke(dc,CreateSKRectWH(0, 0,width - LastZeroPos.X + _Zero.X, _Zero.Y - LastZeroPos.Y));
                }
            }
            else if (_Zero.X > LastZeroPos.X)
            {
                rm.Invoke(dc,CreateSKRectWH(0, 0, (int)(_Zero.X - LastZeroPos.X), height));
                if (_Zero.Y < LastZeroPos.Y)
                {
                    rm.Invoke(dc,CreateSKRectWH((int)(_Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + _Zero.Y), width, height));

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    rm.Invoke(dc,CreateSKRectWH((int)(_Zero.X - LastZeroPos.X), 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
                }
            }
            else
            {
                if (_Zero.Y < LastZeroPos.Y)
                {
                    rm.Invoke(dc,CreateSKRectWH(0, (int)(height - LastZeroPos.Y + _Zero.Y), width, height));

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    rm.Invoke(dc,CreateSKRectWH(0, 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
                }
            }
        }
    }
}
