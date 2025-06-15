using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.Base.Values;
namespace CsGrafeq.Base
{
    public abstract class Addon
    {
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
        protected void RefreshOwnerArguments()
        {
            OwnerArguments value= OwnerArguments;
            UnitLengthX = value.GetUX();
            UnitLengthY = value.GetUY();
            Zero = value.GetZero();
            Constants = value.GetConstants();
            Size = value.GetSize();
        }
        internal virtual bool AddonOnKeyDown(KeyEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnKeyDown(e);
        }
        internal virtual bool AddonOnKeyUp(KeyEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnKeyUp(e);
        }
        internal virtual bool AddonOnKeyPress(KeyPressEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnKeyPress(e);
        }
        internal virtual bool AddonOnMouseMove(AddonMouseMoveEventArgs e)
        {
            if ((!Loaded) ||(!Enabled))
                return true;
            return OnMouseMove(e);
        }
        internal virtual bool AddonOnMouseDown(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnMouseDown(e);
        }
        internal virtual bool AddonOnMouseUp(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnMouseUp(e);
        }
        internal virtual bool AddonOnMouseDoubleClick(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnMouseDoubleClick(e);
        }
        internal virtual bool AddonOnMouseClick(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnMouseClick(e);
        }
        internal virtual bool AddonOnMousWheel(MouseEventArgs e)
        {
            if ((!Loaded) || (!Enabled))
                return true;
            return OnMouseWheel(e);
        }
        //true代表可继续传递
        //false代表拦截
        protected virtual bool OnKeyDown(KeyEventArgs e)
        {
            return true;
        }
        protected virtual bool OnKeyUp(KeyEventArgs e)
        {
            return true;
        }
        protected virtual bool OnKeyPress(KeyPressEventArgs e)
        {
            return true;
        }
        protected virtual bool OnMouseDown(MouseEventArgs e)
        {
            return true;
        }
        protected virtual bool OnMouseMove(AddonMouseMoveEventArgs e)
        {
            return true;
        }
        protected virtual bool OnMouseUp(MouseEventArgs e)
        {
            return true;
        }
        protected virtual bool OnMouseClick(MouseEventArgs e)
        {
            return true;
        }
        protected virtual bool OnMouseDoubleClick(MouseEventArgs e)
        {
            return true;
        }
        protected virtual bool OnMouseWheel(MouseEventArgs e)
        {
            return true;
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
    internal struct OwnerArguments
    {
        public Action AskForRender;
        public Func<double> GetUX, GetUY;
        public Func<double, double> PMX, PMY, MPX, MPY;
        public Func<double[]> GetConstants;
        public Func<PointL> GetZero;
        public Func<Size> GetSize;
        public Func<int> GetPanelWidth;
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
