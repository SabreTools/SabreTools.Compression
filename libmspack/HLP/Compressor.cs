namespace SabreTools.Compression.libmspack.HLP
{
    /// <summary>
    /// TODO
    /// </summary>
    public class Compressor : BaseCompressor
    {
        /// <summary>
        /// Creates a new HLP compressor
        /// </summary>
        public Compressor()
        {
            this.system = new mspack_default_system();
            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }
    }
}