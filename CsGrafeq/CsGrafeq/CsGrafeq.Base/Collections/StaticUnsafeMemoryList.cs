using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsGrafeq.Collections;

public static class StaticUnsafeMemoryList<T> where T : struct
{
    private static readonly unsafe void* pointer;
    private static int current;
    public const uint Capacity = 1024;
    public readonly static uint Size= (uint)Marshal.SizeOf<T>();

    static StaticUnsafeMemoryList()
    {
        unsafe
        {
            pointer = NativeMemory.Alloc(Capacity * Size);
        }
        Clear();
    }
    public unsafe static Span<T> Rent(int length)
    {
        var rentSpan= new Span<T>(Unsafe.Add<T>(pointer,current), length);
        current+=length;
        return rentSpan;
    }
    public static void Return(int length)
    {
        current -= length;
    }

    public static void Return(Span<T> span)
    {
        current -= span.Length;
    }

    public static void Clear()
    {
        current = 0;
    }
}