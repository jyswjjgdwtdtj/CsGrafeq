using System;
using System.Drawing;
using System.Windows.Forms;

namespace CsGrafeq
{
    public partial class AxisDisplayer :UserControl
    {
        protected Graphics TargetGraphics;
        internal int width, height;
        protected bool loaded = false;
        protected BufferedGraphics buf;

        protected double _UnitLength = 20.00000001;
        public double UnitLength
        {
            get { return _UnitLength; }
            set { _UnitLength = value;Render(TargetGraphics); }
        }
        protected PointL _Zero = new PointL() { X=250,Y=250};
        public PointL Zero
        {
            get { return _Zero; }
            set { _Zero = value; Render(TargetGraphics); }
        }
        protected Color Color_White, Color_Black,Color_A;
        protected SolidBrush Brush_White, Brush_Black;
        protected Pen Pen_White, Pen_Black;
        public AxisDisplayer()
        {
            TargetGraphics = CreateGraphics();
            buf = TargetGraphics.GetBuffer(ClientRectangle);
            Size = new Size(500, 500);
            Initialize();
        }
        protected void Initialize()
        {
            Font = new Font("Consolas", 15);
            Color_White = Color.White;
            Color_Black = Color.Black;
            Color_A = Color.FromArgb(0,255,255,255);
            Pen_White = new Pen(Color_White, 1);
            Pen_Black = new Pen(Color_Black, 1);
            Brush_White = new SolidBrush(Color_White);
            Brush_Black = new SolidBrush(Color_Black);
        }
        #region Render
        protected virtual void Render(Graphics rt)
        {
            if (!loaded)
                return;
            Graphics graphics = buf.Graphics;
            graphics.Clear(Color_White);
            RenderAxisLine(graphics, ClientRectangle);
            RenderAxisNumber(graphics,ClientRectangle);
            buf.Render();
        }
        protected void RenderAxisLine(Graphics gb,Rectangle rect)
        {
            if (!loaded)
                return;
            Pen b0, b1, b2;
            b0 = new Pen(Color.FromArgb(190, 190, 190));
            b1 = new Pen(Color.FromArgb(128, 128, 128));
            Pen pb =Pen_Black;
            if (Range(0, width, _Zero.X) == _Zero.X)
            {
                gb.DrawLine(
                    pb,
                    new PointF(_Zero.X, 0f),
                    new PointF(_Zero.X, height)
                );
            }
            pb = Pen_Black;
            if (Range(0, height, _Zero.Y) == _Zero.Y)
            {
                gb.DrawLine(
                    pb,
                    new PointF(0f, _Zero.Y),
                    new PointF(width, _Zero.Y)
                );
            }
            int zsX = (int)Math.Floor(Math.Log((350 / _UnitLength), 10));
            int zsY = (int)Math.Floor(Math.Log((350 / _UnitLength), 10));
            double addnumX = Math.Pow(10, zsX);
            double addnumY = Math.Pow(10, zsY);
            decimal addnumDX = (decimal)Math.Pow(10, zsX);
            decimal addnumDY = (decimal)Math.Pow(10, zsY);
            double p = Range(rect.Top - 3, rect.Bottom - Font.Height + 1, _Zero.Y);
            for (double i = Math.Min(_Zero.X - (addnumX * _UnitLength), MathToPixelX(Round(PixelToMathX(width), -zsX))); i > 0; i -= (addnumX * _UnitLength))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                if (num % (10 * addnumDX) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    (float)i, 0,
                    (float)i, height
                );
            }
            for (double i = Math.Max(_Zero.X + (addnumX * _UnitLength), MathToPixelX(Round(PixelToMathX(0), -zsX))); i < width; i += (addnumX * _UnitLength))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                if (num % (10 * addnumDX) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    (float)i, 0,
                    (float)i, height
                );
            }
            for (double i = Math.Min(_Zero.Y - (addnumY * _UnitLength), MathToPixelY(Round(PixelToMathY(height), -zsY))); i > 0; i -= (addnumY * _UnitLength))
            {
                decimal num = Round((decimal)PixelToMathY(i), -zsY);
                if (num % (10 * addnumDY) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    0, (float)i,
                    width, (float)i
                );
            }
            for (double i = Math.Max(_Zero.Y + (addnumY * _UnitLength), MathToPixelY(Round(PixelToMathY(0), -zsY))); i < height; i += (addnumY * _UnitLength))
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
            int zsX = (int)Math.Floor(Math.Log((350 / _UnitLength), 10));
            int zsY = (int)Math.Floor(Math.Log((350 / _UnitLength), 10));
            double addnumX = Math.Pow(10, zsX);
            double addnumY = Math.Pow(10, zsY);
            decimal addnumDX = (decimal)Math.Pow(10, zsX);
            decimal addnumDY = (decimal)Math.Pow(10, zsY);
            double p = Range(rect.Top - 3, rect.Bottom - Font.Height + 1, _Zero.Y);
            float fff = 1f / 4f * Font.Height;
            for (double i = Math.Min(_Zero.X - (addnumX * _UnitLength), MathToPixelX(Round(PixelToMathX(width), -zsX))); i > 0; i -= (addnumX * _UnitLength))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                gb.DrawString(num.ToString(), Font, Brush_Black, (float)(i - (num.ToString().Length) * fff - 2), (float)p);
            }
            for (double i = Math.Max(_Zero.X + (addnumX * _UnitLength), MathToPixelX(Round(PixelToMathX(0), -zsX))); i < width; i += (addnumX * _UnitLength))
            {
                decimal num = Round((decimal)PixelToMathX(i), -zsX);
                gb.DrawString(num.ToString(), Font, Brush_Black, (float)(i - (num.ToString().Length) * fff - 2), (float)p);
            }
            for (double i = Math.Min(_Zero.Y - (addnumY * _UnitLength), MathToPixelY(Round(PixelToMathY(height), -zsY))); i > 0; i -= (addnumY * _UnitLength))
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
            for (double i = Math.Max(_Zero.Y + (addnumY * _UnitLength), MathToPixelY(Round(PixelToMathY(0), -zsY))); i < height; i += (addnumY * _UnitLength))
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
        public static double Range(double min, double max, double num)
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
            return _Zero.X + d * _UnitLength;
        }

        protected virtual double MathToPixelY(double d)
        {
            return _Zero.Y + -d * _UnitLength;
        }
        protected virtual double PixelToMathX(double d)
        {
            return ((d - _Zero.X) / _UnitLength);
        }
        protected virtual double PixelToMathY(double d)
        {
            return -(d - _Zero.Y) / _UnitLength;
        }
        #endregion
        #region OverridedMethods
        protected PointL MouseDownPos = new PointL() { X = 0, Y = 0 };
        protected PointL MouseDownZeroPos = new PointL() { X = 0, Y = 0 };
        protected PointL LastZeroPos;
        protected bool MouseDownLeft = false;
        protected bool MouseDownRight = false;
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
            if (MouseDownLeft)
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
            if(LastZeroPos!=_Zero)
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
            double cursor_x = e.X, cursor_y = e.Y;
            double times_x = (_Zero.X - cursor_x) / _UnitLength;
            double times_y = (_Zero.Y - cursor_y) / _UnitLength;
            double delta;
            delta = Math.Pow(Math.Log(Math.Abs(e.Delta)+1)+1,0.1);
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
            _Zero = new PointL() {
                X=(long)(times_x * _UnitLength + cursor_x),
                Y=(long)(times_y * _UnitLength + cursor_y)
            };
            Render(TargetGraphics);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            width = ClientSize.Width; height = ClientSize.Height;
            buf = TargetGraphics.GetBuffer(ClientRectangle);
            Render(TargetGraphics);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            width = ClientSize.Width; height = ClientSize.Height;
            /*if (!loaded)
            {
                FunctionDisplayer FD = new FunctionDisplayer();
                this.Controls.Add(FD);
            }*/
            loaded = true;
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
        public static BufferedGraphics GetBuffer(this Graphics graphics,Rectangle r)
        {
            return BufferedGraphicsManager.Current.Allocate(graphics, r);
        }
    }
    
    public struct PointL
    {
        public long X;
        public long Y;
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
