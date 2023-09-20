namespace SabreTools.Compression.libmspack
{
    public static class macros
    {
        private static uint __egi32(FixedArray<byte> a, int n)
        {
            return (uint)((a[n + 3] << 24) | (a[n + 2] << 16) | (a[n + 1] << 8) | (a[n + 0]));
        }

        public static ulong EndGetI64(FixedArray<byte> a, int n)
        {
            return (__egi32(a, n + 4) << 32) | __egi32(a, n + 0);
        }

        public static uint EndGetI32(FixedArray<byte> a, int n)
        {
            return __egi32(a, n + 0);
        }

        public static ushort EndGetI16(FixedArray<byte> a, int n)
        {
            return (ushort)((a[n + 1] << 8) | a[n + 0]);
        }

        public static uint EndGetM32(FixedArray<byte> a, int n)
        {
            return (uint)((a[n + 0] << 24) | (a[n + 1] << 16) | (a[n + 2] << 8) | (a[n + 3]));
        }

        public static ushort EndGetM16(FixedArray<byte> a, int n)
        {
            return (ushort)((a[n + 0] << 8) | a[n + 1]);
        }
    }
}