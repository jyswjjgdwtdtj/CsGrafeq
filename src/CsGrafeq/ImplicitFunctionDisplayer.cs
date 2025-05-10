
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.ExMethods;
namespace CsGrafeq
{
    public class ImplicitFunctionDisplayer : AxisDisplayer
    {
        public readonly CsGrafeqImplicitFunctionCollection Functions;
        private const int AtoZ = 'z' - 'a' + 1;
        private double[] _ConstantsValue = new double[AtoZ] ;

        /// <summary>
        /// a-z常量值
        /// </summary> 
        public double[] ConstantsValue
        {
            get {
                double[] c = new double[AtoZ];
                Array.Copy(_ConstantsValue,c,AtoZ);
                return c; 
            }
            set {
                if (value.Length != AtoZ)
                    throw new ArgumentException(nameof(value));
                Array.Copy(value,_ConstantsValue,AtoZ);
                
            }
        }
        private string[] AcceptedConstant = new string[] {
            "a",
            "b",
            "c",
            "d",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "s",
            "u",
            "v",
            "w",
            "z"};
        public void SetConstantValue(int index,double num)
        {
            if(!AcceptedConstant.Contains(((char)(index+'a')).ToString()))
                throw new ArgumentException(nameof(index));
            _ConstantsValue[index] = num;
            Graphics ImageGraphics = Graphics.FromImage(Bitmap);
            ImageGraphics.Clear(Color_A);
            foreach(var i in Functions.innerList)
            {
                if (i.UsedConstant[index])
                {
                    i.BitmapGraphics.Clear(Color_A);
                    RenderFunction(i.BitmapGraphics, i, ClientRectangle);
                }
                ImageGraphics.DrawImage(i._Bitmap,0,0);
            }
            buf.Graphics.Clear(Color_White);
            RenderAxisLine(buf.Graphics,ClientRectangle);
            buf.Graphics.DrawImage(Bitmap,0,0);
            RenderAxisNumber(buf.Graphics, ClientRectangle);
            buf.Render(TargetGraphics);
        }
        public double GetConstantValue(int index)
        {
            return _ConstantsValue[index];
        }

        private int _Quality = 0;
        /// <summary>
        /// 当移动坐标系时是否重新渲染全部的函数图像
        /// </summary>
        public MovingRenderMode MovingRenderMode { get; set; }
        /// <summary>
        /// 函数绘制精度 不可使用
        /// </summary>
        /// <value>只能为0-4的整数</value>
        public int Quality
        {
            get
            {
                //throw new NotImplementedException();
                return _Quality;
            }
            set
            {
                throw new NotImplementedException();
                _Quality = (int)RangeIn(0, 4, value);
                Render(TargetGraphics);
            }
        }
        private Bitmap Bitmap;
        public ImplicitFunctionDisplayer() : base()
        {
            Functions = new CsGrafeqImplicitFunctionCollection(this);
            MovingRenderMode = MovingRenderMode.RenderEdge;
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
            DBRender.Add(2, (Graphics graphics) =>
            {
                Graphics bmpgraphics = Graphics.FromImage(Bitmap);
                bmpgraphics.Clear(Color_A);
                RenderFunctions(bmpgraphics, ClientRectangle);
                graphics.DrawImage(Bitmap, 0, 0);
            });
            DBRenderTo.Add(2, ((Graphics, Rectangle) st) =>
            {
                foreach (ImplicitFunction func in Functions.innerList)
                {
                    RenderFunction(st.Item1, func, st.Item2);
                }
            });
#if DEBUG
            Functions.Add(
                "0=tan((x))+tan((y))"
            ).CheckPixelMode=CheckPixelMode.UseMarchingSquares;
#endif
        }
        /// <summary>
        /// 绘制函数
        /// </summary>
        /// <param name="rt">指定的绘制对象</param>
        /// <param name="r">绘制区域</param>
        private void RenderFunctions(Graphics rt,Rectangle r)
        {
            foreach (ImplicitFunction func in Functions.innerList)
            {
                func.BitmapGraphics.Clear(Color_A);
                RenderFunction(func.BitmapGraphics, func, r);
                rt.DrawImage(func._Bitmap,0,0);
            }
        }

