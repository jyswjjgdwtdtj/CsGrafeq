namespace CsGrafeq.Numeric;

public interface INeedClone<T> where T : INeedClone<T>
{
    static abstract T Clone(T source);
}