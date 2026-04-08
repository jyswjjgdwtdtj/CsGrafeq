using CsGrafeq.Interfaces;

namespace CsGrafeq.Numeric.Exact;


public struct VectorExact:IPoint<ExactNumber>
{
    
    public VectorExact(ExactNumber x, ExactNumber y)
    {
        X = x;
        Y = y;
    }
    public ExactNumber X { get; private set; }
    public ExactNumber Y { get;private set; }

    public void SetValue(ExactNumber x, ExactNumber y)
    {
        X = x;
        Y = y;
    }
    public static VectorExact Zero { get; } = new(default, default);
    public static VectorExact NaN { get; } = new(ExactNumber.NaN, ExactNumber.NaN);
    public Vec ToVec()
    {
        return new Vec(X.ToFloat(), Y.ToFloat());
    }
    public static VectorExact operator -(VectorExact left, VectorExact right)=>
        new VectorExact(left.X - right.X, left.Y - right.Y);
    public static VectorExact operator +(VectorExact left, VectorExact right)=>
        new VectorExact(left.X + right.X, left.Y + right.Y);
    
    public static VectorExact operator *(VectorExact left, ExactNumber right)=>
        new VectorExact(left.X * right, left.Y * right);
    public static VectorExact operator /(VectorExact left, ExactNumber right)=>
        new VectorExact(left.X / right, left.Y / right);
    public ExactNumber GetLength()
    {
        return ExactNumber.Sqrt(X * X + Y * Y);
    }

    public bool IsInvalid()
    {
        return double.IsNaN(X.ToFloat())||double.IsNaN(Y.ToFloat());
    }

    public override string ToString()
    {
        return $"{{{X},{Y}}}";
    }
}