namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// Base class for all decompressor implementations
    /// </summary>
    public abstract class Decompressor
    {
        public mspack_system system { get; set; }
    }
}