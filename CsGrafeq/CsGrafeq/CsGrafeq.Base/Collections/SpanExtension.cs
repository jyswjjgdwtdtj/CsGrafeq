namespace CsGrafeq.Collections;

public static class SpanExtension
{
    extension<T>(Span<T> span)
    {
        public Span<T> SetInForce(T first)
        {
            span[0] = first;
            return span;
        }
        public Span<T> SetInForce(T first,T second)
        {
            span[0] = first;
            span[1]= second;
            return span;
        }
        public Span<T> SetInForce(T first, T second,T third)
        {
            span[0] = first;
            span[1]= second;
            span[2]= third;
            return span;
        }
        public Span<T> SetInForce(T first, T second, T third,T fourth)
        {
            span[0] = first;
            span[1]= second;
            span[2]= third;
            span[3]= fourth;
            return span;
        }
        public Tout[] Select<Tout>(Func<T,Tout> selector)
        {
            var result = new Tout[span.Length];
            for(var i=0;i<span.Length;i++)
            {
                result[i]=selector(span[i]);
            }

            return result;
        }
    }
}