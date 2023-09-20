namespace SabreTools.Compression.libmspack
{
    public static class mszip
    {
        /// <summary>
        /// Size of LZ history window
        /// </summary>
        public const int MSZIP_FRAME_SIZE = 32768;

        /// <summary>
        /// Literal/length huffman tree
        /// </summary>
        public const int MSZIP_LITERAL_MAXSYMBOLS = 288;

        public const int MSZIP_LITERAL_TABLEBITS = 9;

        /// <summary>
        /// Distance huffman tree
        /// </summary>
        public const int MSZIP_DISTANCE_MAXSYMBOLS = 32;

        public const int MSZIP_DISTANCE_TABLEBITS = 6;

        // If there are less direct lookup entries than symbols, the longer
        // code pointers will be <= maxsymbols. This must not happen, or we
        // will decode entries badly
        public const int MSZIP_LITERAL_TABLESIZE = MSZIP_LITERAL_MAXSYMBOLS * 4;
        public const int MSZIP_DISTANCE_TABLESIZE = 1 << MSZIP_DISTANCE_TABLEBITS + (MSZIP_DISTANCE_MAXSYMBOLS * 2);

        /// <summary>
        /// Allocates MS-ZIP decompression stream for decoding the given stream.
        /// 
        /// - uses system->alloc() to allocate memory
        ///
        /// - returns null if not enough memory
        ///
        /// - input_buffer_size is how many bytes to use as an input bitstream buffer
        ///
        /// - if repair_mode is non-zero, errors in decompression will be skipped
        ///   and 'holes' left will be filled with zero bytes. This allows at least
        ///   a partial recovery of erroneous data.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="input_buffer_size"></param>
        /// <param name="repair_mode"></param>
        /// <returns></returns>
        public static mszipd_stream mszipd_init(mspack_system system, mspack_file input, mspack_file output, int input_buffer_size, int repair_mode) => null;
    
        /// <summary>
        /// Decompresses, or decompresses more of, an MS-ZIP stream.
        ///
        /// - out_bytes of data will be decompressed and the function will return
        ///   with an MSPACK_ERR_OK return code.
        ///
        /// - decompressing will stop as soon as out_bytes is reached. if the true
        ///   amount of bytes decoded spills over that amount, they will be kept for
        ///   a later invocation of mszipd_decompress().
        ///
        /// - the output bytes will be passed to the system->write() function given in
        ///   mszipd_init(), using the output file handle given in mszipd_init(). More
        ///   than one call may be made to system->write()
        ///
        /// - MS-ZIP will read input bytes as necessary using the system->read()
        ///   function given in mszipd_init(), using the input file handle given in
        ///   mszipd_init(). This will continue until system->read() returns 0 bytes,
        ///   or an error.
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="out_bytes"></param>
        /// <returns></returns>
        public static MSPACK_ERR mszipd_decompress(mszipd_stream zip, long out_bytes) => MSPACK_ERR.MSPACK_ERR_OK;
    
        /// <summary>
        /// Decompresses an entire MS-ZIP stream in a KWAJ file. Acts very much
        /// like mszipd_decompress(), but doesn't take an out_bytes parameter
        /// </summary>
        /// <param name="zip"></param>
        /// <returns></returns>
        public static MSPACK_ERR mszipd_decompress_kwaj(mszipd_stream zip) => MSPACK_ERR.MSPACK_ERR_OK;
    
        /// <summary>
        /// Frees all stream associated with an MS-ZIP data stream
        ///
        /// - calls system->free() using the system pointer given in mszipd_init()
        /// </summary>
        /// <param name="zip"></param>
        public static void mszipd_free(mszipd_stream zip) { }
    }
}