        /// <summary>
        /// 绘制单个函数
        /// </summary>
        /// <param name="rt">指定的绘制对象</param>
        /// <param name="f">要绘制的函数</param>
        /// <param name="targetrect">绘制区域</param>
        private void RenderFunction(Graphics rt, ImplicitFunction f, Rectangle targetrect)
        {
            double ratio = Math.Pow(2, _Quality);
            ConcurrentBag<Rectangle> RectToCalc = new ConcurrentBag<Rectangle>() { CreateRectByBound((int)(ratio * targetrect.Left), (int)(ratio * targetrect.Top), (int)(ratio * targetrect.Right), (int)(ratio * targetrect.Bottom)) };
            ConcurrentBag<RectangleF> RectToRender = new ConcurrentBag<RectangleF>();
            SolidBrush brush = new SolidBrush(f.color);
            Func<int, int, int, int, bool> func=null;
            bool checkpixel = f.CheckPixelMode != CheckPixelMode.None;
            do
            {
                Rectangle[] rs = RectToCalc.ToArray();
                RectToCalc = new ConcurrentBag<Rectangle>();
                Action<int> atn;
                if(f.DrawingMode == DrawingMode.Interval)
                    atn=(idx) => RenderRectInterval(rt, f, rs[idx], RectToCalc, RectToRender, brush, func,checkpixel,ratio);
                else
                    atn= (idx) => RenderRectIntervalSet(rt, f, rs[idx], RectToCalc, RectToRender, brush, func,checkpixel, ratio);
                for (int i = 0; i < rs.Length; i += 100)
                {
                    RectToRender = new ConcurrentBag<RectangleF>();
                    Parallel.For(i, Math.Min(i + 100, rs.Length), atn);
                    //for (int j = i; j < Math.Min(i + 100, rs.Length); j++) atn(j);
                    if (RectToRender.Count!=0)
                        rt.FillRectangles(brush,RectToRender.ToArray());
                    RectToRender = null;
                }
            } while (RectToCalc.Count != 0);
        }
        /// <summary>
        /// 绘制单个函数
        /// </summary>
        /// <param name="rt">指定的绘制对象</param>
        /// <param name="f">要绘制的函数</param>
        /// <param name="r">绘制区域</param>
        /// <param name="RectToCalc">要继续细化绘制的区域</param>
        /// <param name="RectToRender">要绘制的区域</param>
        private void RenderRectInterval(Graphics rt, ImplicitFunction f, Rectangle r, ConcurrentBag<Rectangle> RectToCalc, ConcurrentBag<RectangleF> RectToRender, SolidBrush brush, Func<int, int, int, int, bool> func,bool checkpixel, double ratio)
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
            double Xexhaust = 0.25d / UnitLengthX;
            double Yexhaust = 0.25d / UnitLengthY;
            MarchingSquaresDelegate msd = f.MarchingSquaresFunction;
            for (int i = r.Left; i < r.Right; i += dx)
            {
                double di = i;
                double xmin = PixelToMathX(i / ratio);
                double xmax = PixelToMathX((i + dx) / ratio);
                Interval xi = new Interval(xmin,xmax);
                for (int j = r.Top; j < r.Bottom; j += dy)
                {
                    double dj = j;
                    double ymin = PixelToMathY(j / ratio);
                    double ymax = PixelToMathY((j + dy) / ratio);
                    Interval yi = new Interval(ymin,ymax);
                    (bool first, bool second) result = f.IntervalImpFunction.Invoke(xi, yi,ConstantsValue);
                    if (result == TT)
                    {
                        RectToRender.Add(CreateRectFByBound((float)(di / ratio), (float)(dj / ratio), (float)Math.Min((di + dx) / ratio, r.Right), (float)Math.Min((dj + dy) / ratio, r.Bottom)));
                    }
                    else if (result == FT)
                    {
                        if (dx == 1 && dx == 1)
                        {
                            if ((!checkpixel)||(checkpixel&& CheckCurveExists(msd, xmin, ymin, xmax, ymax, Xexhaust, Yexhaust, ConstantsValue)))
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
        /// <summary>
        /// 绘制单个函数
        /// </summary>
        /// <param name="rt">指定的绘制对象</param>
        /// <param name="f">要绘制的函数</param>
        /// <param name="r">绘制区域</param>
        /// <param name="RectToCalc">要继续细化绘制的区域</param>
        /// <param name="RectToRender">要绘制的区域</param>
        private void RenderRectIntervalSet(Graphics rt, ImplicitFunction f, Rectangle r, ConcurrentBag<Rectangle> RectToCalc, ConcurrentBag<RectangleF> RectToRender, SolidBrush brush, Func<int, int, int, int, bool> func, bool checkpixel, double ratio)
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
            double Xexhaust=0.25d/UnitLengthX;
            double Yexhaust=0.25d/UnitLengthY;
            MarchingSquaresDelegate msd = f.MarchingSquaresFunction;
            for (int i = r.Left; i < r.Right; i += dx)
            {
                double di = i;
                double xmin = PixelToMathX(i / ratio);
                double xmax = PixelToMathX((i + dx) / ratio);
                IntervalSet xi = new IntervalSet(xmin,xmax);
                for (int j = r.Top; j < r.Bottom; j += dy)
                {
                    double dj = j;
                    double ymin = PixelToMathY(j / ratio);
                    double ymax = PixelToMathY((j + dy) / ratio);
                    IntervalSet yi = new IntervalSet(ymin,ymax);
                    (bool first, bool second) result = f.IntervalSetImpFunction.Invoke(xi, yi, ConstantsValue);
                    if (result == TT)
                    {
                        RectToRender.Add(CreateRectFByBound((float)(di / ratio), (float)(dj / ratio), (float)Math.Min((di + dx) / ratio, r.Right), (float)Math.Min((dj + dy) / ratio, r.Bottom)));
                    }
                    else if (result == FT)
                    {
                        if (dx == 1 && dx == 1)
                        {
                            if ((!checkpixel) || (checkpixel && CheckCurveExists(msd, xmin, ymin, xmax, ymax, Xexhaust,Yexhaust, ConstantsValue)))
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
        internal bool CheckCurveExists(MarchingSquaresDelegate msd,double xmin,double ymin,double xmax,double ymax,double Xexhaust,double Yexhaust, double[] consts)
        {
            if (msd.Invoke(xmin, ymin, xmax, ymax, consts))
                return true;
            if (xmax-xmin<Xexhaust&&ymax-ymin<Yexhaust)
                return false;
            if (ymax - ymin > xmax - xmin)
            {
                double half = (ymax + ymin) / 2;
                return
                    CheckCurveExists(msd, xmin, ymin, xmax, half, Xexhaust, Yexhaust, consts) ||
                    CheckCurveExists(msd, xmin, half, xmax, ymax, Xexhaust, Yexhaust, consts);
            }
            else
            {
                double half = (xmax + xmin) / 2;
                return
                    CheckCurveExists(msd, xmin, ymin, half, ymax, Xexhaust, Yexhaust, consts) ||
                    CheckCurveExists(msd, half, ymin, xmax, ymax, Xexhaust, Yexhaust, consts);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((!loaded) || (!Visible))
                return;
            bool l = MouseOnYAxis, ll = MouseOnXAxis;
            MouseOnYAxis = Math.Abs(e.X - _Zero.X) < 3;
            MouseOnXAxis = Math.Abs(e.Y - _Zero.Y) < 3;
            if (l != MouseOnYAxis || ll != MouseOnXAxis)
            {
                Graphics gg = buf.Graphics;
                gg.Clear(Color_White);
                RenderAxisLine(gg,ClientRectangle);
                gg.DrawImage(Bitmap,0,0);
                RenderAxisNumber(gg,ClientRectangle);
                CallDBRender(gg);
                buf.Render();
            }
            if (MouseDownLeft&&CanMove)
            {//移动零点
                _Zero.X = (MouseDownZeroPos.X + e.X - MouseDownPos.X);
                _Zero.Y = (MouseDownZeroPos.Y + e.Y - MouseDownPos.Y);
                if (MovingRenderMode == MovingRenderMode.RenderAll)
                {
                    Render(TargetGraphics);
                    return;
                }
                int width = ClientSize.Width;
                int height = ClientSize.Height;
                Graphics graphics = buf.Graphics;
                graphics.Clear(Color_White);
                RenderAxisLine(graphics,ClientRectangle);
                if (Math.Abs(LastZeroPos.X - _Zero.X) > 20 || Math.Abs(LastZeroPos.Y - _Zero.Y) > 20 || e.Clicks == 142857 )
                {
                    //需修改图片
                    Bitmap bmp;
                    Graphics imageGraphics= Graphics.FromImage(Bitmap);
                    imageGraphics.Clear(Color_A);
                    foreach(var i in Functions.innerList)
                    {
                        bmp = new Bitmap(i._Bitmap);
                        i.BitmapGraphics.Clear(Color_A);
                        i.BitmapGraphics.DrawImage(bmp, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                        bmp.Dispose();
                        RenderMovedPlace(i.BitmapGraphics,i);
                        imageGraphics.DrawImage(i._Bitmap,0,0);
                    }
                    graphics.DrawImage(Bitmap,0,0);
                    LastZeroPos = _Zero;
                }
                else
                {
                    graphics.DrawImage(Bitmap, _Zero.X - LastZeroPos.X, _Zero.Y - LastZeroPos.Y);
                    RenderMovedPlace(graphics);
                }
                RenderAxisNumber(graphics,ClientRectangle);
                //CallDBRender(graphics);
                buf.Render();
            }
            return;

        }
        private void RenderMovedPlace(Graphics graphics)
        {
            foreach(var impfunc in Functions.innerList)
            {
                RenderMovedPlace(graphics,impfunc);
            }
        }
        private void RenderMovedPlace(Graphics drawtogf,ImplicitFunction impFunc)
        {
            if (_Zero.X < LastZeroPos.X)
            {
                RenderFunction(drawtogf, impFunc, CreateRectByBound((int)(width - LastZeroPos.X + _Zero.X), 0, width, height));
                if (_Zero.Y < LastZeroPos.Y)
                {
                    RenderFunction(drawtogf, impFunc, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), (int)(width - LastZeroPos.X + _Zero.X), height));

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    RenderFunction(drawtogf, impFunc, CreateRectByBound(0, 0, (int)(width - LastZeroPos.X + _Zero.X), (int)(_Zero.Y - LastZeroPos.Y)));
                }
            }
            else if (_Zero.X > LastZeroPos.X)
            {
                RenderFunction(drawtogf, impFunc, CreateRectByBound(0, 0, (int)(_Zero.X - LastZeroPos.X), height));
                if (_Zero.Y < LastZeroPos.Y)
                {
                    RenderFunction(drawtogf, impFunc, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), (int)(height - LastZeroPos.Y + _Zero.Y), width, height));

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    RenderFunction(drawtogf, impFunc, CreateRectByBound((int)(_Zero.X - LastZeroPos.X), 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
                }
            }
            else
            {
                if (_Zero.Y < LastZeroPos.Y)
                {
                    RenderFunction(drawtogf, impFunc, CreateRectByBound(0, (int)(height - LastZeroPos.Y + _Zero.Y), width, height));

                }
                else if (_Zero.Y > LastZeroPos.Y)
                {
                    RenderFunction(drawtogf, impFunc, CreateRectByBound(0, 0, width, (int)(_Zero.Y - LastZeroPos.Y)));
                }
            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if ((!loaded) || (!Visible))
                return;
            if (width != 0 && height != 0)
            {
                foreach (var i in Functions.innerList)
                {
                    i._Bitmap.Dispose();
                    i.Bitmap = new Bitmap(width, height);
                }
                Bitmap.Dispose();
                Bitmap = new Bitmap(width, height);
            }
        }

        private bool Wheeling = false;
        private DateTime WheelingTime = DateTime.Now;
        private double previousunitlengthX,previousunitlengthY;
        private PointL previouszero;
        private readonly Timer WheelingTimer = new Timer() { Interval = 30 };
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!CanZoom)
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
            if (CanMove)
            {
                _Zero = new PointL()
                {
                    X = (long)(times_x * _UnitLengthX + cursor_x),
                    Y = (long)(times_y * _UnitLengthY + cursor_y)
                };
            }
            double ratioX = _UnitLengthX / previousunitlengthX;
            double ratioY = _UnitLengthY / previousunitlengthY;
            if (ratioX > 2 || ratioX < 0.5|| ratioY > 2 || ratioY < 0.5)
            {
                previousunitlengthX = _UnitLengthX;
                previousunitlengthY = _UnitLengthY;
                previouszero = _Zero;
                Render(TargetGraphics);
            }
            else
            {
                Graphics graphics= buf.Graphics;
                graphics.Clear(Color_White);
                RenderAxisLine(graphics,ClientRectangle);
                graphics.DrawImage(Bitmap,new RectangleF(
                    (float)(_Zero.X - previouszero.X * ratioX),
                    (float)(_Zero.Y - previouszero.Y * ratioY),
                    (float)(ratioX*Bitmap.Width),
                    (float)(ratioY*Bitmap.Height)
                ));
                RenderAxisNumber(graphics,ClientRectangle);
                buf.Render();
            }
        }
    }
    public enum MovingRenderMode
    {
        RenderAll,
        RenderEdge
    }
    internal static partial class ExMethods
    {
        public static Rectangle CreateRectByBound(int left, int top, int right, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }
        public static RectangleF CreateRectFByBound(float left, float top, float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }
        /*
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
        }*/
        //大于0则为10 等于0为0 小于0为1
        
    }
    public class CsGrafeqImplicitFunctionCollection 
    {
        internal List<ImplicitFunction> innerList=new List<ImplicitFunction>();
        internal Control owner;
        internal CsGrafeqImplicitFunctionCollection(Control fd)
        {
            this.owner = fd;
        }
        public ImplicitFunction Add(ComparedExpression ec)
        {
            ImplicitFunction impf =new ImplicitFunction(ec,owner.ClientSize);
            innerList.Add(impf);
            return impf;
        }
        public ImplicitFunction Add(string expression)
        {
            ImplicitFunction impf = new ImplicitFunction(expression, owner.ClientSize);
            innerList.Add(impf);
            return impf;
        }
        public bool RemoveAt(int index)
        {
            if (index >= innerList.Count && index < 0)
                return false;
            innerList[index].Dispose();
            innerList.RemoveAt(index);
            return true;
        }
        public ImplicitFunction this[int index]
        {
            get
            {
                return innerList[index];
            }
        }
        public int Count
        {
            get
            {
                return innerList.Count;
            }
        }
    }
}
