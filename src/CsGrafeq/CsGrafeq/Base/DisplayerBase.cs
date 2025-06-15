using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.ExMethods;
using static CsGrafeq.Base.Values;
using static CsGrafeq.Expression.ExpressionBuilder;
using CsGrafeq.Base;
using System.Diagnostics;
using CsGrafeq.Geometry;
using System.Runtime.CompilerServices;

namespace CsGrafeq.Base
{
    internal sealed class DisplayerBase:Control
    {
        public static bool UseAntiAlias = true;
        private const int MINRENDERINTERVAL= 10;
        private Graphics TargetGraphics;
        private int width, height;
        private bool loaded = false;
        private BufferedGraphics buf;
        private LinkedList<AddonClass> Addons = new LinkedList<AddonClass>();
        private PointL _Zero = new PointL() { X = 250, Y = 250 };
        private OwnerArguments BasicOA = new OwnerArguments();
        private double[] Constants = new double['z' - 'a' + 1];
        private DateTime LastRenderTimer = DateTime.Now;
        private bool NeedRender;
        private readonly Timer RenderTimer = new Timer() { Interval = 200 };
        public int PanelWidth = 0;
        public bool CanMoveOrZoom = true;
        public bool DrawAxisLine = true;
        public bool DrawAxisNumber = true;
        public bool DrawAxisGrid = true;

