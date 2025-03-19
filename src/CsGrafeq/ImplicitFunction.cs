using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq
{
    internal delegate (bool, bool) IntervalImpFunctionDelegate(Interval i1, Interval i2);
    internal delegate (bool, bool) IntervalSetImpFunctionDelegate(IntervalSet i1, IntervalSet i2);
    internal delegate int NumberImpFunctionDelegate(double n1,double n2);
    public class ImplicitFunction
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
        internal NumberImpFunctionDelegate NumberFunction;
        internal IntervalSetImpFunctionDelegate IntervalSetImpFunction;
        private string _Expression;
        internal Color color;
        internal ExpressionType Type;
        internal string ExpressionRecord;
        /// <summary>
        /// 使用Interval或IntervalSet计算
        /// </summary>
        public DrawingMode Mode=DrawingMode.Interval;
        /// <summary>
        /// 获取指定x区间与y区间的计算结果
        /// </summary>
        public (bool, bool) GetFunctionResult(Interval i1, Interval i2)
        {
            if (IntervalImpFunction == null)
                throw new Exception("函数未生成");
            return IntervalImpFunction(i1, i2);
        }
        /// <summary>
        /// 获取指定x区间与y区间的计算结果
        /// </summary>
        public (bool, bool) GetFunctionResult(IntervalSet i1, IntervalSet i2)
        {
            if (IntervalImpFunction == null)
                throw new Exception("函数未生成");
            return IntervalSetImpFunction(i1, i2);
        }
        /// <summary>
        /// 表达式
        /// </summary>
        public string Expression
        {
            get => _Expression;
        }
        internal ImplicitFunction(string expression)
        {
            _Expression = expression;
            (IntervalImpFunction, IntervalSetImpFunction, NumberFunction) = ExpressionComplier.Complie(expression);
            ExpressionRecord = ExpressionComplier.Record;
            color = GetRandomColor();
            if (!expression.Contains("="))
            {
                color = Color.FromArgb(120, color);
                Type = expression.Contains("<") ? ExpressionType.Less : ExpressionType.Greater;
            }
            else
            {
                Type = ExpressionType.Equal;
                Mode = DrawingMode.IntervalSet;
            }
        }
        internal ImplicitFunction(ExpressionCompared ec)
        {
            _Expression = String.Empty;
            (IntervalImpFunction, IntervalSetImpFunction, NumberFunction) = ExpressionComplier.Complie(ec);
            ExpressionRecord = ExpressionComplier.Record;
            color = GetRandomColor();
            ExpressionComplier.Element ele = ec.Elements[ec.Elements.Count - 1];
            if (ele.NameOrValue!="Equal")
            {
                color = Color.FromArgb(120, color);
                Type = ele.NameOrValue=="Less" ? ExpressionType.Less : ExpressionType.Greater;
            }
            else
            {
                Type = ExpressionType.Equal;
            }
        }

        public enum ExpressionType
        {
            Equal,Less,Greater
        }
        public enum DrawingMode
        {
            Interval,IntervalSet
        }

    }

}
