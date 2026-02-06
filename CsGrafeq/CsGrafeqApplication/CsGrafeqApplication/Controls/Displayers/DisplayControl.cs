using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using CsGrafeq.Debug;
using CsGrafeq.Utilities;
using SkiaSharp;
using static CsGrafeq.Utilities.ThrowHelper;

namespace CsGrafeqApplication.Controls.Displayers;

public delegate void RenderHandler(SKCanvas dc, SKRect bounds);

public class DisplayControl : CartesianDisplayer
{
    /// <summary>
    ///     上一时刻的零点位置
    /// </summary>
    private PointL LastZeroPos;

    /// <summary>
    ///     鼠标按下时的位置
    /// </summary>
    private PointL MouseDownPos = new() { X = 0, Y = 0 };

    /// <summary>
    ///     鼠标按下时的零点位置
    /// </summary>
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
        StopWheeling();
        if (CallPointerPressed(e) == DoNext)
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
        if (!e.Pointer.IsPrimary) return;
        StopWheeling();
        if (CallPointerMoved(e) == DoNext)
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
                        using var dc = new SKCanvas(TotalBuffer);
                        dc.Clear(AxisBackground);
                        RenderAxes(dc);
                        if (!Setting.Instance.MoveOptimization || (LastZeroPos - Zero).Length > 50)
                        {
                            foreach (var rt in Addons.SelectMany(adn=>adn.Layers))
                            {
                                
                                if (!rt.IsActive)
                                    return;
                                var size = rt.RenderTargetSize;
                                if (size.Width != TotalBuffer.Width || size.Height != TotalBuffer.Height)
                                {
                                    Throw(
                                        $"Bitmap size mismatch TotalBufferSize:{TotalBuffer.Width},{TotalBuffer.Height} RTSize:{size.Width},{size.Height}");
                                    return;
                                }

                                rt.CopyRenderTargetTo(TempBuffer);
                                using (var canvas = rt.GetBitmapCanvas()!)
                                {
                                    canvas.Clear(SKColors.Transparent);
                                    canvas.DrawBitmap(TempBuffer, Zero.X - LastZeroPos.X,
                                        Zero.Y - LastZeroPos.Y);
                                    canvas.Flush();
                                    RenderExtension.RenderMovedPlace(rt.Render, Bounds.Size,
                                        new Point(Zero.X, Zero.Y),
                                        new Point(LastZeroPos.X, LastZeroPos.Y));
                                }
                                rt.DrawRenderTargetTo(dc, 0, 0);
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
                                layer.DrawRenderTargetTo(dc, (int)(Zero.X - LastZeroPos.X),
                                    (int)(Zero.Y - LastZeroPos.Y));
                                //RenderMovedPlace(dc, layer.Render);
                            }
                        }

                        RenderAxesNumber(dc);
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
        if (CallPointerReleased(e) == DoNext)
        {
            if (LastZeroPos != Zero) ForceToRender(CancellationToken.None);
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
        CallPointerTapped(e);
    }

    protected virtual void OnPointerDoubleTapped(TappedEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        CallPointerDoubleTapped(e);
    }

    private void RenderMovedPlace(SKCanvas dc, RenderHandler rm)
    {
        RenderExtension.RenderMovedPlace(dc, rm, Bounds.Size, new Point(Zero.X, Zero.Y),
            new Point(LastZeroPos.X, LastZeroPos.Y));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (CallKeyDown(e) == DoNext)
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
        if (CallKeyUp(e) == DoNext)
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