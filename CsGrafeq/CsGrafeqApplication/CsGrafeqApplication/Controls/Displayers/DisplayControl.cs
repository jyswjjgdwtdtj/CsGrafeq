using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Input;
using CsGrafeq.Setting;
using CsGrafeqApplication.Events;
using CsGrafeqApplication.Utilities;
using SkiaSharp;
using BigPoint = CsGrafeq.PointBase<CsGrafeq.Numeric.BigNumber<long, double>>;

namespace CsGrafeqApplication.Controls.Displayers;

public sealed class DisplayControl : CartesianDisplayer
{
    /// <summary>
    ///     鼠标按下时的位置 用以计算出新的Zero
    /// </summary>
    private BigPoint _pointerDownPointerPos = new() { X = 0, Y = 0 };

    /// <summary>
    ///     鼠标按下时的零点位置 用以计算出新的Zero
    /// </summary>
    private BigPoint _pointerDownZeroPos = new() { X = 0, Y = 0 };

    /// <summary>
    ///     上一次重绘补全完整绘制区域时的Zero
    /// </summary>
    private BigPoint _previousRenderZeroPos;

    public DisplayControl()
    {
        Focusable = true;
        AddHandler(TappedEvent, (_, e) => OnPointerTapped(e));
        AddHandler(DoubleTappedEvent, (_, e) => OnPointerDoubleTapped(e));
    }

    private bool _isPointerLeftButtonDown;
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        StopWheeling();
        var ev = e.Cast(this);
        if (CallPointerPressed(ev) == DoNext)
        {
            _isPointerLeftButtonDown = true;
            if (e.Properties.IsLeftButtonPressed) OnPointerPressedLeftButton(ev);
            base.OnPointerPressed(e);
        }
        else
        {
            AskForRender();
        }
    }

    private void OnPointerPressedLeftButton(MouseEventArgs e, CancellationToken ct = default)
    {
        Console.WriteLine("Press");
        var p = e.Position;
        _pointerDownPointerPos = p.ToBigPoint();
        _pointerDownZeroPos = ZeroPos;
        lock (TotalBufferLock)
        {
            _previousRenderZeroPos = ZeroPos;
        }
    }


    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        StopWheeling();
        var ev = e.Cast(this);
        if (CallPointerMoved(ev) == DoNext)
        {
            var current = ev.Position.ToBigPoint();
            if (ev.Properties.IsLeftButtonPressed&&_isPointerLeftButtonDown)
            {
                ZeroPos = new BigPoint
                {
                    X = _pointerDownZeroPos.X + current.X - _pointerDownPointerPos.X,
                    Y = _pointerDownZeroPos.Y + current.Y - _pointerDownPointerPos.Y
                };
                OnPointerMovedLeftButton(ev, CancellationToken.None);
            }
        }
        else
        {
            AskForRender();
        }
    }

    private void OnPointerMovedLeftButton(MouseEventArgs e, CancellationToken ct)
    {
        Console.WriteLine("move");
        if (ZeroPos != _previousRenderZeroPos)
        {
            lock (TotalBufferLock)
            {
                var dc = new SKCanvas(TotalBuffer);
                dc.Clear(AxisBackground);
                RenderAxes(dc);
                if (!Setting.Instance.MoveOptimization || (_previousRenderZeroPos - ZeroPos).Length > 50)
                {
                    var lastZero = _previousRenderZeroPos;
                    _previousRenderZeroPos = ZeroPos;
                    foreach (var rt in Addons.SelectMany(adn => adn.Layers).Where(rt => rt.IsActive))
                    {
                        rt.CopyRenderTargetTo(TempBuffer);
                        using var canvas = rt.GetBitmapCanvas()!;
                        canvas.Clear(SKColors.Transparent);
                        canvas.DrawBitmap(TempBuffer, (int)(ZeroPos.X - lastZero.X).ToDecimal(),
                            (int)(ZeroPos.Y - lastZero.Y).ToDecimal(), SkiaHelper.CompoundBufferPaint);
                        canvas.Flush();
                        RenderExtension.RenderMovedPlace(rt.Render, Bounds.Size,
                            new Point(ZeroPos.X, ZeroPos.Y),
                            new Point(lastZero.X, lastZero.Y), CancellationHelperToken);
                        rt.DrawRenderTargetTo(dc, 0, 0);
                    }
                }
                else
                {
                    foreach (var layer in Addons.SelectMany(adn => adn.Layers).Where(rt => rt.IsActive))
                        layer.DrawRenderTargetTo(dc, (int)(ZeroPos.X - _previousRenderZeroPos.X),
                            (int)(ZeroPos.Y - _previousRenderZeroPos.Y));
                }

                RenderAxesNumber(dc);
                dc.Dispose();
            }

            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs eventArgs)
    {
        var e = eventArgs.Cast(this);
        StopWheeling();
        if (CallPointerReleased(e) == DoNext)
        {
            var current = e.Position.ToBigPoint();
            if (_isPointerLeftButtonDown)
            {
                _isPointerLeftButtonDown = false;
                //移动零点
                ZeroPos = new BigPoint
                {
                    X = _pointerDownZeroPos.X + current.X - _pointerDownPointerPos.X,
                    Y = _pointerDownZeroPos.Y + current.Y - _pointerDownPointerPos.Y
                };
                OnPointerReleasedLeftButton(e);
            }
        }
        else
        {
            AskForRender();
        }
    }

    private void OnPointerReleasedLeftButton(MouseEventArgs e)
    {
        Console.WriteLine("Release");
        if (ZeroPos != _previousRenderZeroPos)
        {
            if (Setting.Instance.MoveOptimization)
            {
                lock (TotalBufferLock)
                {
                    var dc = new SKCanvas(TotalBuffer);
                    dc.Clear(AxisBackground);
                    RenderAxes(dc);
                    var lastZero = _previousRenderZeroPos;
                    _previousRenderZeroPos = ZeroPos;
                    foreach (var rt in Addons.SelectMany(adn => adn.Layers).Where(rt => rt.IsActive))
                    {
                        rt.CopyRenderTargetTo(TempBuffer);
                        using var canvas = rt.GetBitmapCanvas()!;
                        canvas.Clear(SKColors.Transparent);
                        canvas.DrawBitmap(TempBuffer, (int)(ZeroPos.X - lastZero.X).ToDecimal(),
                            (int)(ZeroPos.Y - lastZero.Y).ToDecimal(), SkiaHelper.CompoundBufferPaint);
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

    private void OnPointerTapped(TappedEventArgs e)
    {
        if (!e.Pointer.IsPrimary) return;
        CallPointerTapped(e.Cast(this));
    }

    private void OnPointerDoubleTapped(TappedEventArgs e)
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