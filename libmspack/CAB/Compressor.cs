namespace SabreTools.Compression.libmspack.CAB
{
    /// <summary>
    /// TODO
    /// </summary>
    public class Compressor : BaseCompressor
    {
        /// <summary>
        /// Creates a new CAB compressor
        /// </summary>
        public Compressor()
        {
            this.system = new CABSystem();
            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }
    }
}