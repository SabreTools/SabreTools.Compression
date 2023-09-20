namespace SabreTools.Compression.libmspack.HLP
{
    /// <summary>
    /// TODO
    /// </summary>
    public class Decompressor : BaseDecompressor
    {
        /// <summary>
        /// Creates a new HLP decompressor
        /// </summary>
        public Decompressor()
        {
            this.system = new mspack_default_system();
            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }
    }
}