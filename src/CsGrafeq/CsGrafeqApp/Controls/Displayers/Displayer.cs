using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;
using System.Diagnostics;
using CsGrafeq;
using CsGrafeqApp.Addons;
using AddonPointerEventArgs = CsGrafeqApp.Addons.Addon.AddonPointerEventArgs;
using AddonPointerWheelEventArgs = CsGrafeqApp.Addons.Addon.AddonPointerWheelEventArgs;
using AddonPointerEventArgsBase= CsGrafeqApp.Addons.Addon.AddonPointerEventArgsBase;
using static System.Math;

namespace CsGrafeqApp.Controls.Displayers
{
    public abstract class Displayer:SkiaControl
    {
        public const bool DoNext = true;
        public const bool Intercept = false;
        protected SKBitmap TotalBuffer = new SKBitmap(1, 1);
        private bool CanPerform = true;
        [Content]
        public AddonList Addons { get; } = new();
        public Displayer():base()
        {
            Addons.CollectionChanged += ChildrenChanged;
        }
        protected sealed override void OnSkiaRender(SKCanvas dc)
        {
            lock (TotalBuffer)
            {
                dc.DrawBitmap(TotalBuffer, SKPoint.Empty);
            }
        }
        public abstract double MathToPixelX(double x);
        public abstract double MathToPixelY(double x);
        public abstract double PixelToMathX(double x);
        public abstract double PixelToMathY(double x);
        public AvaPoint MathToPixel(Vec vec)
        {
            return new AvaPoint(MathToPixelX(vec.X), MathToPixelY(vec.Y));
        }
        public SKPoint MathToPixelSK(Vec vec)
        {
            return new SKPoint((float)MathToPixelX(vec.X), (float)MathToPixelY(vec.Y));
        }
        public Vec PixelToMath(AvaPoint point)
        {
            return new Vec(PixelToMathX(point.X), PixelToMathY(point.Y));
        }
        public Vec PixelToMath(SKPoint point)
        {
            return new Vec(PixelToMathX(point.X), PixelToMathY(point.Y));
        }
        protected bool CallAddonPointerPressed(PointerPressedEventArgs e)
        {
            AvaPoint loc = e.GetPosition(this);
            var args = new AddonPointerEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers);
            foreach (var addon in Addons)
                if (addon.AddonPointerPressed(args) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonPointerMoved(PointerEventArgs e)
        {
            AvaPoint loc = e.GetPosition(this);
            var args = new AddonPointerEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers);
            foreach (var addon in Addons)
                if (addon.AddonPointerMoved(args) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonPointerReleased(PointerReleasedEventArgs e)
        {
            AvaPoint loc = e.GetPosition(this);
            var args = new AddonPointerEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers);
            foreach (var addon in Addons)
                if (addon.AddonPointerReleased(args) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonPointerWheeled(PointerWheelEventArgs e)
        {
            AvaPoint loc = e.GetPosition(this);
            var args = new AddonPointerWheelEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers,new Vec(e.Delta.X,e.Delta.Y));
            foreach (var addon in Addons)
                if (addon.AddonPointerWheeled(args) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonPointerTapped(TappedEventArgs e)
        {
            AvaPoint loc = e.GetPosition(this);
            var args = new AddonPointerEventArgsBase(loc.X, loc.Y,e.KeyModifiers);
            foreach (var addon in Addons)
                if (addon.AddonPointerTapped(args) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonPointerDoubleTapped(TappedEventArgs e)
        {
            AvaPoint loc = e.GetPosition(this);
            var args = new AddonPointerEventArgsBase(loc.X, loc.Y,e.KeyModifiers);
            foreach (var addon in Addons)
                if (addon.AddonPointerDoubleTapped(args) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonKeyDown(KeyEventArgs e)
        {
            foreach (var addon in Addons)
                if (addon.AddonKeyDown(e) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonKeyPress(KeyEventArgs e)
        {
            foreach (var addon in Addons)
                if (addon.AddonKeyPress(e) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected bool CallAddonKeyUp(KeyEventArgs e)
        {
            foreach (var addon in Addons)
                if (addon.AddonKeyUp(e) == Intercept)
                    return Intercept;
            return DoNext;
        }
        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            if (e.NewSize.Width > e.PreviousSize.Width || e.NewSize.Height > e.PreviousSize.Height)
            {
                if ((int)e.NewSize.Width != (int)e.PreviousSize.Width || (int)e.NewSize.Height != e.PreviousSize.Height)
                {
                    lock (TotalBuffer)
                    {
                        TotalBuffer.Dispose();
                        TotalBuffer = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                        foreach (var i in Addons)
                        {
                            i.Bitmap.Dispose();
                            i.Bitmap = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                        }
                        Invalidate();
                    }
                }
            }
        }
        public void Invalidate(Addon addon)
        {
            if(!CanPerform)
                return;
            if (Addons.Contains(addon))
            {
                using (SKCanvas dc=new SKCanvas(addon.Bitmap))
                {
                    dc.Clear();
                    addon.AddonRender(dc,Bounds.ToSKRect());
                }
            }
            CompoundBuffer();
            InvalidateVisual();
        }
        public void Invalidate()
        {
            if(!CanPerform)
                return;
            foreach(var i in Addons)
            {
                using (SKCanvas dc = new SKCanvas(i.Bitmap))
                {
                    dc.Clear(SKColors.Transparent);
                    i.AddonRender(dc, Bounds.ToSKRect());
                }
            }
            CompoundBuffer();
            InvalidateVisual();
        }
        public void InvalidateNotUpdate()
        {
            if(!CanPerform)
                return;
            foreach (var i in Addons)
            {
                using (SKCanvas dc = new SKCanvas(i.Bitmap))
                {
                    dc.Clear(SKColors.Transparent);
                    i.AddonRender(dc, Bounds.ToSKRect());
                }
            }
            CompoundBuffer();
        }

        public void InvalidateBuffer()
        {
            if(!CanPerform)
                return;
            CompoundBuffer();
            InvalidateVisual();
        }
        private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach(var adn in Addons)
            {
                if (adn.Owner == null)
                {
                    adn.Bitmap = new SKBitmap((int)Max(Bounds.Width, 1), (int)Max(Bounds.Height, 1));
                    adn.Owner = this;
                    Invalidate(adn);
                }
            }
        }
        public abstract void CompoundBuffer();

        public void Suspend()
        {
            CanPerform = false;
        }

        public void Resume(bool perform=true)
        {
            CanPerform = true;
            if(perform)
                Invalidate();
        }
    }
}
