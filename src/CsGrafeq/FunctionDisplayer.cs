using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.ExtendedMethods;

namespace CsGrafeq
{
    public class FunctionDisplayer : AxisDisplayer
    {
        private readonly List<ImplicitFunction> ImpFuncs = new List<ImplicitFunction>() {  };
        private int _Quality = 0;
        private Bitmap Bitmap;
        public int Quality
        {
            get
            {
                return _Quality;
            }
            set
            {
                _Quality = (int)Range(0, 4, value);
                Render(TargetGraphics);
            }
        }
        public FunctionDisplayer() : base()
        {
            Bitmap = new Bitmap(Width, Height);
            WheelingTimer.Tick += (s, e) =>
            {
                if (Wheeling)
                {
                    if ((DateTime.Now - WheelingTime).TotalMilliseconds > 150)
                    {
                        Wheeling = false;
                        Render(TargetGraphics);
                    }
                }
            };
            WheelingTimer.Start();
        }
        public void AddExpression(string expression)
        {
            try
            {
                ImpFuncs.Add(new ImplicitFunction(expression));
                Render(TargetGraphics);
            }
            catch
            {
                throw;
            }
        }
        public void RemoveExpression(string expression)
        {
            for (int i = 0; i < ImpFuncs.Count; i++)
            {
                if (ImpFuncs[i].Expression == expression)
                {
                    ImpFuncs.RemoveAt(i);
                    return;
                }
            }
            throw new Exception("算式不存在");
        }

