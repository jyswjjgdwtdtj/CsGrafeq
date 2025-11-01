using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq;
using static CsGrafeq.ExMethods;

namespace CsGrafeq
{
    [Obsolete("", true)]
    public delegate (bool, bool) IntervalImpFunctionDelegate(Interval i1, Interval i2, double[] constlist);
    public delegate (bool, bool) IntervalSetImpFunctionDelegate(IntervalSet i1, IntervalSet i2, double[] constlist);
    public delegate int NumberImpFunctionDelegate(double n1, double n2, double[] constlist);
    public delegate bool MarchingSquaresDelegate(double left,double top,double right,double bottom,double[] constlist);

    public abstract class Function : IDisposable
    {
        private static readonly Array colors = Enum.GetValues(typeof(KnownColor));
        private static readonly Random rnd = new Random();
        private static readonly double k = Math.Pow(1 / (1 + Math.Pow(1.5, 2.2) + Math.Pow(0.6, 2.2)), 1 / 2.2);

        private static double GetRealBrightness(Color c)
        {
            return (k * Math.Pow(Math.Pow(((double)c.R / 255), 2.2) + Math.Pow(((double)c.G / 255 * 1.5), 2.2) + Math.Pow(((double)c.B / 255 * 0.6), 2.2), 1 / 2.2));
        }
        protected static Color GetRandomColor()
        {
            Color color;
            do
            {
                color = Color.FromKnownColor((KnownColor)colors.GetValue(rnd.Next(colors.Length - 27) + 27));
            } while (GetRealBrightness(color) > 0.7);
            return color;
        }
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
        internal string _Expression;
        internal Color color;
        internal string ExpressionRecord;
        internal Function()
        {
            color = GetRandomColor();
        }
    } 
    public class ImplicitFunction : Function
    {
        [Obsolete("", true)]
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
        /// <summary>
        /// 是否使用MarchingSquares检查图像
        /// </summary>
        public CheckPixelMode CheckPixelMode = CheckPixelMode.None;
        /// <summary>
        /// 表达式
        /// </summary>
        public string Expression
        {
            get => _Expression;
        }
        internal ImplicitFunction(string expression,Size s):this(ExpressionCompiler.ParseTokens(ExpressionCompiler.GetTokens(expression)),s)
        {
        }
        internal ImplicitFunction(ComparedExpression ec,Size s):this(ec.Elements.ToArray(),s)
        {
        }
        internal ImplicitFunction(Element[] eles,Size s):base()
        {
            _Expression = string.Empty;
            (IntervalSetImpFunction, MarchingSquaresFunction, UsedConstant) = ExpressionCompiler.Compile(eles);
            _Bitmap = new Bitmap(s.Width,s.Height);
            BitmapGraphics= Graphics.FromImage(_Bitmap);
            ExpressionRecord = ExpressionCompiler.Record;
        }
        public void Dispose()
        {
            _Bitmap.Dispose();
        }
    }
    public enum CheckPixelMode
    {
        UseMarchingSquares, None
    }
    internal static partial class ExMethods
    {
        internal static bool MyContains<T>(this IEnumerable<T> iter, Func<T, bool> predicate)
        {
            foreach(var i in iter)
            {
                if(predicate.Invoke(i))
                    return true;
            }
            return false;
        }
        internal static T SetProperty<T>(this T obj, string propname, object value)
        {
            Type t = typeof(T);
            FieldInfo f = t.GetField(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (f != null)
            {
                f.SetValue(obj, value);
                return obj;
            }
            PropertyInfo p = t.GetProperty(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (p != null)
            {
                p.SetValue(obj, value);
                return obj;
            }
            return obj;
        }
        internal static object GetProperty<T>(this T obj, string propname)
        {
            Type t = typeof(T);
            FieldInfo f = t.GetField(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (f != null)
            {
                return f.GetValue(obj);
            }
            PropertyInfo p = t.GetProperty(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (p != null)
            {
                return p.GetValue(obj);
            }
            return null;
        }
        internal static T GetProperty<T>(this T obj, string propname,ref object value)
        {
            Type t = typeof(T);
            FieldInfo f = t.GetField(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (f != null)
            {
                value=f.GetValue(obj);
                return obj;
            }
            PropertyInfo p = t.GetProperty(propname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (p != null)
            {
                value=p.GetValue(obj);
                return obj;
            }
            return obj;
        }
    }
}
