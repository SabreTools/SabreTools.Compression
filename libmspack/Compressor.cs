namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// Base class for all compressor implementations
    /// </summary>
    public abstract class Compressor
    {
        public mspack_system system { get; set; }
    }
}