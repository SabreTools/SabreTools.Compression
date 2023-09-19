namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for SZDD compressed files.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_szdd_decompressor()"/>
    /// <see cref="mspack_destroy_szdd_decompressor()"/>
    public abstract class msszdd_decompressor
    {
        public mspack_system system { get; set; }

        public MSPACK_ERR error { get; set; }

        /// <summary>
        /// Opens a SZDD file and reads the header.
        ///
        /// If the file opened is a valid SZDD file, all headers will be read and
        /// a msszddd_header structure will be returned.
        ///
        /// In the case of an error occuring, NULL is returned and the error code
        /// is available from last_error().
        ///
        /// The filename pointer should be considered "in use" until close() is
        /// called on the SZDD file.
        /// </summary>
        /// <param name="filename">
        /// The filename of the SZDD compressed file. This is
        /// passed directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a msszddd_header structure, or NULL on failure</returns>
        /// <see cref="close(msszddd_header)"/> 
        public abstract msszddd_header open(in string filename);

        /// <summary>
        /// Closes a previously opened SZDD file.
        ///
        /// This closes a SZDD file and frees the msszddd_header associated with
        /// it.
        ///
        /// The SZDD header pointer is now invalid and cannot be used again.
        /// </summary>
        /// <param name="szdd">The SZDD file to close</param>
        public abstract void close(msszddd_header szdd);

        /// <summary>
        /// Extracts the compressed data from a SZDD file.
        ///
        /// This decompresses the compressed SZDD data stream and writes it to
        /// an output file.
        /// </summary>
        /// <param name="szdd">The SZDD file to extract data from</param>
        /// <param name="filename">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR extract(msszddd_header szdd, in string filename);

        /// <summary>
        /// Decompresses an SZDD file to an output file in one step.
        ///
        /// This opens an SZDD file as input, reads the header, then decompresses
        /// the compressed data immediately to an output file, finally closing
        /// both the input and output file. It is more convenient to use than
        /// open() then extract() then close(), if you do not need to know the
        /// SZDD output size or missing character.
        /// </summary>
        /// <param name="input">
        /// The filename of the input SZDD file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR decompress(in string input, in string output);

        /// <summary>
        /// Returns the error code set by the most recently called method.
        ///
        /// This is useful for open() which does not return an
        /// error code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/> 
        /// <see cref="extract(msszddd_header, in string)"/> 
        /// <see cref="decompress(in string, in string)"/> 
        public abstract MSPACK_ERR last_error();
    }
}