        private double _UnitLengthX = 20.0001d;
        private double _UnitLengthY = 20.0001d;
        public double UnitLengthX
        {
            get { return _UnitLengthX; }
            set { _UnitLengthX = value; Render(); }
        }
        public double UnitLengthY
        {
            get { return _UnitLengthY; }
            set { _UnitLengthY = value; Render(); }
        }
        public PointL Zero
        {
            get { return _Zero; }
            set { _Zero = value; Render(); }
        }
        public Point ZeroInt
        {
            get { return new Point((int)_Zero.X, (int)_Zero.Y); }
            set { _Zero = new PointL(value.X, value.Y); Render(); }
        }
        public DisplayerBase()
        {
            Initialize();
            InitializeComponent();
            WheelingTimer.Tick += (s, e) =>
            {
                if (Wheeling)
                {
                    if ((DateTime.Now - WheelingTime).TotalMilliseconds > 150)
                    {
                        Wheeling = false;
                        Render(true);
                    }
                }
            };
            WheelingTimer.Start();
            RenderTimer.Tick += (s, e) =>
            {
                if (NeedRender)
                {
                    Render(true);
                }
            };
            RenderTimer.Start();

            BasicOA.PMX = PixelToMathX;
            BasicOA.PMY = PixelToMathY;
            BasicOA.MPX = MathToPixelX;
            BasicOA.MPY = MathToPixelY;
            BasicOA.GetZero = () => _Zero;
            BasicOA.GetUX = () => UnitLengthX;
            BasicOA.GetUY = () => UnitLengthY;
            BasicOA.GetConstants = () => Constants;
            BasicOA.GetSize = () => ClientSize;
            BasicOA.GetPanelWidth = () => PanelWidth;
        }
        private void Initialize()
        {
            Font = new Font("Consolas", 15);
            TargetGraphics = CreateGraphics();
            buf = TargetGraphics.GetBuffer(ClientRectangle);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            width=ClientSize.Width;
            height=ClientSize.Height;
            buf = TargetGraphics.GetBuffer(ClientRectangle);
            foreach(var addon in Addons)
            {
                addon.Image.Dispose();
                addon.Image=new Bitmap(width, height);
                addon.ImageGraphics = addon.Image.GetGraphics();
                addon.ImageGraphics.Clear(Color_Transparent);
            }
            Render(true);
        }
        private void AskRenderAddon(AddonClass addon)
        {
            addon.NeedRender = true;
        }
        public Addon GetAddon(string name)
        {
            return GetAddon(name,0);
        }
        public Addon GetAddon(string name, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            int i = 0;
            foreach (var addon in Addons)
            {
                if (addon.Addon.GetType().Name.ToLower() == name.ToLower())
                {
                    if (i == index)
                        return addon.Addon;
                    i++;
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }
        public T GetAddon<T>(int index) where T : Addon
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            int i = 0;
            foreach (var addon in Addons)
            {
                if (addon.Addon is T)
                {
                    if (i == index)
                        return (T)addon.Addon;
                    i++;
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }
        public void AppendAddon(Addon addon)
        {
            foreach(var i in Addons)
            {
                if (i.Addon == addon)
                {
                    return;
                }
            }
            AddonClass ac = new AddonClass(addon, ClientSize);
            Addons.AddLast(ac);
            BasicOA.AskForRender = () => AskRenderAddon(ac);
            addon.OwnerArguments = BasicOA;
        }
        public void RemoveAddon(Addon addon)
        {
            foreach (var i in Addons)
            {
                if (i.Addon == addon)
                {
                    Addons.Remove(i);
                }
            }
        }
        public void InsertAddonBefore(Addon addontoinsert, Addon s)
        {
            AddonClass ao;
            foreach (var i in Addons)
            {
                if (i.Addon == addontoinsert)
                {
                    Addons.Remove(i);
                    ao = i;
                    goto s;
                }
            }
            ao = new AddonClass(addontoinsert, ClientSize);
        s:
            foreach (var i in Addons)
            {
                if (i.Addon == s)
                    Addons.AddBefore(Addons.Find(i), ao);
            }
        }
        public void InsertAddonAfter(Addon addontoinsert, Addon s)
        {
            AddonClass ao;
            foreach (var i in Addons)
            {
                if (i.Addon == addontoinsert)
                {
                    Addons.Remove(i);
                    ao = i;
                    goto s;
                }
            }
            ao = new AddonClass(addontoinsert, ClientSize);
        s:
            foreach (var i in Addons)
            {
                if (i.Addon == s)
                    Addons.AddAfter(Addons.Find(i), ao);
            }
        }
        public void CallRender(Graphics graphics, Rectangle s, bool RenderToAddonImage, bool clearimage)
        {
            foreach (AddonClass addon in Addons)
            {
                Rectangle rect = s;
                if (addon.NeedRender)
                    rect = ClientRectangle;
                addon.Addon.AddonRender(graphics, rect);
                if (clearimage)
                    addon.ImageGraphics.Clear(Color_Transparent);
                if (RenderToAddonImage)
                    addon.Addon.AddonRender(addon.ImageGraphics, rect);
            }
        }
        public void CallRenderForPlotMode(Graphics graphics, Rectangle s)
        {
            foreach (AddonClass addon in Addons)
            {
                if(addon.Addon.AddonMode==AddonMode.ForPlot)
                    addon.Addon.AddonRender(graphics, s);
            }
        }
        public void CallRender(Graphics graphics, Rectangle s, Addon addon)
        {
            addon.AddonRender(graphics, s);
        }
        public void Render()
        {
            Render(false);
        }
        public void Render(bool IsForced)
        {
            if (IsForced||(DateTime.Now-LastRenderTimer).TotalMilliseconds>MINRENDERINTERVAL)
            {
                LastRenderTimer= DateTime.Now;
                NeedRender=false;
                Graphics graphics = buf.Graphics;
                graphics.Clear(Color_White);
                RenderAxisLine(graphics);
                CallRender(graphics, ClientRectangle, true, true);
                RenderAxisNumber(graphics);
                DrawDebugStr(graphics);
                buf.Render();
            }
            else
            {
                NeedRender=true;
            }
        }
        public void ReRenderAxis()
        {
            Graphics graphics = buf.Graphics;
            graphics.Clear(Color_White);
            RenderAxisLine(graphics);
            foreach (AddonClass addon in Addons)
            {
                graphics.DrawImage(addon.Image,0,0);
            }
            RenderAxisNumber(graphics);
            DrawDebugStr(graphics);
            buf.Render();
        }
        public void AskToRender(Addon caller)
        {
            Render();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!loaded)
            {
                //AppendAddon(new Implicit.ImplicitFunc(Sin(x)+Cos(y)==Tan(x*y)));
                //AppendAddon(new InkPad.InkPad());
            }
            loaded = true;
            ReRenderAxis();
        }
        #region Render
        private void RenderAxisLine(Graphics gb)
        {
            if (!loaded)
                return;
            Pen b0, b1, b2;
            b0 = new Pen(Color.FromArgb(190, 190, 190));
            b1 = new Pen(Color.FromArgb(128, 128, 128));
            Pen pb = Pen_Black;
            Pen bluepen = new Pen(Color.Blue, 3);
            //y
            if (RangeIn(0, width, _Zero.X) == _Zero.X)
            {
                if (DrawAxisGrid && !DrawAxisLine)
                    gb.DrawLine(
                        b1,
                        new PointF(_Zero.X, 0f),
                        new PointF(_Zero.X, height)
                    );
                else if(DrawAxisLine)
                    gb.DrawLine(
                        MouseOnYAxis ? bluepen : pb,
                        new PointF(_Zero.X, 0f),
                        new PointF(_Zero.X, height)
                    );
            }
            pb = Pen_Black;
            if (RangeIn(0, height, _Zero.Y) == _Zero.Y)
            {
                if (DrawAxisGrid && !DrawAxisLine)
                    gb.DrawLine(
                    b1 ,
                    new PointF(0f, _Zero.Y),
                    new PointF(width, _Zero.Y)
                );
                else if(DrawAxisLine)
                    gb.DrawLine(
                    MouseOnXAxis ? bluepen : pb,
                    new PointF(0f, _Zero.Y),
                    new PointF(width, _Zero.Y)
                );
            }
            if (!DrawAxisGrid)
                return;
            int zsX = (int)Math.Floor(Math.Log((350 / _UnitLengthX), 10));
            int zsY = (int)Math.Floor(Math.Log((350 / _UnitLengthY), 10));
            double addnumX = Math.Pow(10, zsX);
            double addnumY = Math.Pow(10, zsY);
            decimal addnumDX = (decimal)Math.Pow(10, zsX);
            decimal addnumDY = (decimal)Math.Pow(10, zsY);
            double p = RangeIn(- 3, height - Font.Height + 1, _Zero.Y);
            for (double i = Math.Min(_Zero.X - (addnumX * _UnitLengthX), MathToPixelX(Round(PixelToMathX(width), -zsX))); i > 0; i -= (addnumX * _UnitLengthX))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                if (num % (10 * addnumDX) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    (float)i, 0,
                    (float)i, height
                );
            }
            for (double i = Math.Max(_Zero.X + (addnumX * _UnitLengthX), MathToPixelX(Round(PixelToMathX(0), -zsX))); i < width; i += (addnumX * _UnitLengthX))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                if (num % (10 * addnumDX) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    (float)i, 0,
                    (float)i, height
                );
            }
            for (double i = Math.Min(_Zero.Y - (addnumY * _UnitLengthY), MathToPixelY(Round(PixelToMathY(height), -zsY))); i > 0; i -= (addnumY * _UnitLengthY))
            {
                decimal num = Round((decimal)PixelToMathY(i), -zsY);
                if (num % (10 * addnumDY) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    0, (float)i,
                    width, (float)i
                );
            }
            for (double i = Math.Max(_Zero.Y + (addnumY * _UnitLengthY), MathToPixelY(Round(PixelToMathY(0), -zsY))); i < height; i += (addnumY * _UnitLengthY))
            {
                decimal num = Round((decimal)PixelToMathY(i), -zsY);
                if (num % (10 * addnumDY) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    0, (float)i,
                    width, (float)i
                );
            }
        }
        private void RenderAxisNumber(Graphics gb)
        {
            if (!DrawAxisNumber)
                return;
            if (!loaded)
                return;
            int zsX = (int)Math.Floor(Math.Log((350 / _UnitLengthX), 10));
            int zsY = (int)Math.Floor(Math.Log((350 / _UnitLengthY), 10));
            double addnumX = Math.Pow(10, zsX);
            double addnumY = Math.Pow(10, zsY);
            decimal addnumDX = (decimal)Math.Pow(10, zsX);
            decimal addnumDY = (decimal)Math.Pow(10, zsY);
            double p = RangeIn(- 3, height- Font.Height + 1, _Zero.Y);
            float fff = 1f / 4f * Font.Height;
            for (double i = Math.Min(_Zero.X - (addnumX * _UnitLengthX), MathToPixelX(Round(PixelToMathX(width), -zsX))); i > 0; i -= (addnumX * _UnitLengthX))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                gb.DrawString(num.ToString(), Font, Brush_Black, (float)(i - (num.ToString().Length) * fff - 2), (float)p);
            }
            for (double i = Math.Max(_Zero.X + (addnumX * _UnitLengthX), MathToPixelX(Round(PixelToMathX(0), -zsX))); i < width; i += (addnumX * _UnitLengthX))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                gb.DrawString(num.ToString(), Font, Brush_Black, (float)(i - (num.ToString().Length) * fff - 2), (float)p);
            }
            for (double i = Math.Min(_Zero.Y - (addnumY * _UnitLengthY), MathToPixelY(Round(PixelToMathY(height), -zsY))); i > 0; i -= (addnumY * _UnitLengthY))
            {
                decimal num = Round((decimal)PixelToMathY(i), -zsY);
                if (PanelWidth+3 > _Zero.X)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, PanelWidth + 3, (float)(i - Font.Height / 2 - 2));
                }
                else if (_Zero.X + num.ToString().Length * Font.Height / 2 > width - 3)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)width - num.ToString().Length * Font.Height / 2 - 5, (float)(i - Font.Height / 2 - 2));
                }
                else
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)_Zero.X, (float)(i - Font.Height / 2 - 2));
                }
            }
            for (double i = Math.Max(_Zero.Y + (addnumY * _UnitLengthY), MathToPixelY(Round(PixelToMathY(0), -zsY))); i < height; i += (addnumY * _UnitLengthY))
            {
                decimal num = Round((decimal)PixelToMathY(i), -zsY);
                if (PanelWidth+3 > _Zero.X)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, PanelWidth + 3, (float)(i - Font.Height / 2 - 2));
                }
                else if (_Zero.X + num.ToString().Length * Font.Height / 2 >width - 3)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)width - num.ToString().Length * Font.Height / 2 - 5, (float)(i - Font.Height / 2 - 2));
                }
                else
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, _Zero.X, (float)(i - Font.Height / 2 - 2));
                }
            }
            gb.DrawString("0", Font, Brush_Black, _Zero.X + 3, _Zero.Y);
        }
        private void RenderMovedPlace(Graphics g)
        {
            if (_Zero.X < LastZeroPos.X)
            {
                CallRender(g, CreateRectByBound((int)(width - LastZeroPos.X + _Zero.X), 0, width, height), false,false);
                if (_Zero.Y < LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), (int)(width - LastZeroPos.X + _Zero.X), height), false, false);

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, 0, (int)(width - LastZeroPos.X + _Zero.X), (int)(_Zero.Y - LastZeroPos.Y)), false, false);
                }
            }
            else if (_Zero.X > LastZeroPos.X)
            {
                CallRender(g, CreateRectByBound(0, 0, (int)(_Zero.X - LastZeroPos.X), height), false, false);
                if (_Zero.Y < LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + _Zero.Y), width, height), false, false);

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), 0, width, (int)(_Zero.Y - LastZeroPos.Y)), false, false);
                }
            }
            else
            {
                if (_Zero.Y < LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), width, height), false, false);

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, 0, width, (int)(_Zero.Y - LastZeroPos.Y)), false, false);
                }
            }
        }
        private void RenderMovedPlace(Graphics g,Addon addon)
        {
            if (addon.AddonMode == AddonMode.ForPixel)
                return;
            if (_Zero.X < LastZeroPos.X)
            {
                CallRender(g, CreateRectByBound((int)(width - LastZeroPos.X + _Zero.X), 0, width, height),addon);
                if (_Zero.Y < LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), (int)(width - LastZeroPos.X + _Zero.X), height),addon);

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, 0, (int)(width - LastZeroPos.X + _Zero.X), (int)(_Zero.Y - LastZeroPos.Y)),addon);
                }
            }
            else if (_Zero.X > LastZeroPos.X)
            {
                CallRender(g, CreateRectByBound(0, 0, (int)(_Zero.X - LastZeroPos.X), height),addon);
                if (_Zero.Y < LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + _Zero.Y), width, height), addon);

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), 0, width, (int)(_Zero.Y - LastZeroPos.Y)), addon);
                }
            }
            else
            {
                if (_Zero.Y < LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), width, height), addon);

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    CallRender(g, CreateRectByBound(0, 0, width, (int)(_Zero.Y - LastZeroPos.Y)), addon);
                }
            }
        }
        #endregion
        #region PointConvertMethods

        private double MathToPixelX(double d)
        {
            return _Zero.X + d * _UnitLengthX;
        }

        private double MathToPixelY(double d)
        {
            return _Zero.Y + -d * _UnitLengthY;
        }
        private double PixelToMathX(double d)
        {
            return ((d - _Zero.X) / _UnitLengthX);
        }
        private double PixelToMathY(double d)
        {
            return -(d - _Zero.Y) / _UnitLengthY;
        }
        #endregion
        #region OverridedMethods
        private PointL MouseDownPos = new PointL() { X = 0, Y = 0 };
        private PointL MouseDownZeroPos = new PointL() { X = 0, Y = 0 };
        private PointL LastZeroPos;
        private bool MouseDownLeft = false;
        private bool MouseDownRight = false;
        private bool MouseOnXAxis = false;
        private bool MouseOnYAxis = false;
        protected override void OnMouseClick(MouseEventArgs e)
        {
            this.Focus();
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnMouseClick(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            base.OnMouseClick(e);
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            this.Focus();
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnMouseDoubleClick(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            base.OnMouseDoubleClick(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnMouseDown(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            if (e.Button == MouseButtons.Left)
            {
                MouseDownLeft = true;
                MouseDownPos = new PointL() { X = e.X, Y = e.Y };
                MouseDownZeroPos = _Zero;
            }
            else if (e.Button == MouseButtons.Right)
            {
                MouseDownRight = true;
            }
            LastZeroPos = _Zero;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.Focus();
            if (!CanMoveOrZoom)
                return;
            AddonMouseMoveEventArgs eventargs=new AddonMouseMoveEventArgs() {X=e.X,Y=e.Y,Button=e.Button,IsDown=MouseDownLeft||MouseDownRight,MouseDownPoint=MouseDownPos,MouseDownZeroPoint=MouseDownZeroPos,MouseEventArgs=e};
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnMouseMove(eventargs))
                {
                    EventOver();
                    Graphics gg = buf.Graphics;
                    gg.Clear(Color_White);
                    RenderAxisLine(gg);
                    foreach (var i in Addons)
                    {
                        gg.DrawImage(i.Image, 0, 0);
                    }
                    RenderAxisNumber(gg);
                    DrawDebugStr(gg);
                    buf.Render();
                    return;
                }
            EventOver();
            bool l = MouseOnYAxis, ll = MouseOnXAxis;
            MouseOnYAxis = Math.Abs(e.X - _Zero.X) < 3;
            MouseOnXAxis = Math.Abs(e.Y - _Zero.Y) < 3;
            if ((l != MouseOnYAxis || ll != MouseOnXAxis) )
            {
                Graphics gg = buf.Graphics;
                gg.Clear(Color_White);
                RenderAxisLine(gg);
                foreach (var i in Addons)
                {
                    gg.DrawImage(i.Image, 0, 0);
                }
                RenderAxisNumber(gg);
                DrawDebugStr(gg);
                buf.Render();
                LastRenderTimer = DateTime.Now;
                return;
            }

            if (MouseDownLeft)
            {//移动零点
                _Zero.X = (MouseDownZeroPos.X + e.X - MouseDownPos.X);
                _Zero.Y = (MouseDownZeroPos.Y + e.Y - MouseDownPos.Y);

                if ((DateTime.Now - LastRenderTimer).TotalMilliseconds > MINRENDERINTERVAL)
                {
                    Graphics graphics = buf.Graphics;
                    graphics.Clear(Color_White);
                    RenderAxisLine(graphics);
                    if (false && Math.Abs(LastZeroPos.X - _Zero.X) > 20 || Math.Abs(LastZeroPos.Y - _Zero.Y) > 20 || e.Clicks == 142857)
                    {
                        Bitmap tmpbmp = new Bitmap(width, height);
                        //需修改图片
                        foreach (var i in Addons)
                        {
                            if (i.Addon.AddonMode == AddonMode.ForPlot)
                            {
                                tmpbmp = new Bitmap(i.Image);
                                i.ImageGraphics.Clear(Color_Transparent);
                                i.ImageGraphics.DrawImage(tmpbmp, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                                tmpbmp.Dispose();
                                if(i.Addon.RenderMode==RenderMode.Move)
                                    RenderMovedPlace(i.ImageGraphics, i.Addon);
                                else
                                {
                                    i.ImageGraphics.Clear(Color_Transparent);
                                    i.Addon.AddonRender(i.ImageGraphics, ClientRectangle);
                                }
                            }
                            graphics.DrawImage(i.Image, 0, 0);
                        }
                        LastZeroPos = _Zero;
                    }
                    else
                    {
                        foreach (var i in Addons)
                        {
                            if(i.Addon.AddonMode==AddonMode.ForPixel)
                                graphics.DrawImage(i.Image, 0, 0);
                            else if (i.Addon.RenderMode==RenderMode.Move)
                                graphics.DrawImage(i.Image, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                                

                            if (i.Addon.RenderMode == RenderMode.Move)
                                RenderMovedPlace(i.ImageGraphics, i.Addon);
                            else
                            {
                                i.ImageGraphics.Clear(Color_Transparent);
                                i.Addon.AddonRender(i.ImageGraphics, ClientRectangle);
                                graphics.DrawImage(i.Image, 0, 0);
                            }
                        }
                    }
                    RenderAxisNumber(graphics);
                    DrawDebugStr(graphics);
                    buf.Render();
                    LastRenderTimer = DateTime.Now;
                }
                else
                {
                    NeedRender = true;
                }
            }
            return;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Focus();
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnMouseUp(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            if (e.Button == MouseButtons.Left)
            {
                MouseDownLeft = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                MouseDownRight = false;
            }
            if (LastZeroPos != _Zero)
                Render(true);
            LastZeroPos = _Zero;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            this.Focus();
            MouseDownLeft = MouseDownRight = false;
            Render();
        }
        private bool Wheeling = false;
        private DateTime WheelingTime = DateTime.Now;
        private double previousunitlengthX, previousunitlengthY;
        private PointL previouszero;
        private readonly Timer WheelingTimer = new Timer() { Interval = 30 };
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.Focus();
            if (!CanMoveOrZoom)
                return;
            WheelingTime = DateTime.Now;
            if (!Wheeling)
            {
                Wheeling = true;
                previousunitlengthX = _UnitLengthX;
                previousunitlengthY = _UnitLengthY;
                previouszero = _Zero;
            }
            double cursor_x = e.X, cursor_y = e.Y;
            double times_x = (_Zero.X - cursor_x) / _UnitLengthX;
            double times_y = (_Zero.Y - cursor_y) / _UnitLengthY;
            double delta;
            delta = Math.Pow(Math.Log(Math.Abs(e.Delta) + 1) + 1, 0.1);
            if (e.Delta == 120)
                delta = 1.3;
            if (e.Delta == -120)
                delta = 1.3;
            if (e.Delta > 0)
            {
                if (MouseOnXAxis && MouseOnYAxis)
                {
                    _UnitLengthX *= delta;
                    _UnitLengthY *= delta;
                }
                else if (MouseOnYAxis)
                    _UnitLengthY *= delta;
                else if (MouseOnXAxis)
                    _UnitLengthX *= delta;
                else
                {
                    _UnitLengthX *= delta;
                    _UnitLengthY *= delta;
                }
            }
            else
            {
                if (MouseOnXAxis && MouseOnYAxis)
                {
                    _UnitLengthX /= delta;
                    _UnitLengthY /= delta;
                }
                else if (MouseOnYAxis)
                    _UnitLengthY /= delta;
                else if (MouseOnXAxis)
                    _UnitLengthX /= delta;
                else
                {
                    _UnitLengthX /= delta;
                    _UnitLengthY /= delta;
                }
            }
            _UnitLengthX = RangeIn(0.01, 1000000, _UnitLengthX);
            _UnitLengthY = RangeIn(0.01, 1000000, _UnitLengthY);
            _Zero = new PointL()
            {
                X = (long)(times_x * _UnitLengthX + cursor_x),
                Y = (long)(times_y * _UnitLengthY + cursor_y)
            };
            double ratioX = _UnitLengthX / previousunitlengthX;
            double ratioY = _UnitLengthY / previousunitlengthY;
            if (ratioX > 2 || ratioX < 0.5 || ratioY > 2 || ratioY < 0.5)
            {
                previousunitlengthX = _UnitLengthX;
                previousunitlengthY = _UnitLengthY;
                previouszero = _Zero;
                Render(true);
            }
            else
            {
                if ((DateTime.Now - LastRenderTimer).TotalMilliseconds > MINRENDERINTERVAL)
                {
                    Graphics graphics = buf.Graphics;
                    graphics.Clear(Color_White);
                    RenderAxisLine(graphics);
                    RectangleF rect = new RectangleF(
                        (float)(_Zero.X - previouszero.X * ratioX),
                        (float)(_Zero.Y - previouszero.Y * ratioY),
                        (float)(ratioX * width),
                        (float)(ratioY * height));
                    foreach (var i in Addons)
                    {
                        if (i.Addon.AddonMode == AddonMode.ForPlot)
                            graphics.DrawImage(i.Image, rect);
                        else
                            graphics.DrawImage(i.Image, 0, 0);
                    }
                    RenderAxisNumber(graphics);
                    DrawDebugStr(graphics);
                    buf.Render();
                    LastRenderTimer = DateTime.Now;
                }
                else
                {
                    NeedRender = true;
                }
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnKeyDown(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnKeyUp(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            base.OnKeyUp(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            foreach (var addon in Addons)
                if (!addon.Addon.AddonOnKeyPress(e))
                {
                    EventOver();
                    return;
                }
            EventOver();
            base.OnKeyPress(e);
        }
        private void EventOver()
        {
            bool flag = false;
            foreach (var i in Addons)
            {
                if (i.NeedRender)
                {
                    i.NeedRender = false;
                    i.ImageGraphics.Clear(Color_Transparent);
                    CallRender(i.ImageGraphics, ClientRectangle, i.Addon);
                    flag = true;
                }
            }
            if (flag)
                ReRenderAxis();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DisplayerBase
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Name = "DisplayerBase";
            this.Size = new System.Drawing.Size(500, 500);
            this.ResumeLayout(false);

        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Delete:
                    return false;
            }
            return base.ProcessDialogKey(keyData);
        }
        #endregion
        public static string DebugStr;
        [Conditional("DEBUG")]
        public static void DrawDebugStr(Graphics g)
        {
            g.DrawBubblePopup(DebugStr, new Font("Arial", 9), new Point(1, 1), Color.White);
        }

    }
    public static class Values
    {
        public static Color Color_White, Color_Black, Color_Transparent,Color_Blue,Color_T_Grey,Color_Red;
        public static SolidBrush Brush_White, Brush_Black,Brush_Blue,Brush_T_Grey,Brush_Red;
        public static Pen Pen_White, Pen_Black,Pen_Blue,Pen_Red;
        public static (bool, bool) TT = (true, true);
        public static (bool, bool) FT = (false, true);
        public static (bool, bool) FF = (false, false);
        static Values()
        {
            Color_White = Color.White;
            Color_Black = Color.Black;
            Color_Blue= Color.Blue;
            Color_Red= Color.Red;   
            Color_T_Grey = Color.FromArgb(170,Color.Gray);
            Brush_T_Grey=new SolidBrush(Color_T_Grey);
            Color_Transparent = Color.FromArgb(0, 255, 255, 255);
            Pen_White = new Pen(Color_White, 1);
            Pen_Black = new Pen(Color_Black, 1);
            Pen_Blue=new Pen(Color_Blue, 1);
            Pen_Red = new Pen(Color_Red,1);
            Brush_Blue = new SolidBrush(Color_Blue);
            Brush_White = new SolidBrush(Color_White);
            Brush_Black = new SolidBrush(Color_Black);
            Brush_Red=new SolidBrush (Color_Red);
        }
    }
    public struct RenderArguments
    {
        public double UnitLengthX, UnitLengthY;
        public PointL Zero;
        public Func<double, double> PMX, PMY, MPX, MPY;
        public double[] Constants;
    }
    internal class AddonClass
    {
        public AddonClass(Addon addon,Size size) { 
            Addon = addon;
            Image=new Bitmap(size.Width, size.Height);
            ImageGraphics = Image.GetGraphics();
        }
        public Addon Addon;
        public Bitmap Image;
        public Graphics ImageGraphics;
        public bool NeedRender;
    }
}
namespace CsGrafeq
{
    internal static partial class ExMethods
    {
        public static BufferedGraphics GetBuffer(this Graphics graphics, Rectangle r)
        {
            BufferedGraphics bg= BufferedGraphicsManager.Current.Allocate(graphics, r);
            if (DisplayerBase.UseAntiAlias)
            {
                bg.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                bg.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                bg.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                bg.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
            return bg;
        }
        public static Graphics GetGraphics(this Image image)
        {
            Graphics g = Graphics.FromImage(image);
            if (DisplayerBase.UseAntiAlias)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
            return g;
        }
        private static Color Color_White_HalfTransparent = Color.FromArgb(70, Color.Gray);
        private static Brush CWHTBrush=new SolidBrush(Color_White_HalfTransparent);
        private static int r=7;
        public static void DrawBubblePopup(this Graphics graphics, string s, Font font, Point p)
        {
            graphics.DrawBubblePopup( s, font, p,Color_White_HalfTransparent);
        }
        public static void DrawBubblePopup(this Graphics graphics, string s, Font font, Point p,Color color)
        {
            SolidBrush CWHTBrush=new SolidBrush(color);
            if (p.X < -100 || p.X > 100000 || p.Y < -100 || p.Y > 100000)
                return;
            Size strsize = graphics.MeasureString(s, font).ToSize();
            if (strsize.Width < 4 || strsize.Height < 4)
            {
                graphics.FillRectangle(CWHTBrush, new Rectangle(new Point(p.X, p.Y), new Size(strsize.Width + 4, strsize.Height + 4)));
                graphics.DrawString(s, font, Brush_Black, new Point(p.X + 2, p.Y + 2));
                return;
            }
            strsize.Width += 4;
            strsize.Height += 4;
            graphics.FillPie(CWHTBrush, new Rectangle(p, new Size(2 * r, 2 * r)), 180, 90);
            graphics.FillPie(CWHTBrush, new Rectangle(new Point(p.X + strsize.Width - 2 * r, p.Y), new Size(2 * r, 2 * r)), 270, 90);
            graphics.FillPie(CWHTBrush, new Rectangle(new Point(p.X + strsize.Width - 2 * r, p.Y + strsize.Height - 2 * r), new Size(2 * r, 2 * r)), 0, 90);
            graphics.FillPie(CWHTBrush, new Rectangle(new Point(p.X, p.Y + strsize.Height - 2 * r), new Size(2 * r, 2 * r)), 90, 90);
            graphics.FillRectangle(CWHTBrush, new RectangleF(new PointF(p.X + r - 0.3f, p.Y), new SizeF(strsize.Width - 2 * r + 0.6f, strsize.Height)));
            graphics.FillRectangle(CWHTBrush, new RectangleF(new PointF(p.X, p.Y + r - 0.3f), new SizeF(r + 0.3f, strsize.Height - 2 * r + 0.3f)));
            graphics.FillRectangle(CWHTBrush, new RectangleF(new PointF(p.X + strsize.Width - r, p.Y + r - 0.3f), new SizeF(r + 0.3f, strsize.Height - 2 * r + 0.3f)));
            graphics.DrawString(s, font, Brush_Black, new Point(p.X + 2, p.Y + 2));
        }
        public static Point OffSetBy(this Point p,int dx,int dy)
        {
            p.Offset(dx,dy);
            return p;
        }
        public static decimal Round(decimal num, int fix)
        {
            num /= Pow(10, -fix);
            num = Math.Round(num, 0);
            num *= Pow(10, -fix);
            return num;
        }
        private static decimal Pow(decimal num, int times)
        {
            decimal result = 1;
            if (times > 0)
            {
                for (int i = 0; i < times; i++)
                {
                    result *= num;
                }
                return result;
            }
            else if (times == 0)
            {
                return 1;
            }
            else
            {
                for (int i = 0; i > times; i--)
                {
                    result /= num;
                }
                return result;
            }
        }
        public static double RangeIn(double min, double max, double num)
        {
            if (min > num) { return min; }
            if (max < num) { return max; }
            return num;
        }
        public static float Round(float num, int fix)
        {
            return (float)Math.Round(num, fix);
        }
        public static double Round(double num, int fix)
        {
            if (fix < 0)
            {
                num /= Math.Pow(10, -fix);
                num = Math.Round(num, 0);
                num *= Math.Pow(10, -fix);
                return num;
            }
            return Math.Round(num, fix);
        }
    }
    public struct PointL
    {
        public long X;
        public long Y;
        public PointL(long x, long y)
        {
            X = x; Y = y;
        }
        public static bool operator ==(PointL left, PointL right)
        {
            return left.X == right.X && left.Y == right.Y;
        }
        public static bool operator !=(PointL left, PointL right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return $"{{{X},{Y}}}";
        }
    }
    public struct Vec
    {
        public double X;
        public double Y;
        public Vec(double x, double y)
        {
            X = x; Y = y;
        }
        public Vec(Point p)
        {
            X = p.X; Y = p.Y;
        }
        public static bool operator ==(Vec left, Vec right)
        {
            return left.X.Equals( right.X) && left.Y.Equals(right.Y);
        }
        public static bool operator !=(Vec left, Vec right)
        {
            return !(left == right);
        }
        public static Vec operator +(Vec left, Vec right)
        {
            return new Vec(left.X + right.X, left.Y + right.Y);
        }
        public static Vec operator -(Vec left, Vec right)
        {
            return new Vec(left.X - right.X, left.Y - right.Y);
        }
        public static Vec operator *(Vec left, double ratio)
        {
            return new Vec(left.X * ratio, left.Y * ratio);
        }
        public static Vec operator /(Vec left, double ratio)
        {
            return new Vec(left.X / ratio, left.Y / ratio);
        }
        public double GetLength()
        {
            return Math.Sqrt(X*X+Y*Y);
        }
        public static double operator *(Vec left, Vec right)
        {
            return left.X*right.X+left.Y*right.Y;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return $"{{{X},{Y}}}";
        }
        public static implicit operator Vec((double,double) TTuple)
        {
            return new Vec(TTuple.Item1, TTuple.Item2);
        }
        public bool IsInValidVec()
        {
            return double.IsNaN(X) || double.IsNaN(Y);
        }
        public static readonly Vec InvalidVec = new Vec(double.NaN,double.NaN);
		public PointF ToPointF()
		{
			return new PointF((float)X, (float)Y);
		}
		public Point ToPoint()
		{
			return new Point((int)X, (int)Y);
		}
		public double Arg2()
        {
            return Math.Atan2(Y,X).Mod(2*Math.PI);
        }
        public double Arg1()
        {
            return Math.Atan(Y/X);
        }
        public Vec Unit()
        {
            double arg = Arg2();
            return new Vec(Math.Cos(arg),Math.Sin(arg));
        }
    }
    public struct LinearFunc
    {
        public Vec Point1,Point2;
        public LinearFunc(Vec point1,Vec point2)
        {
            this.Point1 = point1;
            this.Point2 = point2;
        }

    }

}