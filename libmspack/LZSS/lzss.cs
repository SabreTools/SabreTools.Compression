namespace SabreTools.Compression.libmspack
{
    public static class lzss
    {
        public const int LZSS_WINDOW_SIZE = 4096;
        public const byte LZSS_WINDOW_FILL = 0x20;

        /// <summary>
        /// Decompresses an LZSS stream.
        /// 
        /// Input bytes will be read in as necessary using the system->read()
        /// function with the input file handle given. This will continue until
        /// system->read() returns 0 bytes, or an error. Errors will be passed
        /// out of the function as MSPACK_ERR_READ errors. Input streams should
        /// convey an "end of input stream" by refusing to supply all the bytes
        /// that LZSS asks for when they reach the end of the stream, rather
        /// than return an error code.
        /// 
        /// Output bytes will be passed to the system->write() function, using
        /// the output file handle given. More than one call may be made to
        /// system->write().
        /// 
        /// As EXPAND.EXE (SZDD/KWAJ), Microsoft Help and QBasic have slightly
        /// different encodings for the control byte and matches, a "mode"
        /// parameter is allowed, to choose the encoding.
        /// </summary>
        /// <param name="system">
        /// An mspack_system structure used to read from
        /// the input stream and write to the output
        /// stream, also to allocate and free memory.
        /// </param>
        /// <param name="input">An input stream with the LZSS data.</param>
        /// <param name="output">An output stream to write the decoded data to.</param>
        /// <param name="input_buffer_size">The number of bytes to use as an input bitstream buffer.</param>
        /// <param name="mode">One of <see cref="LZSS_MODE"/> values</param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public static MSPACK_ERR lzss_decompress(mspack_system system, mspack_file input, mspack_file output, int input_buffer_size, LZSS_MODE mode) => MSPACK_ERR.MSPACK_ERR_OK;
    }
}