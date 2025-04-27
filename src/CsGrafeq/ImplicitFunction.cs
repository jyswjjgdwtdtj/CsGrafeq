using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq
{
    internal delegate (bool, bool) IntervalImpFunctionDelegate(Interval i1, Interval i2, double[] constlist);
    internal delegate (bool, bool) IntervalSetImpFunctionDelegate(IntervalSet i1, IntervalSet i2, double[] constlist);
    internal delegate int NumberImpFunctionDelegate(double n1, double n2, double[] constlist);
    internal delegate bool MarchingSquaresDelegate(double left,double top,double right,double bottom,double[] constlist);
    public class ImplicitFunction : IDisposable
    {
        private static readonly Array colors = Enum.GetValues(typeof(KnownColor));
        private static readonly Random rnd = new Random();
        private static readonly double k = Math.Pow(1 / (1 + Math.Pow(1.5, 2.2) + Math.Pow(0.6, 2.2)), 1 / 2.2);

        private static double GetRealBrightness(Color c)
        {
            return (k * Math.Pow(Math.Pow(((double)c.R / 255), 2.2) + Math.Pow(((double)c.G / 255 * 1.5), 2.2) + Math.Pow(((double)c.B / 255 * 0.6), 2.2), 1 / 2.2));
        }
        private static Color GetRandomColor()
        {
            Color color;
            do
            {
                color = Color.FromKnownColor((KnownColor)colors.GetValue(rnd.Next(colors.Length - 27) + 27));
            } while (GetRealBrightness(color) > 0.7);
            return color;
        }

        internal IntervalImpFunctionDelegate IntervalImpFunction;
        internal MarchingSquaresDelegate MarchingSquaresFunction;
        internal IntervalSetImpFunctionDelegate IntervalSetImpFunction;
        internal Bitmap _Bitmap;
        internal bool[] UsedConstant = new bool['z' - 'a' + 1];
        internal Bitmap Bitmap
        {
            set
            {
                _Bitmap = value;
                BitmapGraphics = Graphics.FromImage(_Bitmap);
            }
        }
        internal Graphics BitmapGraphics;
        internal RectangleF Rects;
        private string _Expression;
        internal Color color;
        internal string ExpressionRecord;
        /// <summary>
        /// 使用Interval或IntervalSet计算
        /// </summary>
        public DrawingMode DrawingMode = DrawingMode.Interval;
        /// <summary>
        /// 是否使用MarchingSquares检查图像
        /// </summary>
        public CheckPixelMode CheckPixelMode = CheckPixelMode.None;
        /// <summary>
        /// 获取指定x区间与y区间的计算结果
        /// </summary>
        public (bool, bool) GetFunctionResult(Interval i1, Interval i2, double[] constlist)
        {
            if (IntervalImpFunction == null)
                throw new Exception("函数未生成");
            return IntervalImpFunction(i1, i2, constlist);
        }
        /// <summary>
        /// 获取指定x区间与y区间的计算结果
        /// </summary>
        public (bool, bool) GetFunctionResult(IntervalSet i1, IntervalSet i2, double[] constlist)
        {
            if (IntervalSetImpFunction == null)
                throw new Exception("函数未生成");
            return IntervalSetImpFunction(i1, i2, constlist);
        }
        /// <summary>
        /// 表达式
        /// </summary>
        public string Expression
        {
            get => _Expression;
        }
        internal ImplicitFunction(string expression):this(ExpressionCompiler.ParseTokens(ExpressionCompiler.GetTokens(expression)))
        {
        }
        internal ImplicitFunction(ComparedExpression ec):this(ec.Elements.ToArray())
        {
        }
        internal ImplicitFunction(ExpressionCompiler.Element[] eles)
        {
            _Expression = string.Empty;
            (IntervalImpFunction, IntervalSetImpFunction, MarchingSquaresFunction, UsedConstant) = ExpressionCompiler.Compile(eles);
            ExpressionRecord = ExpressionCompiler.Record;
            color = GetRandomColor();
            bool containequal = false;
            foreach (var i in eles)
            {
                if (i.NameOrValue.Contains("Equal"))
                {
                    containequal = true;
                    break;
                }
            }
            if (containequal)
            {
                DrawingMode = DrawingMode.IntervalSet;
            }
            else
            {
                color = Color.FromArgb(120, color);
                DrawingMode = DrawingMode.IntervalSet;
            }
        }
        public void Dispose()
        {
            _Bitmap.Dispose();
        }
        public ImplicitFunction SetProperty(string propname, object value)
        {
            Type t = typeof(AxisDisplayer);
            FieldInfo f = t.GetField(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (f != null)
            {
                f.SetValue(this, value);
                return this;
            }
            PropertyInfo p = t.GetProperty(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (p != null)
            {
                p.SetValue(this, value);
                return this;
            }
            return this;
        }
    }
    public enum ExpressionType
    {
        Equal, Less, Greater
    }
    public enum DrawingMode
    {
        Interval, IntervalSet
    }
    public enum CheckPixelMode
    {
        UseMarchingSquares, None
    }
    public struct RectangleD
    {
        public double X,Y,Width,Height;
    }
}
