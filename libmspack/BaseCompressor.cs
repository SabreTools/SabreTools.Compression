namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// Base class for all compressor implementations
    /// </summary>
    public abstract class BaseCompressor : mspack_file
    {
#if NET48
        public mspack_system system { get; set; }
#else
        public mspack_system? system { get; set; }
#endif

        public MSPACK_ERR error { get; set; }
    }
}