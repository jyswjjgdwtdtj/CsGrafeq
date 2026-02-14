using System.Collections;

namespace CsGrafeq.Collections;

internal class BiDictionary<T1, T2>:IEnumerable<BiDictionary<T1,T2>.Pair<T1,T2>> where T1 : notnull where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private readonly Dictionary<T2, T1> _backward = new Dictionary<T2, T1>();

    public BiDictionary()
    {
        Forward = new Indexer<T1, T2>(_forward);
        Backward = new Indexer<T2, T1>(_backward);
    }

    public class Indexer<T3, T4>(Dictionary<T3, T4> dictionary)
        where T3 : notnull
        where T4 : notnull
    {
        public T4 this[T3 index]
        {
            get => dictionary[index];
            set => dictionary[index] = value;
        }
    }

    public class Pair<T5, T6>(T5 forwardKey, T6 backwardKey)
        where T5 : notnull
        where T6 : notnull
    {
        public readonly T5 ForwardKey = forwardKey;
        public readonly T6 BackwardKey = backwardKey;
    }

    public void Add(T1 t1, T2 t2)
    {
        _forward.Add(t1, t2);
        _backward.Add(t2, t1);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<Pair<T1, T2>> GetEnumerator()
    {
        return _forward.Select(kvp => new Pair<T1, T2>(kvp.Key, kvp.Value)).GetEnumerator();
    }

    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Backward { get; private set; }
}