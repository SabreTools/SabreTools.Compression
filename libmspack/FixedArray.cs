using System;
using System.Runtime.InteropServices;

namespace SabreTools.Compression.libmspack
{
    public unsafe class FixedArray<T> where T : struct
    {
        /// <summary>
        /// Direct access to the internal pointer
        /// </summary>
        public IntPtr Pointer { get; private set; }

        /// <summary>
        /// Size of the T object
        /// </summary>
        private int sizeofT { get { return Marshal.SizeOf(typeof(T)); } }

        /// <summary>
        /// Length of the fixed array
        /// </summary>
        private int _length;

        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= _length)
                    return default;

                return (T)Marshal.PtrToStructure(Pointer + i * sizeofT, typeof(T));
            }
            set
            {
                if (i < 0 || i >= _length)
                    return;

                Marshal.StructureToPtr(value, Pointer + i * sizeofT, false);
            }
        }

        public FixedArray(int length)
        {
            Pointer = Marshal.AllocHGlobal(sizeofT * length);
            _length = 0;
        }

        ~FixedArray()
        {
            Marshal.FreeHGlobal(Pointer);
        }

        public static implicit operator T*(FixedArray<T> arr) => (T*)arr.Pointer;

        public static implicit operator T[](FixedArray<T> arr) => arr.ToArray();

        /// <inheritdoc cref="System.Linq.Enumerable.SequenceEqual{TSource}(System.Collections.Generic.IEnumerable{TSource}, System.Collections.Generic.IEnumerable{TSource})"/>
        public bool SequenceEqual(T[] arr)
        {
            if (arr.Length < _length)
                return false;

            for (int i = 0; i < _length; i++)
            {
                if (!this[i].Equals(arr[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Convert the unmanaged data to an array
        /// </summary>
        /// <returns>Array created from the pointer data</returns>
        public T[] ToArray()
        {
            T[] arr = new T[_length];
            for (int i = 0; i < _length; i++)
            {
                arr[i] = this[i];
            }

            return arr;
        }
    }
}