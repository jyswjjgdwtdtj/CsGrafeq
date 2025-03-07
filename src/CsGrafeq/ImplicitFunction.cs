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
    internal delegate (bool, bool) ImpFunctionResultCompared(Interval i1, Interval i2);
    internal delegate int NumberFunctionResultCompared(double n1,double n2);
    internal class ImplicitFunction
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

        internal ImpFunctionResultCompared ImpFunction;
        internal NumberFunctionResultCompared NumberFunction;
        private string _Expression;
        internal Color color;
        internal ExpressionType Type;
        internal string ExpressionRecord;
        public (bool, bool) GetFunctionResult(Interval i1, Interval i2)
        {
            if (ImpFunction == null)
                throw new Exception("函数未生成");
            return ImpFunction(i1, i2);
        }
        public string Expression
        {
            get => _Expression;
        }
        public ImplicitFunction(string expression)
        {
            _Expression = expression;
            (ImpFunction,NumberFunction) = ExpressionComplier.Complie(expression);
            ExpressionRecord = ExpressionComplier.Record;
            color = GetRandomColor();
            if (!expression.Contains("="))
            {
                color = Color.FromArgb(120, color);
                Type=expression.Contains("<")?ExpressionType.Less:ExpressionType.Greater;
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

    }

}
