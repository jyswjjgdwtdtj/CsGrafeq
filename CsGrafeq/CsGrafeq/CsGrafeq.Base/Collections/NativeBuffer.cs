using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsGrafeq.Collections;

public class NativeBuffer<T> : IDisposable where T : struct
{
    private unsafe T* pointer;

    public NativeBuffer(nuint length, bool setDefault = false)
    {
        Length = length;
        unsafe
        {
            pointer = (T*)NativeMemory.Alloc(length * Size);
        }

        if (setDefault)
            foreach (ref var item in this)
                item = default;
    }

    public NativeBuffer(Span<T> span) : this((nuint)span.Length)
    {
        unsafe
        {
            fixed (T* ptr = span)
            {
                Buffer.MemoryCopy(ptr, pointer, Size * span.Length, Size * span.Length);
            }
        }
    }

    public unsafe NativeBuffer(T value) : this(1)
    {
        *pointer = value;
    }

    public nuint Length { get; }
    public uint Size { get; } = (uint)Marshal.SizeOf<T>();

    public ref T this[nuint index]
    {
        get
        {
            unsafe
            {
                return ref index >= Length ? ref ThrowOutOfRange(index, Length) : ref *(pointer + index);
            }
        }
    }

    public unsafe ref T First => ref *pointer;
    public unsafe ref T Last => ref *(pointer + Length - 1);

    public virtual void Dispose()
    {
        unsafe
        {
            // 判断内存是否有效
            if ((int)pointer != 0)
            {
                NativeMemory.Free(pointer);
                pointer = (T*)0;
            }
        }

        GC.SuppressFinalize(this);
    }

    [DoesNotReturn]
    private ref T ThrowOutOfRange(nuint index, nuint length)
    {
        throw new IndexOutOfRangeException($"Index:{index} Length:{length}");
    }

    public NativeBufferEnumerator GetEnumerator()
    {
        unsafe
        {
            return new NativeBufferEnumerator(ref pointer, Length);
        }
    }

    ~NativeBuffer()
    {
        Dispose();
    }

    public NativeBuffer<T> Clone()
    {
        return Clone(0, Length);
    }

    public unsafe NativeBuffer<T> Clone(nuint start, nuint length)
    {
        NativeBuffer<T> buffer = new(length);
        Buffer.MemoryCopy(pointer + start, buffer.pointer, Size * length, Size * length);
        return buffer;
    }

    public NativeBuffer<T> SliceAndDispose(nuint start, nuint length)
    {
        var res = Clone(start, length);
        Dispose();
        return res;
    }

    public Span<T> AsSpan()
    {
        unsafe
        {
            return new Span<T>(pointer, (int)Length);
        }
    }

    public ref struct NativeBufferEnumerator
    {
        private readonly unsafe ref T* pointer;
        private readonly nuint length;
        private ref T current;
        private nuint index;

        public ref T Current
        {
            get
            {
                unsafe
                {
                    // 确保指向的内存仍然有效
                    if (pointer == (T*)0) return ref Unsafe.NullRef<T>();

                    return ref current;
                }
            }
        }

        public unsafe NativeBufferEnumerator(ref T* pointer, nuint length)
        {
            this.pointer = ref pointer;
            this.length = length;
            index = 0;
            current = ref Unsafe.NullRef<T>();
        }

        public bool MoveNext()
        {
            unsafe
            {
                // 确保没有越界并且指向的内存仍然有效
                if (index >= length || pointer == (T*)0) return false;

                if (Unsafe.IsNullRef(ref current)) current = ref *pointer;
                else current = ref Unsafe.Add(ref current, 1);
            }

            index++;
            return true;
        }
    }
}