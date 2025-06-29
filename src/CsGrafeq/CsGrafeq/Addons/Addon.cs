using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.Base.Values;
namespace CsGrafeq.Addons
{
    public abstract class Addon
    {
        internal Control OpControl;
        protected abstract void Render(Graphics graphics, Rectangle size);
        internal void AddonRender(Graphics graphics, Rectangle size)
        {
            if (!Loaded)
                return;
            Render(graphics, size);
        }
        protected AddonMode _AddonMode;
        protected RenderMode _RenderMode;
        public AddonMode AddonMode
        {
            get=>_AddonMode;
        }
        public RenderMode RenderMode
        {
            get => _RenderMode;
        }
        public bool Enabled=true;
        private OwnerArguments _OwnerArguments;
        protected bool Loaded=false;
        internal OwnerArguments OwnerArguments
        {
            get
            {
                return _OwnerArguments;
            }
            set
            {
                _OwnerArguments = value;
                PixelToMathX = value.PMX;
                PixelToMathY = value.PMY;
                MathToPixelX = value.MPX;
                MathToPixelY = value.MPY;
                AskForRender = value.AskForRender;
                RefreshOwnerArguments();
                Loaded = true;
                OnLoaded();
            }
        }
        protected double UnitLengthX,UnitLengthY;
        protected PointL Zero;
        public Func<double, double> PixelToMathX,PixelToMathY,MathToPixelX,MathToPixelY;
        public Action AskForRender;
        public double[] Constants;
        public Size Size;
        public Rectangle Rectangle;
        protected void RefreshOwnerArguments()
        {
            OwnerArguments value= OwnerArguments;
            UnitLengthX = value.GetUX();
            UnitLengthY = value.GetUY();
            Zero = value.GetZero();
            Constants = value.GetConstants();
            Size = value.GetSize();
            Rectangle = new Rectangle(Point.Empty,Size);
        }
        internal virtual bool AddonOnKeyDown(KeyEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnKeyDown(e);
        }
        internal virtual bool AddonOnKeyUp(KeyEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnKeyUp(e);
        }
        internal virtual bool AddonOnKeyPress(KeyPressEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnKeyPress(e);
        }
        internal virtual bool AddonOnMouseMove(AddonMouseMoveEventArgs e)
        {
            if ((!Loaded) ||(!Enabled))
                return DoNext;
            return OnMouseMove(e);
        }
        internal virtual bool AddonOnMouseDown(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnMouseDown(e);
        }
        internal virtual bool AddonOnMouseUp(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnMouseUp(e);
        }
        internal virtual bool AddonOnMouseDoubleClick(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnMouseDoubleClick(e);
        }
        internal virtual bool AddonOnMouseClick(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnMouseClick(e);
        }
        internal virtual bool AddonOnMouseWheel(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnMouseWheel(e);
        }
        internal virtual bool AddonOnMouseLeave(EventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return DoNext;
            return OnMouseLeave(e);
        }
        //true代表可继续传递
        //false代表拦截
        protected virtual bool OnKeyDown(KeyEventArgs e)
        {
            return true;
        }
        protected virtual bool OnKeyUp(KeyEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnKeyPress(KeyPressEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseDown(MouseEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseMove(AddonMouseMoveEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseUp(MouseEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseClick(MouseEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseDoubleClick(MouseEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseWheel(MouseEventArgs e)
        {
            return DoNext;
        }
        protected virtual bool OnMouseLeave(EventArgs e)
        {
            return DoNext;
        }
        protected virtual void OnLoaded()
        {

        }
        
        private string _DebugStr="";
        [Conditional("DEBUG")]
        protected void SetDebugStr(string str)
        {
            _DebugStr = str;
        }
    }
    public struct AddonMouseMoveEventArgs
    {
        public int X;
        public int Y;
        public bool IsDown;
        public MouseButtons Button;
        public PointL MouseDownPoint;
        public PointL MouseDownZeroPoint;
        public MouseEventArgs MouseEventArgs;
    }
    public enum AddonMode
    {
        ForPlot,
        ForPixel
    }
    public enum RenderMode
    {
        All,
        Move,
    }
}
