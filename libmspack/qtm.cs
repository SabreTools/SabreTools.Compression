namespace SabreTools.Compression.libmspack
{
    public static class qtm
    {
        public const int QTM_FRAME_SIZE = 32768;

        /// <summary>
        /// Allocates Quantum decompression state for decoding the given stream.
        ///
        /// - returns NULL if window_bits is outwith the range 10 to 21 (inclusive).
        ///
        /// - uses system->alloc() to allocate memory
        ///
        /// - returns NULL if not enough memory
        ///
        /// - window_bits is the size of the Quantum window, from 1Kb (10) to 2Mb (21).
        ///
        /// - input_buffer_size is the number of bytes to use to store bitstream data.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="window_bits"></param>
        /// <param name="input_buffer_size"></param>
        /// <returns></returns>
        public static qtmd_stream qtmd_init(mspack_system system, mspack_file input, mspack_file output, int window_bits, int input_buffer_size) => null;
    
        /// <summary>
        /// Decompresses, or decompresses more of, a Quantum stream.
        ///
        /// - out_bytes of data will be decompressed and the function will return
        ///   with an MSPACK_ERR_OK return code.
        ///
        /// - decompressing will stop as soon as out_bytes is reached. if the true
        ///   amount of bytes decoded spills over that amount, they will be kept for
        ///   a later invocation of qtmd_decompress().
        ///
        /// - the output bytes will be passed to the system->write() function given in
        ///   qtmd_init(), using the output file handle given in qtmd_init(). More
        ///   than one call may be made to system->write()
        ///
        /// - Quantum will read input bytes as necessary using the system->read()
        ///   function given in qtmd_init(), using the input file handle given in
        ///   qtmd_init(). This will continue until system->read() returns 0 bytes,
        ///   or an error.
        /// </summary>
        /// <param name="qtm"></param>
        /// <param name="out_bytes"></param>
        /// <returns></returns>
        public static MSPACK_ERR qtmd_decompress(qtmd_stream qtm, long out_bytes) => MSPACK_ERR.MSPACK_ERR_OK;

        /// <summary>
        /// Frees all state associated with a Quantum data stream
        ///
        /// - calls system->free() using the system pointer given in qtmd_init()
        /// </summary>
        /// <param name="qtm"></param>
        public static void qtmd_free(qtmd_stream qtm) { }
    }
}