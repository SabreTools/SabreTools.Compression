namespace SabreTools.Compression.LZX
{
    /// <see href="https://github.com/wine-mirror/wine/blob/master/dlls/cabinet/cabinet.h"/>
    internal class Bits
    {
        public uint BitBuffer;

        public int BitsLeft;

        public int InputPosition; //byte*
    }
}