using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CsGrafeqApp.Classes;
using CsGrafeqApp.Controls.Displayers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Addons
{
    public abstract class Addon:AvaloniaObject
    {
        public const bool DoNext = true;
        public const bool Intercept = false;
        private readonly OnceLock IsLoaded=new OnceLock();
        public bool IsEnabled = true;
        private Displayer? _Owner;
        //Addon内部勿动
        public SKBitmap Bitmap=new SKBitmap();
        public UserControl OperationControl
        {
            get; init;
        }=new UserControl();
        protected Addon() { }
        public virtual Displayer? Owner
        {
            get => _Owner;
            set { 
                _Owner = value;
                IsLoaded.SetValueTrue();
            }
        }
        public abstract string Name { get; }
        public string asdfasdf { get; } = "123";
        protected abstract void Render(SKCanvas dc, SKRect rect);
        internal void AddonRender(SKCanvas dc, SKRect rect)
        {
            if (IsLoaded.Value)
                Render(dc, rect);
        }
        internal bool AddonKeyDown(KeyEventArgs e)
        {
            if (!IsLoaded.Value || !IsEnabled)
                return DoNext;
            return KeyDown(e);
        }
        internal bool AddonKeyUp(KeyEventArgs e)
        {
            if (!IsLoaded.Value || !IsEnabled)
                return DoNext;
            return KeyUp(e);
        }
        internal virtual bool AddonPointerMoved(AddonPointerEventArgs e)
        {
            if (!IsLoaded.Value || !IsEnabled)
                return DoNext;
            return PointerMoved(e);
        }
        internal virtual bool AddonPointerPressed(AddonPointerEventArgs e)
        {
            if (!IsLoaded.Value || !IsEnabled)
                return DoNext;
            return PointerPressed(e);
        }
        internal virtual bool AddonPointerReleased(AddonPointerEventArgs e)
        {
            if (!IsLoaded.Value || !IsEnabled)
                return DoNext;
            return PointerReleased(e);
        }
        internal virtual bool AddonPointerWheeled(AddonPointerWheelEventArgs e)
        {
            if (!IsLoaded.Value || !IsEnabled)
                return DoNext;
            return PointerWheeled(e);
        }
        //true代表可继续传递
        //false代表拦截
        protected virtual bool KeyDown(KeyEventArgs e) => DoNext;
        protected virtual bool KeyUp(KeyEventArgs e) => DoNext;
        protected virtual bool KeyPress(KeyEventArgs e) => DoNext;
        protected virtual bool PointerReleased(AddonPointerEventArgs e) => DoNext;
        protected virtual bool PointerPressed(AddonPointerEventArgs e) => DoNext;
        protected virtual bool PointerMoved(AddonPointerEventArgs e) => DoNext;
        protected virtual bool PointerClicked(AddonPointerEventArgs e) => DoNext;
        protected virtual bool PointerDoubleClicked(AddonPointerEventArgs e) => DoNext;
        protected virtual bool PointerWheeled(AddonPointerWheelEventArgs e) => DoNext;
        private class OnceLock
        {
            private bool _Value=false;
            public bool Value
            {
                get => _Value;
            }
            public void SetValueTrue()
            {
                _Value=true;
            }
        }
        public class AddonPointerEventArgs : EventArgs
        {
            public readonly double X, Y;
            public readonly PointerPointProperties Properties;
            public readonly KeyModifiers modifiers;
            public AddonPointerEventArgs(double x, double y, PointerPointProperties properties, KeyModifiers modifiers)
            {
                X = x;
                Y = y;
                Properties= properties;
                this.modifiers = modifiers;
            }

            public Point Location => new Point(X,Y);
        }
        public class AddonPointerWheelEventArgs : AddonPointerEventArgs
        {
            public readonly Vec Delta;   
            public AddonPointerWheelEventArgs(double x, double y, PointerPointProperties properties, KeyModifiers modifiers,Vec delta) : base(x, y, properties,modifiers)
            {
                Delta = delta;
            }
        }
    }
}
