namespace CsGrafeq.Collections;

public ref struct UnsafeMemoryList<T> : IDisposable where T : struct
{
    private readonly NativeBuffer<T> _originalBuffer;
    private Span<T> _buffer;

    public UnsafeMemoryList(int capacity)
    {
        _originalBuffer = new NativeBuffer<T>((nuint)capacity);
        _buffer = _originalBuffer.AsSpan();
        Capacity = capacity;
        Pointer = 0;
    }

    public Span<T> Rent(int length)
    {
        var rentSpan = _buffer.Slice(Pointer, length);
        Pointer += length;
        return rentSpan;
    }

    public void Return(int length)
    {
        Pointer -= length;
    }

    public int Capacity { get; private set; }

    public int Pointer { get; private set; }

    public void Dispose()
    {
        _originalBuffer.Dispose();
        Capacity = 0;
        Pointer = 0;
        _buffer = default;
    }
}