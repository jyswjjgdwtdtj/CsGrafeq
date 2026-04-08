namespace CsGrafeq.Interfaces;

public interface IPoint<T>
{
    T X { get; }
    T Y { get; }
    bool IsInvalid();
}