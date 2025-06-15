using CsGrafeq.Base;
using CsGrafeq.Expression;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeq.Expression.ExpressionCompiler;
using static CsGrafeq.Base.Values;
using System.Collections.Concurrent;
using static CsGrafeq.ExMethods;

namespace CsGrafeq.Implicit
{
    internal delegate (bool, bool) IntervalSetImpFunctionDelegate(IntervalSet i1, IntervalSet i2, double[] constlist);
    internal delegate int NumberImpFunctionDelegate(double n1, double n2, double[] constlist);
    internal delegate bool MarchingSquaresDelegate(double left, double top, double right, double bottom, double[] constlist);
    public class ImplicitFunc : Addon
    {
        internal string _Expression="This ImplicitFunc Addon is not constructed with string form expression";
        internal Color color=GetRandomDarkColor();
        internal MarchingSquaresDelegate MarchingSquaresFunction;
        internal IntervalSetImpFunctionDelegate IntervalSetImpFunction;
        internal bool[] UsedConstant = new bool['z' - 'a' + 1];
        public CheckPixelMode CheckPixelMode = CheckPixelMode.None;
        public string Expression
        {
            get => _Expression;
        }
        public ImplicitFunc(string expression)
        {
            _Expression = expression;
            IntervalSetImpFunction = ExpressionCompiler.Compile<IntervalSetImpFunctionDelegate>(expression, new CompileInfo(typeof(IntervalSetMath)));
            UsedConstant = ExpressionCompiler.usedconst;
            MarchingSquaresFunction = GetMSDelegate(expression);
            _AddonMode = AddonMode.ForPlot;
            _RenderMode = RenderMode.Move;
        }
        public ImplicitFunc(ComparedExpression ce)
        {
            IntervalSetImpFunction = ExpressionCompiler.Compile<IntervalSetImpFunctionDelegate>(ce.Elements.ToArray(), new CompileInfo(typeof(IntervalSetMath)));
            UsedConstant = ExpressionCompiler.usedconst;
            MarchingSquaresFunction = GetMSDelegate(ce.Elements.ToArray());
            _AddonMode = AddonMode.ForPlot;
            _RenderMode = RenderMode.Move;
        }
        protected override void Render(Graphics graphics, Rectangle rect)
        {
            RefreshOwnerArguments();
            RenderFunction(graphics, rect);
        }
        #region Compile

