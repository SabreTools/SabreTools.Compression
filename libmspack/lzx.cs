namespace SabreTools.Compression.libmspack
{
    public static class lzx
    {
        // Some constants defined by the LZX specification
        public const int LZX_MIN_MATCH = 2;
        public const int LZX_MAX_MATCH = 257;
        public const int LZX_NUM_CHARS = 256;
        public const int LZX_BLOCKTYPE_INVALID = 0;   /* also blocktypes 4-7 invalid */
        public const int LZX_BLOCKTYPE_VERBATIM = 1;
        public const int LZX_BLOCKTYPE_ALIGNED = 2;
        public const int LZX_BLOCKTYPE_UNCOMPRESSED = 3;
        public const int LZX_PRETREE_NUM_ELEMENTS = 20;
        public const int LZX_ALIGNED_NUM_ELEMENTS = 8;   /* aligned offset tree #elements */
        public const int LZX_NUM_PRIMARY_LENGTHS = 7;   /* this one missing from spec! */
        public const int LZX_NUM_SECONDARY_LENGTHS = 249; /* length tree #elements */

        // LZX huffman defines: tweak tablebits as desired
        public const int LZX_PRETREE_MAXSYMBOLS = LZX_PRETREE_NUM_ELEMENTS;
        public const int LZX_PRETREE_TABLEBITS = 6;
        public const int LZX_MAINTREE_MAXSYMBOLS = LZX_NUM_CHARS + 290 * 8;
        public const int LZX_MAINTREE_TABLEBITS = 12;
        public const int LZX_LENGTH_MAXSYMBOLS = LZX_NUM_SECONDARY_LENGTHS + 1;
        public const int LZX_LENGTH_TABLEBITS = 12;
        public const int LZX_ALIGNED_MAXSYMBOLS = LZX_ALIGNED_NUM_ELEMENTS;
        public const int LZX_ALIGNED_TABLEBITS = 7;
        public const int LZX_LENTABLE_SAFETY = 64;  /* table decoding overruns are allowed */

        public const int LZX_FRAME_SIZE = 32768; /* the size of a frame in LZX */

        /// <summary>
        /// Allocates and initialises LZX decompression state for decoding an LZX
        /// stream.
        /// 
        /// This routine uses system->alloc() to allocate memory. If memory
        /// allocation fails, or the parameters to this function are invalid,
        /// NULL is returned.
        /// </summary>
        /// <param name="system">
        /// An mspack_system structure used to read from
        /// the input stream and write to the output
        /// stream, also to allocate and free memory.
        /// </param>
        /// <param name="input">An input stream with the LZX data.</param>
        /// <param name="output">An output stream to write the decoded data to.</param>
        /// <param name="window_bits">
        /// The size of the decoding window, which must be
        /// between 15 and 21 inclusive for regular LZX
        /// data, or between 17 and 25 inclusive for
        /// LZX DELTA data.
        /// </param>
        /// <param name="reset_interval">
        /// The interval at which the LZX bitstream is
        /// reset, in multiples of LZX frames (32678
        /// bytes), e.g. a value of 2 indicates the input
        /// stream resets after every 65536 output bytes.
        /// A value of 0 indicates that the bitstream never
        /// resets, such as in CAB LZX streams.
        /// </param>
        /// <param name="input_buffer_size">The number of bytes to use as an input bitstream buffer.</param>
        /// <param name="output_length">
        /// The length in bytes of the entirely
        /// decompressed output stream, if known in
        /// advance. It is used to correctly perform the
        /// Intel E8 transformation, which must stop 6
        /// bytes before the very end of the
        /// decompressed stream. It is not otherwise used
        /// or adhered to. If the full decompressed
        /// length is known in advance, set it here.
        /// If it is NOT known, use the value 0, and call
        /// lzxd_set_output_length() once it is
        /// known. If never set, 4 of the final 6 bytes
        /// of the output stream may be incorrect.
        /// </param>
        /// <param name="is_delta">
        /// Should be zero for all regular LZX data,
        /// non-zero for LZX DELTA encoded data.
        /// </param>
        /// <returns>
        /// A pointer to an initialised lzxd_stream structure, or NULL if
        /// there was not enough memory or parameters to the function were wrong.
        /// </returns>
        public static lzxd_stream lzxd_init(mspack_system system, mspack_file input, mspack_file output, int window_bits, int reset_interval, int input_buffer_size, long output_length, char is_delta) => null;

        /// <summary>
        /// See description of output_length in lzxd_init()
        /// </summary>
        public static void lzxd_set_output_length(lzxd_stream lzx, long output_length) { }
    
        /// <summary>
        /// Reads LZX DELTA reference data into the window and allows
        /// lzxd_decompress() to reference it.
        /// 
        /// Call this before the first call to lzxd_decompress().
        /// </summary>
        /// <param name="lzx">The LZX stream to apply this reference data to</param>
        /// <param name="system">
        /// An mspack_system implementation to use with the
        /// input param. Only read() will be called.
        /// </param>
        /// <param name="input">
        /// An input file handle to read reference data using
        /// system->read().
        /// </param>
        /// <param name="length">
        /// The length of the reference data. Cannot be longer
        /// than the LZX window size.
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public static MSPACK_ERR lzxd_set_reference_data(lzxd_stream lzx, mspack_system system, mspack_file input, uint length) => MSPACK_ERR.MSPACK_ERR_OK;
    
        /// <summary>
        /// Decompresses entire or partial LZX streams.
        /// 
        /// The number of bytes of data that should be decompressed is given as the
        /// out_bytes parameter. If more bytes are decoded than are needed, they
        /// will be kept over for a later invocation.
        /// 
        /// The output bytes will be passed to the system->write() function given in
        /// lzxd_init(), using the output file handle given in lzxd_init(). More than
        /// one call may be made to system->write().
        /// 
        /// Input bytes will be read in as necessary using the system->read()
        /// function given in lzxd_init(), using the input file handle given in
        /// lzxd_init().  This will continue until system->read() returns 0 bytes,
        /// or an error. Errors will be passed out of the function as
        /// MSPACK_ERR_READ errors.  Input streams should convey an "end of input
        /// stream" by refusing to supply all the bytes that LZX asks for when they
        /// reach the end of the stream, rather than return an error code.
        /// 
        /// If any error code other than MSPACK_ERR_OK is returned, the stream
        /// should be considered unusable and lzxd_decompress() should not be
        /// called again on this stream.
        /// </summary>
        /// <param name="lzx">LZX decompression state, as allocated by lzxd_init().</param>
        /// <param name="out_bytes">The number of bytes of data to decompress.</param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public static MSPACK_ERR lzxd_decompress(lzxd_stream lzx, long out_bytes) => MSPACK_ERR.MSPACK_ERR_OK;
    
        /// <summary>
        /// Frees all state associated with an LZX data stream. This will call
        /// system->free() using the system pointer given in lzxd_init().
        /// </summary>
        /// <param name="lzx">LZX decompression state to free.</param>
        public static void lzxd_free(lzxd_stream lzx) { }
    }
}