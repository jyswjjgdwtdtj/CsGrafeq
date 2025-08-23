using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace CsGrafeq
{
    public partial class AxisDisplayer :DoubleBufferedControl
    {
        public bool ShowAxis = true;
        public bool ShowNumber = true;
        public bool CanMove = true;
        public bool CanZoom = true;

        protected double _UnitLengthX = 20.0001d;
        protected double _UnitLengthY = 20.0001d;
        public double UnitLengthX
        {
            get { return _UnitLengthX; }
            set { _UnitLengthX = value; Render(TargetGraphics); }
        }
        public double UnitLengthY
        {
            get { return _UnitLengthY; }
            set { _UnitLengthY = value; Render(TargetGraphics); }
        }
        protected PointL _Zero = new PointL() { X=250,Y=250};
        public PointL Zero
        {
            get { return _Zero; }
            set { _Zero = value; Render(TargetGraphics); }
        }
        public Point ZeroInt
        {
            get { return new Point((int)_Zero.X, (int)_Zero.Y); }
            set { _Zero =new PointL(value.X,value.Y); Render(TargetGraphics); }
        }
        public AxisDisplayer() : base()
        {
            Size = new Size(500, 500);
            DBRender.Add(1, (Graphics graphics) =>
            {
                RenderAxisLine(graphics, ClientRectangle);
            });
            DBRender.Add(3, (Graphics graphics) =>
            {
                RenderAxisNumber(graphics, ClientRectangle);
            });
            DBRenderTo.Add(1, ((Graphics,Rectangle) st) =>
            {
                RenderAxisLine(st.Item1, st.Item2);
            });
            DBRenderTo.Add(3, ((Graphics, Rectangle) st) =>
            {
                RenderAxisNumber(st.Item1, st.Item2);
            });
#if DEBUG
            /*DBPrint+=(2,(g) =>
            {
                if (!loaded)
                {
                    ImplicitFunctionDisplayer FD = new ImplicitFunctionDisplayer();
                    this.Controls.Add(FD);
                    FD.Dock = DockStyle.Fill;
                }
            });*/
#endif
        }
        #region Render
        protected void RenderAxisLine(Graphics gb,Rectangle rect)
        {
            if (!loaded)
                return;
            if (!ShowAxis)
                return;
            Pen b0, b1, b2;
            b0 = new Pen(Color.FromArgb(190, 190, 190));
            b1 = new Pen(Color.FromArgb(128, 128, 128));
            Pen pb =Pen_Black;
            Pen bluepen = new Pen(Color.Blue,3);
            float width=rect.Width;
            float height=rect.Height;
            //y
            if (RangeIn(0, width, _Zero.X) == _Zero.X)
            {
                gb.DrawLine(
                    MouseOnYAxis?bluepen:pb,
                    new PointF(_Zero.X, 0f),
                    new PointF(_Zero.X, height)
                );
            }
            pb = Pen_Black;
            if (RangeIn(0, height, _Zero.Y) == _Zero.Y)
            {
                gb.DrawLine(
                    MouseOnXAxis ? bluepen : pb,
                    new PointF(0f, _Zero.Y),
                    new PointF(width, _Zero.Y)
                );
            }
            int zsX = (int)Math.Floor(Math.Log((350 / _UnitLengthX), 10));
            int zsY = (int)Math.Floor(Math.Log((350 / _UnitLengthY), 10));
            double addnumX = Math.Pow(10, zsX);
            double addnumY = Math.Pow(10, zsY);
            decimal addnumDX = (decimal)Math.Pow(10, zsX);
            decimal addnumDY = (decimal)Math.Pow(10, zsY);
            double p = RangeIn(rect.Top - 3, rect.Bottom - Font.Height + 1, _Zero.Y);
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
        protected void RenderAxisNumber(Graphics gb, Rectangle rect)
        {
            if (!loaded)
                return;
            if (!ShowNumber)
                return;
            float width = rect.Width;
            float height = rect.Height;
            int zsX = (int)Math.Floor(Math.Log((350 / _UnitLengthX), 10));
            int zsY = (int)Math.Floor(Math.Log((350 / _UnitLengthY), 10));
            double addnumX = Math.Pow(10, zsX);
            double addnumY = Math.Pow(10, zsY);
            decimal addnumDX = (decimal)Math.Pow(10, zsX);
            decimal addnumDY = (decimal)Math.Pow(10, zsY);
            double p = RangeIn(rect.Top - 3, rect.Bottom - Font.Height + 1, _Zero.Y);
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
                if (rect.Left + 3 > _Zero.X)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)rect.Left + 3, (float)(i - Font.Height / 2 - 2));
                }
                else if (_Zero.X + num.ToString().Length * Font.Height / 2 > rect.Right - 3)
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
                if (rect.Left + 3 > _Zero.X)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)rect.Left + 3, (float)(i - Font.Height / 2 - 2));
                }
                else if (_Zero.X + num.ToString().Length * Font.Height / 2 > rect.Right - 3)
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)width - num.ToString().Length * Font.Height / 2 - 5, (float)(i - Font.Height / 2 - 2));
                }
                else
                {
                    gb.DrawString(num.ToString(), Font, Brush_Black, (float)_Zero.X, (float)(i - Font.Height / 2 - 2));
                }
            }
            gb.DrawString("0", Font, Brush_Black, _Zero.X + 3, _Zero.Y);
        }
        #endregion
        #region MathMethods
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
        #endregion
        #region PointConvertMethods

        protected virtual double MathToPixelX(double d)
        {
            return _Zero.X + d * _UnitLengthX;
        }

        protected virtual double MathToPixelY(double d)
        {
            return _Zero.Y + -d * _UnitLengthY;
        }
        protected virtual double PixelToMathX(double d)
        {
            return ((d - _Zero.X) / _UnitLengthX);
        }
        protected virtual double PixelToMathY(double d)
        {
            return -(d - _Zero.Y) / _UnitLengthY;
        }
        #endregion
        #region OverridedMethods
        protected PointL MouseDownPos = new PointL() { X = 0, Y = 0 };
        protected PointL MouseDownZeroPos = new PointL() { X = 0, Y = 0 };
        protected PointL LastZeroPos;
        protected bool MouseDownLeft = false;
        protected bool MouseDownRight = false;
        protected bool MouseOnXAxis=false;
        protected bool MouseOnYAxis=false;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseDownLeft = true;
                MouseDownPos = new PointL() {X=e.X,Y=e.Y };
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
            bool l=MouseOnYAxis,ll=MouseOnXAxis;
            MouseOnYAxis = Math.Abs(e.X - _Zero.X) < 3;
            MouseOnXAxis = Math.Abs(e.Y - _Zero.Y) < 3;
            if (l != MouseOnYAxis || ll != MouseOnXAxis)
                Render(TargetGraphics);
            if (MouseDownLeft&&CanMove)
            {//移动零点
                _Zero.X = (MouseDownZeroPos.X + e.X - MouseDownPos.X);
                _Zero.Y = (MouseDownZeroPos.Y + e.Y - MouseDownPos.Y);
                Render(TargetGraphics);
            }
            LastZeroPos = _Zero;
            return;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseDownLeft = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                MouseDownRight = false;
            }
            if(LastZeroPos!=_Zero && CanMove)
                Render(TargetGraphics);
            LastZeroPos = _Zero;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            MouseDownLeft = MouseDownRight = false;
            Render(TargetGraphics);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if(!CanZoom)
                return;
            double cursor_x = e.X, cursor_y = e.Y;
            double times_x = (_Zero.X - cursor_x) / _UnitLengthX;
            double times_y = (_Zero.Y - cursor_y) / _UnitLengthY;
            double delta;
            delta = Math.Pow(Math.Log(Math.Abs(e.Delta)+1)+1,0.1);
            if (e.Delta == 120)
                delta = 1.3;
            if (e.Delta == -120)
                delta = 1.3;
            if (e.Delta > 0)
            {
                if (MouseOnXAxis&&MouseOnYAxis)
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
            Render(TargetGraphics);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            OnMouseDown(new MouseEventArgs(MouseButtons.Left, 0, 100, 100, 0));
            switch (e.KeyCode)
            {
                case Keys.Left:
                    OnMouseMove(new MouseEventArgs(MouseButtons.Left, 142857, 110, 100, 0));
                    break;
                case Keys.Right:
                    OnMouseMove(new MouseEventArgs(MouseButtons.Left, 142857, 90, 100, 0));
                    break;
                case Keys.Up:
                    OnMouseMove(new MouseEventArgs(MouseButtons.Left, 142857, 100, 110, 0));
                    break;
                case Keys.Down:
                    OnMouseMove(new MouseEventArgs(MouseButtons.Left, 142857, 100, 90, 0));
                    break;
            }
            MouseDownLeft = false;
            LastZeroPos = _Zero;
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return false;
            }
            return base.ProcessDialogKey(keyData);
        }
        #endregion

    }
    #region ExtendedMethods
    internal static partial class ExMethods
    {
        public static BufferedGraphics GetBuffer(this Graphics graphics, Rectangle r)
        {
            return BufferedGraphicsManager.Current.Allocate(graphics, r);
        }
    }
    
    public struct PointL
    {
        public long X;
        public long Y;
        public PointL(long x,long y)
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
    #endregion
}
