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
using CsGrafeqApplication.Utilities;
using SkiaSharp;
using static CsGrafeq.Utilities.ThrowHelper;
using BigPoint= CsGrafeq.PointBase<CsGrafeq.Numeric.BigNumber<long,double>>;

namespace CsGrafeqApplication.Controls.Displayers;

public delegate void RenderHandler(SKCanvas dc, SKRect bounds);

public class DisplayControl : CartesianDisplayer
{
    /// <summary>
    ///     上一时刻的零点位置
    /// </summary>
    private BigPoint PreviousPointerMovedZero;

    /// <summary>
    ///     鼠标按下时的位置
    /// </summary>
    private BigPoint PointerDownPointerPos = new() { X = 0, Y = 0 };

    /// <summary>
    ///     鼠标按下时的零点位置
    /// </summary>
    private BigPoint PointerDownZeroPos = new() { X = 0, Y = 0 };

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
                PointerDownPointerPos = p.ToBigPoint();
                PointerDownZeroPos = ZeroPos;
            }
            PreviousPointerMovedZero = ZeroPos;
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
            var current = e.Position.ToBigPoint();
            MouseOnYAxis = Abs(current.X - ZeroPos.X) < 3;
            MouseOnXAxis = Abs(current.Y - ZeroPos.Y) < 3;
            if (e.Properties.IsLeftButtonPressed)
            {
                //移动零点
                var newZero = new BigPoint
                {
                    X = PointerDownZeroPos.X + current.X - PointerDownPointerPos.X,
                    Y = PointerDownZeroPos.Y + current.Y - PointerDownPointerPos.Y
                };
                if (newZero != ZeroPos)
                {
                    ZeroPos = newZero;
                    lock (TotalBufferLock)
                    {
                        var dc = new SKCanvas(TotalBuffer);
                        dc.Clear(AxisBackground);
                        RenderAxes(dc);
                        if (!Setting.Instance.MoveOptimization || (PreviousPointerMovedZero - ZeroPos).Length > 50)
                        {
                            var lastZero = PreviousPointerMovedZero;
                            PreviousPointerMovedZero = ZeroPos;
                            foreach (var rt in Addons.SelectMany(adn => adn.Layers).Where(rt => rt.IsActive))
                            {
                                rt.CopyRenderTargetTo(TempBuffer);
                                using var canvas = rt.GetBitmapCanvas()!;
                                canvas.Clear(SKColors.Transparent);
                                canvas.DrawBitmap(TempBuffer, (int)((ZeroPos.X - lastZero.X).ToDecimal()),
                                    (int)((ZeroPos.Y - lastZero.Y).ToDecimal()), SkiaHelper.CompoundBufferPaint);
                                canvas.Flush();
                                RenderExtension.RenderMovedPlace(rt.Render, Bounds.Size,
                                    new Point(ZeroPos.X, ZeroPos.Y),
                                    new Point(lastZero.X, lastZero.Y), CancellationHelperToken);
                                rt.DrawRenderTargetTo(dc, 0, 0);
                            }
                        }
                        else
                        {
                            foreach (var layer in Addons.SelectMany(adn => adn.Layers).Where(rt=>rt.IsActive))
                            {
                                layer.DrawRenderTargetTo(dc, (int)(ZeroPos.X - PreviousPointerMovedZero.X),
                                    (int)(ZeroPos.Y - PreviousPointerMovedZero.Y));
                            }
                        }
                        RenderAxesNumber(dc);
                        dc.Dispose();
                    }

                    InvalidateVisual();
                }
            }
        }
        else
        {
            AskForRender();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs eventArgs)
    {
        var e = eventArgs.Cast(this);
        StopWheeling();
        if (CallPointerReleased(e) == DoNext)
        {
            var current = e.Position.ToBigPoint();
            MouseOnYAxis = Abs(current.X - ZeroPos.X) < 3;
            MouseOnXAxis = Abs(current.Y - ZeroPos.Y) < 3;
            if (e.Properties.IsLeftButtonPressed)
            {
                //移动零点
                var newZero = new BigPoint
                {
                    X = PointerDownZeroPos.X + current.X - PointerDownPointerPos.X,
                    Y = PointerDownZeroPos.Y + current.Y - PointerDownPointerPos.Y
                };
                if (newZero != ZeroPos)
                {
                    if (Setting.Instance.MoveOptimization)
                    {
                        lock (TotalBufferLock)
                        {
                            var dc = new SKCanvas(TotalBuffer);
                            dc.Clear(AxisBackground);
                            RenderAxes(dc);
                            var lastZero = PreviousPointerMovedZero;
                            PreviousPointerMovedZero = ZeroPos;
                            foreach (var rt in Addons.SelectMany(adn => adn.Layers).Where(rt => rt.IsActive))
                            {
                                rt.CopyRenderTargetTo(TempBuffer);
                                using var canvas = rt.GetBitmapCanvas()!;
                                canvas.Clear(SKColors.Transparent);
                                canvas.DrawBitmap(TempBuffer, (int)((ZeroPos.X - lastZero.X).ToDecimal()),
                                    (int)((ZeroPos.Y - lastZero.Y).ToDecimal()), SkiaHelper.CompoundBufferPaint);
                                canvas.Flush();
                                RenderExtension.RenderMovedPlace(rt.Render, Bounds.Size,
                                    new Point(ZeroPos.X, ZeroPos.Y),
                                    new Point(lastZero.X, lastZero.Y), CancellationHelperToken);
                                rt.DrawRenderTargetTo(dc, 0, 0);
                            }
                            RenderAxesNumber(dc);
                            dc.Dispose();
                        }
                        InvalidateVisual();
                    }
                    else
                    {
                        ForceToRender(CancellationToken.None);
                    }
                }
            }
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