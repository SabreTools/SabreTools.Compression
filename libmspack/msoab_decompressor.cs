namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for .LZX (Offline Address Book) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_oab_decompressor()"/> 
    /// <see cref="mspack_destroy_oab_decompressor()"/> 
    public abstract class msoab_decompressor
    {
        public mspack_system system { get; set; }

        public int buf_size { get; set; }

        /// <summary>
        /// Decompresses a full Offline Address Book file.
        ///
        /// If the input file is a valid compressed Offline Address Book file, 
        /// it will be read and the decompressed contents will be written to
        /// the output file.
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
        public abstract MSPACK_ERR decompress(in string input, in string output);

        /// <summary>
        /// Decompresses an Offline Address Book with an incremental patch file.
        ///
        /// This requires both a full UNCOMPRESSED Offline Address Book file to
        /// act as the "base", and a compressed incremental patch file as input.
        /// If the input file is valid, it will be decompressed with reference to
        /// the base file, and the decompressed contents will be written to the
        /// output file.
        ///
        /// There is no way to tell what the right base file is for the given
        /// incremental patch, but if you get it wrong, this will usually result
        /// in incorrect data being decompressed, which will then fail a checksum
        /// test.
        /// </summary>
        /// <param name="input">
        /// The filename of the input file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="base">
        /// The filename of the base file to which the
        /// incremental patch shall be applied. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename of the output file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR decompress_incremental(in string input, in string @base, in string output);

        /// <summary>
        /// Sets an OAB decompression engine parameter. Available only in OAB
        /// decompressor version 2 and above.
        ///
        /// - #MSOABD_PARAM_DECOMPBUF: How many bytes should be used as an input
        ///   buffer by decompressors? The minimum value is 16. The default value
        ///   is 4096.
        /// </summary>
        /// <param name="param">The parameter to set</param>
        /// <param name="value">The value to set the parameter to</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        public abstract MSPACK_ERR set_param(MSOABD_PARAM param, int value);
    }
}