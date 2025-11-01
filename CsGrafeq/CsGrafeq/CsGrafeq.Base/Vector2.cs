using CsGrafeq.Interfaces;
using ReactiveUI;

namespace CsGrafeq;

public class Vector2<T> : ReactiveObject, IPoint<T>
{
    public Vector2(T x, T y)
    {
        X = x;
        Y = y;
    }

    public T X
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public T Y
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public void SetValue(T x, T y)
    {
        X = x;
        Y = y;
    }
}

public class Vector2Double : Vector2<double>
{
    public Vector2Double(double x, double y) : base(x, y)
    {
    }

    public static implicit operator Vec(Vector2Double vec)
    {
        return new Vec(vec.X, vec.Y);
    }

    public static implicit operator Vector2Double(Vec vec)
    {
        return new Vector2Double(vec.X, vec.Y);
    }

    public void SetValue(Vec vec)
    {
        SetValue(vec.X, vec.Y);
    }
}