namespace CsGrafeq.Numeric;

public interface INeedClone<T> where T : INeedClone<T>, allows ref struct
{
    static abstract bool NeedClone { get; }
    static abstract T Clone(T source);
}