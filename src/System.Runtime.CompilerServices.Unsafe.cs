namespace System.Runtime.CompilerServices;

unsafe static class Unsafe
{
    public static void* AsPointer<T>(ref T value) { fixed (void* @this = &value) return @this; }
}