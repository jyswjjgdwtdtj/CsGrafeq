using Avalonia.Input;
using SkiaSharp;
using static CsGrafeqApplication.Controls.SkiaEx;
using static CsGrafeq.Extension;

namespace CsGrafeqApplication.Controls.Displayers;

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
        if (!e.Pointer.IsPrimary) return;
        Focus();
        StopWheeling();
        if (CallAddonPointerPressed(e) == DoNext)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                var p = e.GetPosition(this);
                MouseDownPos = new PointL { X = (long)p.X, Y = (long)p.Y };
                MouseDownZeroPos = Zero;
            }

            LastZeroPos = Zero;
        }
        else
        {
            CompoundBuffer();
        }

        Focus();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        StopWheeling();
        if (CallAddonPointerMoved(e) == DoNext)
        {
            var current = e.GetPosition(this);
            bool l = MouseOnYAxis, ll = MouseOnXAxis;
            MouseOnYAxis = Abs(current.X - Zero.X) < 3;
            MouseOnXAxis = Abs(current.Y - Zero.Y) < 3;
            if (e.Properties.IsLeftButtonPressed)
            {
                //移动零点
                var newZero = new PointL
                {
                    X = MouseDownZeroPos.X + (long)current.X - MouseDownPos.X,
                    Y = MouseDownZeroPos.Y + (long)current.Y - MouseDownPos.Y
                };
                if (newZero != Zero)
                {
                    Zero = newZero;
                    lock (TotalBuffer)
                    {
                        using (var dc = new SKCanvas(TotalBuffer))
                        {
                            dc.Clear(AxisBackground);
                            RenderAxisLine(dc);
                            if (!MovingOptimization || (LastZeroPos - Zero).Length > 30)
                            {
                                foreach (var i in Addons)
                                {
                                    if (i.Bitmap.Width != TotalBuffer.Width || i.Bitmap.Height != TotalBuffer.Height)
                                    {
                                        Throw("Bitmap size mismatch");
                                        return;
                                    }

                                    var newbmp = new SKBitmap(TotalBuffer.Width, TotalBuffer.Height);
                                    using (var newbmpcanvas = new SKCanvas(newbmp))
                                    {
                                        newbmpcanvas.Clear(SKColors.Transparent);
                                        newbmpcanvas.DrawBitmap(i.Bitmap, Zero.X - LastZeroPos.X,
                                            Zero.Y - LastZeroPos.Y);
                                        RenderMovedPlace(newbmpcanvas, i.AddonRender);
                                    }

                                    i.Bitmap.Dispose();
                                    i.Bitmap = newbmp;
                                    dc.DrawBitmap(i.Bitmap, 0, 0);
                                }

                                LastZeroPos = Zero;
                            }
                            else
                            {
                                foreach (var i in Addons)
                                {
                                    dc.DrawBitmap(i.Bitmap, Zero.X - LastZeroPos.X, Zero.Y - LastZeroPos.Y);
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
        if (!e.Pointer.IsPrimary) return;
        Focus();
        StopWheeling();
        if (CallAddonPointerReleased(e) == DoNext)
        {
            if (LastZeroPos != Zero) Invalidate();
            LastZeroPos = Zero;
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
        if (!e.Pointer.IsPrimary) return;
        CallAddonPointerTapped(e);
    }

    protected virtual void OnPointerDoubleTapped(TappedEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        CallAddonPointerDoubleTapped(e);
    }

    private void RenderMovedPlace(SKCanvas dc, RenderHandler rm)
    {
        var width = (float)Bounds.Width;
        var height = (float)Bounds.Height;
        if (Zero.X < LastZeroPos.X)
        {
            rm.Invoke(dc, CreateSKRectWH(width - LastZeroPos.X + Zero.X, 0, width, height));
            if (Zero.Y < LastZeroPos.Y)
                rm.Invoke(dc,
                    CreateSKRectWH(0, height - LastZeroPos.Y + Zero.Y, width - LastZeroPos.X + Zero.X, height));
            else if (Zero.Y > LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH(0, 0, width - LastZeroPos.X + Zero.X, Zero.Y - LastZeroPos.Y));
        }
        else if (Zero.X > LastZeroPos.X)
        {
            rm.Invoke(dc, CreateSKRectWH(0, 0, (int)(Zero.X - LastZeroPos.X), height));
            if (Zero.Y < LastZeroPos.Y)
                rm.Invoke(dc,
                    CreateSKRectWH((int)(Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + Zero.Y), width,
                        height));
            else if (Zero.Y > LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH((int)(Zero.X - LastZeroPos.X), 0, width, (int)(Zero.Y - LastZeroPos.Y)));
        }
        else
        {
            if (Zero.Y < LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH(0, (int)(height - LastZeroPos.Y + Zero.Y), width, height));
            else if (Zero.Y > LastZeroPos.Y)
                rm.Invoke(dc, CreateSKRectWH(0, 0, width, (int)(Zero.Y - LastZeroPos.Y)));
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CallAddonKeyDown(e) == DoNext)
        {
        }

        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (CallAddonKeyUp(e) == DoNext)
        {
        }

        e.Handled = true;
    }
}