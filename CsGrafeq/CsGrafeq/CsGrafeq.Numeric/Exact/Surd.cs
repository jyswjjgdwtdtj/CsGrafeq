
namespace CsGrafeq.Numeric.Exact;

public readonly struct Surd(int sign, uint radicand)
{
    public uint Radicand { get; }= radicand;
    public int Sign { get; }= sign;
    public double ToFloat()
    {
        return Math.Sqrt(Radicand)*Sign;
    }
    public override string ToString()
    {
        return $"{(Sign == -1 ? "-" : "")}√({Radicand})";
    }
}