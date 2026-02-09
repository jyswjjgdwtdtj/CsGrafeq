using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using CsGrafeq.Debug;
using CsGrafeq.Utilities;
using CsGrafeqApplication.Events;
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
        if (CallPointerPressed(e.Cast(this)) == DoNext)
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

    protected sealed override void OnPointerMoved(PointerEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        StopWheeling();
        var eventargs = e.Cast(this);
        OnPointerMovedCore(eventargs, CancellationHelperToken);
        
    }

    protected virtual void OnPointerMovedCore(MouseEventArgs e,CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
            return;
        if (CallPointerMoved(e) == DoNext)
        {
            var current = e.Position;
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
                        var dc = new SKCanvas(TotalBuffer);
                        dc.Clear(AxisBackground);
                        RenderAxes(dc);
                        if (!Setting.Instance.MoveOptimization || (LastZeroPos - Zero).Length > 50)
                        {
                            var lastZero = LastZeroPos;
                            LastZeroPos = Zero;
                            var tasks=Addons.SelectMany(adn => adn.Layers).Where(rt=>rt.IsActive).Select((rt) =>
                            {
                                if(ct.IsCancellationRequested)
                                    return Task.CompletedTask;
                                rt.CopyRenderTargetTo(TempBuffer);
                                using var canvas = rt.GetBitmapCanvas()!;
                                canvas.Clear(SKColors.Transparent);
                                canvas.DrawBitmap(TempBuffer, Zero.X - lastZero.X,
                                    Zero.Y - lastZero.Y,SkiaHelper.CompoundBufferPaint);
                                canvas.Flush();
                                return Task.Run(() =>
                                {
                                    RenderExtension.RenderMovedPlace(rt.Render, Bounds.Size,
                                        new Point(Zero.X, Zero.Y),
                                        new Point(lastZero.X, lastZero.Y),CancellationHelperToken);
                                    rt.DrawRenderTargetTo(dc, 0, 0);
                                }, ct);
                            });
                            Task.WaitAll(tasks, ct);
                        }
                        else
                        {
                            foreach (var layer in Addons.SelectMany(adn => adn.Layers).Where(rt=>rt.IsActive))
                            {
                                layer.DrawRenderTargetTo(dc, (int)(Zero.X - LastZeroPos.X),
                                    (int)(Zero.Y - LastZeroPos.Y));
                            }
                        }
                        RenderAxesNumber(dc);
                        dc.Dispose();
                    }

                    Dispatcher.UIThread.Invoke(InvalidateVisual);
                }
            }
        }
        else
        {
            AskForRender();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        Focus();
        StopWheeling();
        if (CallPointerReleased(e.Cast(this)) == DoNext)
        {
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
        CallPointerTapped(e.Cast(this));
    }

    protected virtual void OnPointerDoubleTapped(TappedEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        CallPointerDoubleTapped(e.Cast(this));
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