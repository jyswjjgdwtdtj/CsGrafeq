using System;
using Avalonia.Input;
using SkiaSharp;
using static CsGrafeqApp.Controls.SkiaEx;

namespace CsGrafeqApp.Controls.Displayers;

public delegate void RenderHandler(SKCanvas dc, SKRect bounds);

public class DisplayControl : CartesianDisplayer
{
    private PointL LastZeroPos;
    private PointL MouseDownPos = new() { X = 0, Y = 0 };
    private PointL MouseDownZeroPos = new() { X = 0, Y = 0 };

    public DisplayControl()
    {
        Focusable = true;   
        AddHandler(TappedEvent, (_, e) => OnPointerTapped(e));
        AddHandler(DoubleTappedEvent, (_, e) => OnPointerDoubleTapped(e));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Focus();
        StopWheeling();
        if (CallAddonPointerPressed(e) == DoNext)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                var p = e.GetPosition(this);
                MouseDownPos = new PointL { X = (long)p.X, Y = (long)p.Y };
                MouseDownZeroPos = _Zero;
            }

            LastZeroPos = _Zero;
        }
        else
        {
            CompoundBuffer();
        }
        Focus();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        Focus();
        StopWheeling();
        if (CallAddonPointerMoved(e) == DoNext)
        {
            var current = e.GetPosition(this);
            bool l = MouseOnYAxis, ll = MouseOnXAxis;
            MouseOnYAxis = Abs(current.X - _Zero.X) < 3;
            MouseOnXAxis = Abs(current.Y - _Zero.Y) < 3;
            if (e.Properties.IsLeftButtonPressed)
            {
                //移动零点
                var newZero = new PointL
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
                            dc.Clear(AxisBackground);
                            RenderAxisLine(dc);
                            if ((LastZeroPos - _Zero).Length > 30)
                            {
                                var newbmp = new SKBitmap(TotalBuffer.Width, TotalBuffer.Height);
                                foreach (var i in Addons)
                                {
                                    i.Bitmap.CopyTo(newbmp);
                                    using (var currentCanvas = new SKCanvas(i.Bitmap))
                                    {
                                        currentCanvas.Clear(SKColors.Transparent);
                                        currentCanvas.DrawBitmap(newbmp, _Zero.X - LastZeroPos.X,
                                            _Zero.Y - LastZeroPos.Y);
                                        RenderMovedPlace(currentCanvas, i.AddonRender);
                                    }

                                    dc.DrawBitmap(i.Bitmap, 0, 0);
                                }

                                newbmp.Dispose();
                                LastZeroPos = _Zero;
                            }
                            else
                            {
                                foreach (var i in Addons)
                                {
                                    dc.DrawBitmap(i.Bitmap, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                                    RenderMovedPlace(dc, i.AddonRender);
                                }
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
        Focus();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Focus();
        StopWheeling();
        if (CallAddonPointerReleased(e) == DoNext)
        {
            if (LastZeroPos != _Zero) Invalidate();
            LastZeroPos = _Zero;
        }
        else
        {
            CompoundBuffer();
            InvalidateVisual();
        }
        Focus();
    }

    protected virtual void OnPointerTapped(TappedEventArgs e)
    {
        CallAddonPointerTapped(e);
    }

    protected virtual void OnPointerDoubleTapped(TappedEventArgs e)
    {
        CallAddonPointerDoubleTapped(e);
    }

    private void RenderMovedPlace(SKCanvas dc, RenderHandler rm)
    {
        var width = (float)Bounds.Width;
        var height = (float)Bounds.Height;
        if (_Zero.X < LastZeroPos.X)
        {
            rm.Invoke(dc, CreateSKRectWH(width - LastZeroPos.X + _Zero.X, 0, width, height));
            if (_Zero.Y < LastZeroPos.Y)
                rm.Invoke(dc,
                    CreateSKRectWH(0, height - LastZeroPos.Y + _Zero.Y, width - LastZeroPos.X + _Zero.X, height));
            else if (_Zero.Y > LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH(0, 0, width - LastZeroPos.X + _Zero.X, _Zero.Y - LastZeroPos.Y));
        }
        else if (_Zero.X > LastZeroPos.X)
        {
            rm.Invoke(dc, CreateSKRectWH(0, 0, (int)(_Zero.X - LastZeroPos.X), height));
            if (_Zero.Y < LastZeroPos.Y)
                rm.Invoke(dc,
                    CreateSKRectWH((int)(_Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + _Zero.Y), width,
                        height));
            else if (_Zero.Y > LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH((int)(_Zero.X - LastZeroPos.X), 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
        }
        else
        {
            if (_Zero.Y < LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH(0, (int)(height - LastZeroPos.Y + _Zero.Y), width, height));
            else if (_Zero.Y > LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH(0, 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CallAddonKeyDown(e) == DoNext)
        {
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (CallAddonKeyUp(e) == DoNext)
        {
        }
    }
}