namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A compressor for the Offline Address Book (OAB) format.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_oab_compressor()"/> 
    /// <see cref="mspack_destroy_oab_compressor()"/> 
    public abstract class msoab_compressor : BaseCompressor
    {
        /// <summary>
        /// Compress a full OAB file.
        ///
        /// The input file will be read and the compressed contents written to the
        /// output file.
        /// </summary>
        /// <param name="input">
        /// The filename of the input file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename of the output file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR compress(in string input, in string output);

        /// <summary>
        /// Generate a compressed incremental OAB patch file.
        ///
        /// The two uncompressed files "input" and "base" will be read, and an
        /// incremental patch to generate "input" from "base" will be written to
        /// the output file.
        /// </summary>
        /// <param name="input">
        /// The filename of the input file containing the new
        /// version of its contents. This is passed directly
        /// to mspack_system::open().
        /// </param>
        /// <param name="base">
        /// The filename of the original base file containing
        /// the old version of its contents, against which the
        /// incremental patch shall generated. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename of the output file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR compress_incremental(in string input, in string @base, in string output);
    }
}