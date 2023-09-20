namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for KWAJ compressed files.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_kwaj_decompressor()"/> 
    /// <see cref="mspack_destroy_kwaj_decompressor()"/> 
    public abstract class mskwaj_decompressor : Decompressor
    {
        /// <summary>
        /// Opens a KWAJ file and reads the header.
        ///
        /// If the file opened is a valid KWAJ file, all headers will be read and
        /// a mskwajd_header structure will be returned.
        ///
        /// In the case of an error occuring, null is returned and the error code
        /// is available from last_error().
        ///
        /// The filename pointer should be considered "in use" until close() is
        /// called on the KWAJ file.
        /// </summary>
        /// <param name="filename">
        /// The filename of the KWAJ compressed file. This is
        /// passed directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mskwajd_header structure, or null on failure</returns>
        /// <see cref="close(mskwajd_header)"/>
        public abstract mskwajd_header open(in string filename);

        /// <summary>
        /// Closes a previously opened KWAJ file.
        ///
        /// This closes a KWAJ file and frees the mskwajd_header associated
        /// with it. The KWAJ header pointer is now invalid and cannot be
        /// used again.
        /// </summary>
        /// <param name="kwaj">The KWAJ file to close</param>
        /// <see cref="open(in string)"/> 
        public abstract void close(mskwajd_header kwaj);

        /// <summary>
        /// Extracts the compressed data from a KWAJ file.
        ///
        /// This decompresses the compressed KWAJ data stream and writes it to
        /// an output file.
        /// </summary>
        /// <param name="kwaj">The KWAJ file to extract data from</param>
        /// <param name="filename">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR extract(mskwajd_header kwaj, in string filename);

        /// <summary>
        /// Decompresses an KWAJ file to an output file in one step.
        ///
        /// This opens an KWAJ file as input, reads the header, then decompresses
        /// the compressed data immediately to an output file, finally closing
        /// both the input and output file. It is more convenient to use than
        /// open() then extract() then close(), if you do not need to know the
        /// KWAJ output size or output filename.
        /// </summary>
        /// <param name="input">
        /// The filename of the input KWAJ file. This is passed
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
        /// <see cref="search()"/> 
        public abstract MSPACK_ERR last_error();
    }
}