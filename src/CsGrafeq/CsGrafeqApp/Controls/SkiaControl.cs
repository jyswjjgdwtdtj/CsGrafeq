using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Controls
{
    public class SkiaControl:TemplatedControl
    {
        private CustomDrawOperation CustomDrawOper;
        private Rect _ValidRect;
        public SkiaControl()
        {
            Background = new SolidColorBrush(Colors.Red);
            CustomDrawOper = new CustomDrawOperation(Bounds);
            CustomDrawOper.OnDraw += (SKCanvas dc) =>
            {
                OnSkiaRender(dc);
            };
        }
        public Rect ValidRect
        {
            get => _ValidRect;
            set => _ValidRect = value;
        }
        protected virtual void OnSkiaRender(SKCanvas dc)
        {
            
        }
        public sealed override void Render(DrawingContext context)
        {
            context.FillRectangle(new SolidColorBrush(Colors.Transparent), Bounds);
            context.Custom(CustomDrawOper);
        }
        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            CustomDrawOper.Bounds = new Rect(e.NewSize);
            ValidRect = new Rect(e.NewSize);
        }
        private class CustomDrawOperation : ICustomDrawOperation
        {
            private Rect _Bounds;
            public CustomDrawOperation(Rect bounds)
            {
                _Bounds = bounds;
            }
            public void Dispose()
            {
            }
            public event Action<SKCanvas>? OnDraw;
            public Rect Bounds { get => _Bounds; set => _Bounds = value; }
            public bool HitTest(Point p) => true;
            public bool Equals(ICustomDrawOperation? other) => false;
            public void Render(ImmediateDrawingContext context)
            {
                var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
                if (leaseFeature != null)
                {
                    using var lease = leaseFeature.Lease();
                    var canvas = lease.SkCanvas;
                    OnDraw?.Invoke(canvas);
                }
            }
        }
        protected class PropertyResetEventArgs<T>(T FormerValue)
        {
            public readonly T Value=FormerValue;
        }
    }
    public static class SkiaEx
    {
        public static SKColor Dark = new SKColor(0x00, 0xa6, 0x7e);
        public static SKColor Light = new SKColor(0x88, 0xff, 0xcc);
        public static SKColor Median = new SKColor(0, 0xb8, 0x9f);
        public static SKPaint FilledBlack = new SKPaint() {IsAntialias=true };
        public static SKPaint FilledRed = new SKPaint() { IsAntialias = true };
        public static SKPaint StrokeBlack = new SKPaint() { IsAntialias = true };
        public static SKPaint StrokeRed = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledBlue = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledWhite = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledTranparentGrey = new SKPaint() { IsAntialias = true };
        public static SKFont TextFont;
        public static SKPaint PolygonPaint = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledGray1 = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledGray2 = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledBlackShady = new SKPaint() { IsAntialias = true };
        public static SKPaint StrokeBlackShady = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledDark = new SKPaint() { IsAntialias = true };
        public static SKPaint StrokeDark = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledLight = new SKPaint() { IsAntialias = true };
        public static SKPaint StrokeLight = new SKPaint() { IsAntialias = true };
        public static SKPaint FilledMedian = new SKPaint() { IsAntialias = true };
        public static SKPaint UnshadyFilledMedian = new SKPaint() { IsAntialias = true,Color=Median };
        public static SKPaint StrokeMedian = new SKPaint() { IsAntialias = true };
        public static SKPaint UnshadyStrokeMedian = new SKPaint() { IsAntialias = true, Color = Median,IsStroke=true };
        public static SKPaint FilledTpMedian = new SKPaint() { IsAntialias = true };
        public static SKPaint AntiAlias = new SKPaint() { IsAntialias = true };
        static SkiaEx()
        {
            FilledBlack.Color = new SKColor(0, 0, 0);
            FilledRed.Color = new SKColor(255, 0, 0);
            StrokeBlack.Color = new SKColor(0,0,0);
            StrokeRed.Color = new SKColor(255,0,0);
            StrokeRed.IsStroke = true;
            StrokeBlack.IsStroke = true;
            FilledBlue.Color = new SKColor(0, 0, 255);
            FilledWhite.Color = new SKColor(255, 255, 255);
            FilledTranparentGrey.Color = new SKColor(0x80, 0x80, 0x80, 70);
            FilledTranparentGrey.BlendMode = SKBlendMode.SrcOver;
            TextFont = new SKFont(SKTypeface.FromFamilyName("Microsoft Yahei UI"));
            FilledGray1.Color = new SKColor(190, 190, 190);
            FilledGray2.Color = new SKColor(128, 128, 128);
            FilledBlackShady.Color = new SKColor(0, 0, 0);
            FilledBlackShady.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: SKColors.Gray
            );
            StrokeBlackShady.Color = new SKColor(0, 0, 0);
            StrokeBlackShady.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: SKColors.Gray
            );
            StrokeBlack.IsStroke = true;
            FilledDark.Color = Dark;
            FilledDark.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: Median
            );
            StrokeDark.Color = Dark;
            StrokeDark.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: Median
            );
            StrokeDark.IsStroke = true;
            FilledLight.Color = Median;
            FilledLight.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: Light
            );
            StrokeLight.Color = Median;
            StrokeLight.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: Light
            );
            StrokeLight.IsStroke = true;
            FilledMedian.Color = Median;
            FilledMedian.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: Light
            );
            StrokeMedian.Color = Median;
            StrokeMedian.ImageFilter = SKImageFilter.CreateDropShadow(
                dx: 0,
                dy: 0,
                sigmaX: 2,
                sigmaY: 2,
                color: Light
            );
            StrokeMedian.IsStroke = true;
            FilledTpMedian.Color = Median.WithAlpha(90);

        }
        public static void DrawBubble(this SKCanvas dc, string s, SKPoint point)
        {
            SKSize size = new SKSize();
            size.Width = TextFont.MeasureText(s, FilledBlack);
            size.Height = TextFont.Size;
            dc.DrawRoundRect(new SKRoundRect(CreateSKRectWH(point.X, point.Y, size.Width + 4, size.Height + 4), 4), FilledTranparentGrey);
            dc.DrawText(s, new SKPoint(point.X + 2, point.Y + 2 + size.Height / 2 + 4), TextFont, FilledBlack);
        }
        public static SKPoint OffSetBy(this SKPoint point, float dx, float dy)
        {
            return new SKPoint(point.X + dx, point.Y + dy);
        }
        public static SKRect CreateSKRectWH(float x, float y, float width, float height)
        {
            return new SKRect(x, y, x + width, y + height);
        }
        public static SKRect CreateSKRectWH(double x, double y, double width, double height)
        {
            return new SKRect((float)x, (float)y, (float)(x + width), (float)(y + height));
        }
        public static SKRect ToSKRect(this Avalonia.Rect rect)
        {
            return new SKRect((float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height);
        }
        public static bool ContainsPoint(this SKRect rect, SKPoint point)
        {
            return rect.Top < point.Y&&point.Y < rect.Bottom&&rect.Left<point.X&&point.X<rect.Right;
        }
    }
}