        protected override void Render(Graphics rt)
        {
            if ((!loaded) || (!Visible))
                return;
            Graphics graphics = buf.Graphics;
            graphics.Clear(Color_White);
            Graphics bmpgraphics = Graphics.FromImage(Bitmap);
            bmpgraphics.Clear(Color_A);
            RenderImpFuncs(bmpgraphics, ClientRectangle);
            RenderAxisLine(graphics,ClientRectangle);
            graphics.DrawImage(Bitmap,0,0);
            RenderAxisNumber(graphics,ClientRectangle);
            buf.Render();
        }
        private void RenderImpFuncs(Graphics rt,Rectangle r)
        {
            foreach (ImplicitFunction func in ImpFuncs)
            {
                RenderImpFunc(rt, func, r);
            }
        }
        private void RenderImpFunc(Graphics rt, ImplicitFunction f, Rectangle targetrect)
        {
            double ratio = Math.Pow(2, _Quality);
            ConcurrentBag<Rectangle> RectToCalc = new ConcurrentBag<Rectangle>() { CreateRectByBound((int)(ratio * targetrect.Left), (int)(ratio * targetrect.Top), (int)(ratio * targetrect.Right), (int)(ratio * targetrect.Bottom)) };
            ConcurrentBag<RectangleF> RectToRender = new ConcurrentBag<RectangleF>();
            SolidBrush brush = new SolidBrush(f.color);
            Func<int, int, int, int, bool> func;
            switch (f.Type)
            {
                case ImplicitFunction.ExpressionType.Equal: func = IsAllGeOrLeThanZero; break;
                case ImplicitFunction.ExpressionType.Less: func = IsAllLeThanZero; break;
                default: func = IsAllGeThanZero; break;
            }
            do
            {
                Rectangle[] rs = RectToCalc.ToArray();
                RectToCalc = new ConcurrentBag<Rectangle>();
                Action<int> atn = (idx) => RenderRect(rt, f, rs[idx], RectToCalc, RectToRender, brush, func,ratio);
                for (int i = 0; i < rs.Length; i += 100)
                {
                    RectToRender = new ConcurrentBag<RectangleF>();
                    Parallel.For(i, Math.Min(i + 100, rs.Length), atn);
                    if(RectToRender.Count!=0)
                        rt.FillRectangles(brush,RectToRender.ToArray());
                    RectToRender = null;
                }
            } while (RectToCalc.Count != 0);
        }
        private void RenderRect(Graphics rt, ImplicitFunction f, Rectangle r, ConcurrentBag<Rectangle> RectToCalc, ConcurrentBag<RectangleF> RectToRender, SolidBrush brush, Func<int, int, int, int, bool> func, double ratio)
        {
            if (r.Height == 0 || r.Width == 0)
                return;
            int xtimes = 2, ytimes = 2;
            if (r.Width > r.Height)
                ytimes = 1;
            else if (r.Width < r.Height)
                xtimes = 1;
            int dx = (int)Math.Ceiling(((double)r.Width) / xtimes);
            int dy = (int)Math.Ceiling(((double)r.Height) / ytimes);
            NumberFunctionResultCompared nf = f.NumberFunction;
            for (int i = r.Left; i < r.Right; i += dx)
            {
                double di = i;
                Interval xi = new Interval(PixelToMathX(i / ratio), PixelToMathX((i + dx) / ratio));
                for (int j = r.Top; j < r.Bottom; j += dy)
                {
                    double dj = j;
                    Interval yi = new Interval(PixelToMathY(j / ratio), PixelToMathY((j + dy) / ratio));
                    (bool first, bool second) result = f.ImpFunction.Invoke(xi, yi);

                    if (result == (true, true))
                    {
                        if (func(
                            nf.Invoke(xi.Min, yi.Min),
                            nf.Invoke(xi.Min, yi.Max),
                            nf.Invoke(xi.Max, yi.Max),
                            nf.Invoke(xi.Max, yi.Min)
                        ))
                            RectToRender.Add(CreateRectFByBound((float)(di / ratio), (float)(dj / ratio), (float)Math.Min((di + dx) / ratio, r.Right), (float)Math.Min((dj + dy) / ratio, r.Bottom)));
                    }
                    else if (result == (false, true))
                    {
                        if (dx <= 1 && dx <= 1)
                        {
                            if (func(
                                nf.Invoke(xi.Min, yi.Min),
                                nf.Invoke(xi.Min, yi.Max),
                                nf.Invoke(xi.Max, yi.Max),
                                nf.Invoke(xi.Max, yi.Min)
                            ))
                                RectToRender.Add(CreateRectFByBound((float)(di / ratio), (float)(dj / ratio), (float)Math.Min((di + dx) / ratio, r.Right), (float)Math.Min((dj + dy) / ratio, r.Bottom)));
                        }
                        else
                        {
                            RectToCalc.Add(CreateRectByBound(i, j, Math.Min(i + dx, r.Right), Math.Min(j + dy, r.Bottom)));
                        }
                    }
                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((!loaded) || (!Visible))
                return;
            if (MouseDownLeft)
            {//移动零点
                _Zero.X = (MouseDownZeroPos.X + e.X - MouseDownPos.X);
                _Zero.Y = (MouseDownZeroPos.Y + e.Y - MouseDownPos.Y);
                int width = ClientSize.Width;
                int height = ClientSize.Height;
                Graphics graphics = buf.Graphics;
                graphics.Clear(Color_White);
                RenderAxisLine(graphics,ClientRectangle);
                if (Math.Abs(LastZeroPos.X - _Zero.X) > 20 || Math.Abs(LastZeroPos.Y - _Zero.Y) > 20 || e.Clicks == 142857)
                {
                    //需修改图片
                    Bitmap bmp = new Bitmap(Bitmap);
                    Graphics ImageGraphics = Graphics.FromImage(Bitmap);
                    ImageGraphics.Clear(Color_A);
                    ImageGraphics.DrawImage(bmp, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                    RenderMovedPlace(ImageGraphics);
                    bmp.Dispose();
                    graphics.DrawImage(Bitmap,0,0);
                    LastZeroPos = _Zero;
                }
                else
                {
                    graphics.DrawImage(Bitmap, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                    RenderMovedPlace(graphics);
                }
                RenderAxisNumber(graphics,ClientRectangle);
                buf.Render();
            }
            return;

        }
        private void RenderMovedPlace(Graphics drawtogf)
        {
            foreach (var impFunc in ImpFuncs)
            {
                if (_Zero.X < LastZeroPos.X)
                {
                    RenderImpFunc(drawtogf, impFunc, CreateRectByBound((int)(width - LastZeroPos.X + _Zero.X), 0, width, height));
                    if (_Zero.Y < LastZeroPos.Y)
                    {
                        RenderImpFunc(drawtogf, impFunc, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), (int)(width - LastZeroPos.X + _Zero.X), height));

                    }
                    else if (_Zero.Y > LastZeroPos.Y)
                    {
                        RenderImpFunc(drawtogf, impFunc, CreateRectByBound(0, 0, (int)(width - LastZeroPos.X + _Zero.X), (int)(_Zero.Y - LastZeroPos.Y)));
                    }
                }
                else if (_Zero.X > LastZeroPos.X)
                {
                    RenderImpFunc(drawtogf, impFunc, CreateRectByBound(0, 0, (int)(_Zero.X - LastZeroPos.X), height));
                    if (_Zero.Y < LastZeroPos.Y)
                    {
                        RenderImpFunc(drawtogf, impFunc, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + _Zero.Y), width, height));

                    }
                    else if (_Zero.Y > LastZeroPos.Y)
                    {
                        RenderImpFunc(drawtogf, impFunc, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
                    }
                }
                else
                {
                    if (_Zero.Y < LastZeroPos.Y)
                    {
                        RenderImpFunc(drawtogf, impFunc, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), width, height));

                    }
                    else if (_Zero.Y > LastZeroPos.Y)
                    {
                        RenderImpFunc(drawtogf, impFunc, CreateRectByBound(0, 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
                    }
                }
            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if ((!loaded) || (!Visible))
                return;
            Bitmap=new Bitmap(Width,Height);
        }

        private bool Wheeling = false;
        private DateTime WheelingTime = DateTime.Now;
        private double priviousunitlength;
        private PointL priviouszero;
        private readonly Timer WheelingTimer = new Timer() { Interval = 30 };
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            WheelingTime = DateTime.Now;
            if (!Wheeling)
            {
                Wheeling = true;
                priviousunitlength = _UnitLength;
                priviouszero = _Zero;
            }
            double cursor_x = e.X, cursor_y = e.Y;
            double times_x = (_Zero.X - cursor_x) / _UnitLength;
            double times_y = (_Zero.Y - cursor_y) / _UnitLength;
            double delta;
            delta = Math.Pow(Math.Log(Math.Abs(e.Delta) + 1) + 1, 0.1);
            if (e.Delta == 120)
                delta = 1.3;
            if (e.Delta == -120)
                delta = 1.3;
            if (e.Delta > 0)
            {
                _UnitLength *= delta;
            }
            else
            {
                _UnitLength /= delta;
            }
            _UnitLength = Range(0.01, 1000000, _UnitLength);
            _Zero = new PointL()
            {
                X = (long)(times_x * _UnitLength + cursor_x),
                Y = (long)(times_y * _UnitLength + cursor_y)
            };
            double ratio = _UnitLength / priviousunitlength;
            if (ratio > 2 || ratio < 0.5)
            {
                priviousunitlength = _UnitLength;
                priviouszero = _Zero;
                Render(TargetGraphics);
            }
            else
            {
                Graphics graphics= buf.Graphics;
                graphics.Clear(Color_White);
                RenderAxisLine(graphics,ClientRectangle);
                graphics.DrawImage(Bitmap,new RectangleF(
                    (float)(_Zero.X - priviouszero.X * ratio),
                    (float)(_Zero.Y - priviouszero.Y * ratio),
                    (float)(ratio*Bitmap.Width),
                    (float)(ratio*Bitmap.Height)
                ));
                RenderAxisNumber(graphics,ClientRectangle);
                buf.Render();
            }
        }
    }
    public static class ExtendedMethods
    {
        public static Rectangle CreateRectByBound(int left, int top, int right, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }
        public static RectangleF CreateRectFByBound(float left, float top, float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }
        public unsafe static double GetNextFloatNum(double num)
        {
            long a = (*(long*)(&num) + 1);
            return *(double*)(&a);
        }
        public unsafe static float GetNextFloatNum(float num)
        {
            int a = (*(int*)(&num) + 1);
            return *(float*)(&a);
        }
        public unsafe static double GetPreviousFloatNum(double num)
        {
            long a = (*(long*)(&num) - 1);
            return *(double*)(&a);
        }
        public unsafe static float GetPreviousFloatNum(float num)
        {
            int a = (*(int*)(&num) - 1);
            return *(float*)(&a);
        }
        internal static bool IsAllGeOrLeThanZero(int n1, int n2, int n3, int n4)
        {
            int a = n1 + n2 + n3 + n4;
            return !(a % 10==0 || a < 5);
        }
        internal static bool IsAllGeThanZero(int n1, int n2, int n3, int n4)
        {
            return !(n1 + n2 + n3 + n4<5);
        }
        internal static bool IsAllLeThanZero(int n1, int n2, int n3, int n4)
        {
            return (n1 + n2 + n3 + n4)%10!=0;
        }
        internal static bool Ret(int n1, int n2, int n3, int n4)
        {
            return true;
        }
    }
}
