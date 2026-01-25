using System.Buffers;

namespace CsGrafeq.Collections;

public ref struct UnsafeMemoryList<T>:IDisposable where T : struct
{
    private NativeBuffer<T> _originalBuffer;
    private Span<T> _buffer;
    private int _capacity;
    private int _pointer;

    public UnsafeMemoryList(int capacity)
    {
        _originalBuffer= new NativeBuffer<T>((nuint)capacity);
        _buffer= _originalBuffer.AsSpan();
        _capacity= capacity;
        _pointer= 0;
    }
    
    public Span<T> Rent(int length)
    {
        var rentSpan= _buffer.Slice(_pointer, length);
        _pointer += length;
        return rentSpan;
    }

    public void Return(int length)
    {
        _pointer -= length;
    }
    public int Capacity => _capacity;
    public int Pointer => _pointer;

    public void Dispose()
    {
        _originalBuffer.Dispose();
        _capacity = 0;
        _pointer = 0;
        _buffer = default;
    }
}