using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsGrafeqApplication.Addons
{
    public class Renderable:IDisposable
    {
        public bool Changed { get; set; } = false;
        internal SKBitmap Bitmap = new();
        internal event Action<SKCanvas, SKRect>? OnRender;
        internal void Render(SKCanvas dc, SKRect rect)
        {
            OnRender?.Invoke(dc, rect);
        }
        public void Dispose()
        {
            Bitmap?.Dispose();
            GC.SuppressFinalize(this);
        }
        ~Renderable()
        {
            Dispose();
        }
    }
}
