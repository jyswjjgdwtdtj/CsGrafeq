using Avalonia;
using Avalonia.Input;
using CsGrafeq.Compiler;
using SkiaSharp;
using System;
using static CsGrafeq.Extension;
using static CsGrafeqApplication.Controls.SkiaEx;

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
                            if (false)//&&(!MovingOptimization || (LastZeroPos - Zero).Length > 30))
                            {
                                foreach (var adn in Addons)
                                {
                                    foreach(var rt in adn.Layers)
                                    {
                                        var i = rt.Bitmap;
                                        if (i.Width != TotalBuffer.Width || i.Height != TotalBuffer.Height)
                                        {
                                            Throw("Bitmap size mismatch");
                                            return;
                                        }
                                        var newbmp = i.Copy();
                                        using (var canvas = new SKCanvas(i))
                                        {
                                            canvas.Clear(SKColors.Transparent);
                                            canvas.DrawBitmap(newbmp, Zero.X - LastZeroPos.X,
                                                Zero.Y - LastZeroPos.Y);
                                            RenderMovedPlace(canvas, rt.Render);
                                        }
                                        newbmp.Dispose();
                                        dc.DrawBitmap(i, 0, 0);
                                    }
                                }
                                LastZeroPos = Zero;
                            }
                            else
                            {
                                foreach (var adn in Addons)
                                {
                                    foreach(var layer in adn.Layers)
                                    {
                                        var j = layer.Bitmap;
                                        dc.DrawBitmap(j, Zero.X - LastZeroPos.X, Zero.Y - LastZeroPos.Y);
                                        //RenderMovedPlace(dc,layer.Render);
                                    }
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
        RenderExtension.RenderMovedPlace(dc, rm,Bounds.Size,new Point(Zero.X,Zero.Y),new Point(LastZeroPos.X,LastZeroPos.Y));
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