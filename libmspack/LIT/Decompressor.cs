namespace SabreTools.Compression.libmspack.LIT
{
    /// <summary>
    /// TODO
    /// </summary>
    public class Decompressor : BaseDecompressor
    {
        /// <summary>
        /// Creates a new LIT decompressor
        /// </summary>
        public Decompressor()
        {
            this.system = new mspack_default_system();
            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }
    }
}