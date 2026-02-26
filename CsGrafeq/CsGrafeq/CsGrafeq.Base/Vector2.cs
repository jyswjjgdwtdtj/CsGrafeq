using CsGrafeq.Interfaces;
using CsGrafeq.MVVM;
using ReactiveUI;

namespace CsGrafeq;

public class Vector2<T> : ObservableObject, IPoint<T>
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

    public bool IsInvalid()
    {
        return false;
    }
}
