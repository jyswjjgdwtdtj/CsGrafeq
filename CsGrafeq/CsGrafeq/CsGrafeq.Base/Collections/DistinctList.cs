using System.Collections;

namespace CsGrafeq.Collections;

public class DistinctList<T> : IList<T>
{
    private readonly List<T> _innerList = new();

    public void Add(T shape)
    {
        if (!_innerList.Contains(shape))
            _innerList.Add(shape);
    }

    public void Clear()
    {
        _innerList.Clear();
    }

    public bool Contains(T shape)
    {
        return _innerList.Contains(shape);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _innerList.CopyTo(array, arrayIndex);
    }

    public bool Remove(T shape)
    {
        return _innerList.Remove(shape);
    }

    public int Count => _innerList.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator()
    {
        return _innerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _innerList.GetEnumerator();
    }

    public int IndexOf(T shape)
    {
        return _innerList.IndexOf(shape);
    }

    public T this[int index]
    {
        get => _innerList[index];
        set => _innerList[index] = value;
    }

    public void Insert(int index, T shape)
    {
        _innerList.Insert(index, shape);
    }

    public void RemoveAt(int index)
    {
        _innerList.RemoveAt(index);
    }

    public T[] ToArray()
    {
        return _innerList.ToArray();
    }
}