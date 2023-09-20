using System;
using System.Runtime.InteropServices;

namespace SabreTools.Compression.libmspack
{
    public unsafe class FixedArray<T> where T : struct
    {
        public IntPtr Pointer { get; private set; }

        int sizeofT { get { return Marshal.SizeOf(typeof(T)); } }

        public T this[int i]
        {
            get
            {
                return (T)Marshal.PtrToStructure(Pointer + i * sizeofT, typeof(T));
            }
            set
            {
                Marshal.StructureToPtr(value, Pointer + i * sizeofT, false);
            }
        }

        public FixedArray(int length)
        {
            Pointer = Marshal.AllocHGlobal(sizeofT * length);
        }

        ~FixedArray()
        {
            Marshal.FreeHGlobal(Pointer);
        }
    }
}