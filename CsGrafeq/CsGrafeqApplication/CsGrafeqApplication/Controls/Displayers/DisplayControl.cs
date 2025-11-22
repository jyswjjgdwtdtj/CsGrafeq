using Avalonia;
using Avalonia.Input;
using CsGrafeq.Debug;
using SkiaSharp;
using static CsGrafeq.Utilities.ThrowHelper;

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
        Debug.LogPointer("PointerPressed");
        if (!e.Pointer.IsPrimary) return;
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
            base.OnPointerPressed(e);
        }
        else
        {
            AskForRender();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        Debug.LogPointer("PointerMoved");
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
                    lock (TotalBufferLock)
                    {
                        using (var dc = new SKCanvas(TotalBuffer))
                        {
                            dc.Clear(AxisBackground);
                            RenderAxisLine(dc);
                            if (!MovingOptimization || (LastZeroPos - Zero).Length > 100)
                            {
                                foreach (var adn in Addons)
                                foreach (var rt in adn.Layers)
                                {
                                    if (!rt.IsActive)
                                        continue;
                                    var size = rt.GetSize();
                                    if (size.Width != TotalBuffer.Width || size.Height != TotalBuffer.Height)
                                    {
                                        Throw("Bitmap size mismatch");
                                        return;
                                    }

                                    var newbmp = rt.GetCopy();
                                    using (var canvas = rt.GetBitmapCanvas())
                                    {
                                        canvas.Clear(SKColors.Transparent);
                                        canvas.DrawBitmap(newbmp, Zero.X - LastZeroPos.X,
                                            Zero.Y - LastZeroPos.Y);
                                        RenderMovedPlace(canvas, rt.Render);
                                    }

                                    newbmp.Dispose();
                                    rt.DrawBitmap(dc, 0, 0);
                                }

                                LastZeroPos = Zero;
                            }
                            else
                            {
                                foreach (var adn in Addons)
                                foreach (var layer in adn.Layers)
                                {
                                    if (!layer.IsActive)
                                        continue;
                                    layer.DrawBitmap(dc, (int)(Zero.X - LastZeroPos.X), (int)(Zero.Y - LastZeroPos.Y));
                                    RenderMovedPlace(dc, layer.Render);
                                }
                            }

                            RenderAxisNumber(dc);
                        }
                    }

                    InvalidateVisual();
                }
            }

            base.OnPointerMoved(e);
        }
        else
        {
            AskForRender();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Debug.LogPointer("PointerReleased");
        if (!e.Pointer.IsPrimary) return;
        Focus();
        StopWheeling();
        if (CallAddonPointerReleased(e) == DoNext)
        {
            if (LastZeroPos != Zero) ForceToRender();
            LastZeroPos = Zero;
            base.OnPointerReleased(e);
        }
        else
        {
            AskForRender();
        }
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
        RenderExtension.RenderMovedPlace(dc, rm, Bounds.Size, new Point(Zero.X, Zero.Y),
            new Point(LastZeroPos.X, LastZeroPos.Y));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CallAddonKeyDown(e) == DoNext)
        {
            base.OnKeyDown(e);
        }
        else
        {
            e.Handled = true;
            AskForRender();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (CallAddonKeyUp(e) == DoNext)
        {
            base.OnKeyUp(e);
        }
        else
        {
            e.Handled = true;
            AskForRender();
        }
    }
}