        private static readonly MethodInfoHelper NumberMethods = new MethodInfoHelper(typeof(NumberMath));
        private static MarchingSquaresDelegate GetMSDelegate(Element[] eles)
        {
            DynamicMethod imp = new DynamicMethod("MSFunction", typeof(bool), new Type[] { typeof(double), typeof(double), typeof(double), typeof(double), typeof(double[]) });
            ILGenerator ilg = imp.GetILGenerator();
            ILRecorder il = new ILRecorder(ilg);
            Stack<DynamicMethod> numfuncs = new Stack<DynamicMethod>();
            Stack<Element> uiopers = new Stack<Element>();
            Stack<Element> cpopers = new Stack<Element>();
            List<Element> elelist = new List<Element>();
            int loc = 0;
            string[] s = new string[] { "less", "greater", "lessequal", "greaterequal", "equal" };
            for (; loc < eles.Length; loc++)
            {
                if (eles[loc].NameOrValue == "Union" || eles[loc].NameOrValue == "Intersect")
                {
                    uiopers.Push(eles[loc]);
                    continue;
                }
                elelist.Add(eles[loc]);
                if (s.Contains(eles[loc].NameOrValue.ToLower()))
                {
                    cpopers.Push(eles[loc]);
                    numfuncs.Push(GetNumDelegate(elelist.ToArray()));
                    elelist.Clear();
                    continue;
                }

            }
            OpCode left = OpCodes.Ldarg_0;
            OpCode top = OpCodes.Ldarg_1;
            OpCode right = OpCodes.Ldarg_2;
            OpCode bottom = OpCodes.Ldarg_3;
            MethodInfo allless = ((Func<int, int, int, int, bool>)NumberMath.IsAllLeThanZero).Method;
            MethodInfo allgreater = ((Func<int, int, int, int, bool>)NumberMath.IsAllGeThanZero).Method;
            MethodInfo crosszero = ((Func<int, int, int, int, bool>)NumberMath.IsCrossZero).Method;
            //载入四个点
            DynamicMethod numfunc = numfuncs.Pop();
            il.Emit(left);
            il.Emit(top);
            il.Emit(OpCodes.Ldarg, 4);
            il.Emit(OpCodes.Call, numfunc);
            il.Emit(left);
            il.Emit(bottom);
            il.Emit(OpCodes.Ldarg, 4);
            il.Emit(OpCodes.Call, numfunc);
            il.Emit(right);
            il.Emit(top);
            il.Emit(OpCodes.Ldarg, 4);
            il.Emit(OpCodes.Call, numfunc);
            il.Emit(right);
            il.Emit(bottom);
            il.Emit(OpCodes.Ldarg, 4);
            il.Emit(OpCodes.Call, numfunc);
            Element ele = cpopers.Pop();
            if (ele.NameOrValue.Contains("Less"))
                il.Emit(OpCodes.Call, allless);
            else if (ele.NameOrValue.Contains("Greater"))
                il.Emit(OpCodes.Call, allgreater);
            else
                il.Emit(OpCodes.Call, crosszero);
            //此时栈中有一个bool类型
            for (int i = 0; i < uiopers.Count; i++)
            {
                //载入四个点
                numfunc = numfuncs.Pop();
                il.Emit(left);
                il.Emit(top);
                il.Emit(OpCodes.Ldarg, 4);
                il.Emit(OpCodes.Call, numfunc);
                il.Emit(left);
                il.Emit(bottom);
                il.Emit(OpCodes.Ldarg, 4);
                il.Emit(OpCodes.Call, numfunc);
                il.Emit(right);
                il.Emit(top);
                il.Emit(OpCodes.Ldarg, 4);
                il.Emit(OpCodes.Call, numfunc);
                il.Emit(right);
                il.Emit(bottom);
                il.Emit(OpCodes.Ldarg, 4);
                il.Emit(OpCodes.Call, numfunc);
                ele = cpopers.Pop();
                if (ele.NameOrValue.Contains("Less"))
                    il.Emit(OpCodes.Call, allless);
                else if (ele.NameOrValue.Contains("Greater"))
                    il.Emit(OpCodes.Call, allgreater);
                else
                    il.Emit(OpCodes.Call, crosszero);
                //此时栈中有两个bool类型
                ele = uiopers.Pop();
                if (ele.NameOrValue == "Union")
                    il.Emit(OpCodes.Or);
                else
                    il.Emit(OpCodes.And);
                //此时有一个bool类型
            }
            //此时有一个bool类型
            il.Emit(OpCodes.Ret);
            return (MarchingSquaresDelegate)imp.CreateDelegate(typeof(MarchingSquaresDelegate));
        }
        private static MarchingSquaresDelegate GetMSDelegate(string exp)
        {
            Element[] eles = ExpressionCompiler.ParseTokens(ExpressionCompiler.GetTokens(exp));
            return GetMSDelegate(eles);
        }
        private static DynamicMethod GetNumDelegate(Element[] eles)
        {
            foreach (Element ele in eles)
            {
                if (ele.NameOrValue == "Union" || ele.NameOrValue == "Intersect")
                    throw new Exception();
            }
            DynamicMethod imp = new DynamicMethod("NumFunction", typeof(int), new Type[] { typeof(double), typeof(double), typeof(double[]) });
            ILGenerator ilg = imp.GetILGenerator();
            ILRecorder il = new ILRecorder(ilg);
            EmitElements(il, eles, NumberMethods, new char[] { 'x', 'y' }, true);
            il.Emit(OpCodes.Ret);
            imp.Invoke(null, new object[] { 0, 0, new double[26] });
            return imp;
        }
        #endregion
        private void RenderFunction(Graphics rt, Rectangle targetrect)
        {
            ConcurrentBag<Rectangle> RectToCalc = new ConcurrentBag<Rectangle>() { targetrect };
            ConcurrentBag<RectangleF> RectToRender = new ConcurrentBag<RectangleF>();
            SolidBrush brush = new SolidBrush(color);
            Func<int, int, int, int, bool> func = null;
            bool checkpixel = CheckPixelMode != CheckPixelMode.None;
            do
            {
                Rectangle[] rs = RectToCalc.ToArray();
                RectToCalc = new ConcurrentBag<Rectangle>();
                Action<int> atn = (idx) => RenderRectIntervalSet(rt,rs[idx], RectToCalc, RectToRender, brush, func, checkpixel);
                for (int i = 0; i < rs.Length; i += 100)
                {
                    RectToRender = new ConcurrentBag<RectangleF>();
                    Parallel.For(i, Math.Min(i + 100, rs.Length), atn);
                    if (RectToRender.Count != 0)
                        rt.FillRectangles(brush, RectToRender.ToArray());
                    RectToRender = null;
                }
            } while (RectToCalc.Count != 0);
        }
        private void RenderRectIntervalSet(Graphics rt, Rectangle r, ConcurrentBag<Rectangle> RectToCalc, ConcurrentBag<RectangleF> RectToRender, SolidBrush brush, Func<int, int, int, int, bool> func, bool checkpixel)
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
            for (int i = r.Left; i < r.Right; i += dx)
            {
                double di = i;
                double xmin = PixelToMathX(i);
                double xmax = PixelToMathX(i + dx);
                IntervalSet xi = new IntervalSet(xmin, xmax);
                for (int j = r.Top; j < r.Bottom; j += dy)
                {
                    double dj = j;
                    double ymin = PixelToMathY(j);
                    double ymax = PixelToMathY((j + dy));
                    IntervalSet yi = new IntervalSet(ymin, ymax);
                    (bool first, bool second) result =IntervalSetImpFunction.Invoke(xi, yi, Constants);
                    if (result == TT)
                    {
                        RectToRender.Add(CreateRectFByBound((float)(di), (float)(dj), (float)Math.Min((di + dx), r.Right), (float)Math.Min((dj + dy), r.Bottom)));
                    }
                    else if (result == FT)
                    {
                        if (dx == 1 && dx == 1)
                        {
                            if ((!checkpixel) || (checkpixel && CheckCurveExists(xmin, ymin, xmax, ymax, Xexhaust, Yexhaust, Constants)))
                                RectToRender.Add(CreateRectFByBound((float)(di), (float)(dj), (float)Math.Min((di + dx), r.Right), (float)Math.Min((dj + dy), r.Bottom)));
                        }
                        else
                        {
                            RectToCalc.Add(CreateRectByBound(i, j, Math.Min(i + dx, r.Right), Math.Min(j + dy, r.Bottom)));
                        }
                    }
                }
            }
        }
        internal bool CheckCurveExists(double xmin, double ymin, double xmax, double ymax, double Xexhaust, double Yexhaust, double[] consts)
        {
            if (MarchingSquaresFunction.Invoke(xmin, ymin, xmax, ymax, consts))
                return true;
            if (xmax - xmin < Xexhaust && ymax - ymin < Yexhaust)
                return false;
            if (ymax - ymin > xmax - xmin)
            {
                double half = (ymax + ymin) / 2;
                return
                    CheckCurveExists(xmin, ymin, xmax, half, Xexhaust, Yexhaust, consts) ||
                    CheckCurveExists( xmin, half, xmax, ymax, Xexhaust, Yexhaust, consts);
            }
            else
            {
                double half = (xmax + xmin) / 2;
                return
                    CheckCurveExists(xmin, ymin, half, ymax, Xexhaust, Yexhaust, consts) ||
                    CheckCurveExists(half, ymin, xmax, ymax, Xexhaust, Yexhaust, consts);
            }
        }
    }
    public enum CheckPixelMode
    {
        UseMarchingSquares, None
    }

}
namespace CsGrafeq
{
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
        private static readonly Array colors = Enum.GetValues(typeof(KnownColor));
        private static readonly Random rnd = new Random();
        private static readonly double k = Math.Pow(1 / (1 + Math.Pow(1.5, 2.2) + Math.Pow(0.6, 2.2)), 1 / 2.2);
        public static double GetRealBrightness(Color c)
        {
            return (k * Math.Pow(Math.Pow(((double)c.R / 255), 2.2) + Math.Pow(((double)c.G / 255 * 1.5), 2.2) + Math.Pow(((double)c.B / 255 * 0.6), 2.2), 1 / 2.2));
        }
        public static Color GetRandomDarkColor()
        {
            Color color;
            do
            {
                color = Color.FromKnownColor((KnownColor)colors.GetValue(rnd.Next(colors.Length - 27) + 27));
            } while (GetRealBrightness(color) > 0.7);
            return color;
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

    }
}