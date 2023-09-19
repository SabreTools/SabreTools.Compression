using static SabreTools.Compression.libmspack.lzx;

namespace SabreTools.Compression.libmspack
{
    public unsafe class lzxd_stream
    {
        /// <summary>
        /// I/O routines
        /// </summary>
        public mspack_system sys { get; set; }

        /// <summary>
        /// Input file handle
        /// </summary>
        public mspack_file input { get; set; }

        /// <summary>
        /// Output file handle
        /// </summary>
        public mspack_file output { get; set; }

        /// <summary>
        /// Number of bytes actually output
        /// </summary>
        public long offset { get; set; }

        /// <summary>
        /// Overall decompressed length of stream
        /// </summary>
        public long length { get; set; }

        /// <summary>
        /// Decoding window
        /// </summary>
        public byte* window { get; set; }

        /// <summary>
        /// Window size
        /// </summary>
        public uint window_size { get; set; }

        /// <summary>
        /// LZX DELTA reference data size
        /// </summary>
        public uint ref_data_size { get; set; }

        /// <summary>
        /// Number of match_offset entries in table
        /// </summary>
        public uint num_offsets { get; set; }

        /// <summary>
        /// Decompression offset within window
        /// </summary>
        public uint window_posn { get; set; }

        /// <summary>
        /// Current frame offset within in window
        /// </summary>
        public uint frame_posn { get; set; }

        /// <summary>
        /// The number of 32kb frames processed
        /// </summary>
        public uint frame { get; set; }

        /// <summary>
        /// Which frame do we reset the compressor?
        /// </summary>
        public uint reset_interval { get; set; }

        /// <summary>
        /// For the LRU offset system
        /// </summary>
        public uint R0 { get; set; }

        /// <summary>
        /// For the LRU offset system
        /// </summary>
        public uint R1 { get; set; }

        /// <summary>
        /// For the LRU offset system
        /// </summary>
        public uint R2 { get; set; }

        /// <summary>
        /// Uncompressed length of this LZX block
        /// </summary>
        public uint block_length { get; set; }

        /// <summary>
        /// Uncompressed bytes still left to decode
        /// </summary>
        public uint block_remaining { get; set; }

        /// <summary>
        /// Magic header value used for transform
        /// </summary>
        public int intel_filesize { get; set; }

        /// <summary>
        /// Has intel E8 decoding started?
        /// </summary>
        public byte intel_started { get; set; }

        /// <summary>
        /// Type of the current block
        /// </summary>
        public byte block_type { get; set; }

        /// <summary>
        /// Have we started decoding at all yet?
        /// </summary>
        public byte header_read { get; set; }

        /// <summary>
        /// Have we reached the end of input?
        /// </summary>
        public byte input_end { get; set; }

        /// <summary>
        /// Does stream follow LZX DELTA spec?
        /// </summary>
        public byte is_delta { get; set; }

        public MSPACK_ERR error { get; set; }

        #region I/O buffering

        public byte* inbuf { get; set; }

        public byte* i_ptr { get; set; }

        public byte* i_end { get; set; }

        public byte* o_ptr { get; set; }

        public byte* o_end { get; set; }

        public uint bit_buffer { get; set; }

        public uint bits_left { get; set; }

        public uint inbuf_size { get; set; }

        #endregion

        #region Huffman code lengths

        public byte[] PRETREE_len { get; set; } = new byte[LZX_PRETREE_MAXSYMBOLS + LZX_LENTABLE_SAFETY];

        public byte[] MAINTREE_len { get; set; } = new byte[LZX_MAINTREE_MAXSYMBOLS + LZX_LENTABLE_SAFETY];

        public byte[] LENGTH_len { get; set; } = new byte[LZX_LENGTH_MAXSYMBOLS + LZX_LENTABLE_SAFETY];

        public byte[] ALIGNED_len { get; set; } = new byte[LZX_ALIGNED_MAXSYMBOLS + LZX_LENTABLE_SAFETY];

        #endregion

        #region Huffman decoding tables

        public ushort[] PRETREE_table { get; set; } = new ushort[(1 << LZX_PRETREE_TABLEBITS) + (LZX_PRETREE_MAXSYMBOLS * 2)];

        public ushort[] MAINTREE_table { get; set; } = new ushort[(1 << LZX_MAINTREE_TABLEBITS) + (LZX_MAINTREE_MAXSYMBOLS * 2)];

        public ushort[] LENGTH_table { get; set; } = new ushort[(1 << LZX_LENGTH_TABLEBITS) + (LZX_LENGTH_MAXSYMBOLS * 2)];

        public ushort[] ALIGNED_table { get; set; } = new ushort[(1 << LZX_ALIGNED_TABLEBITS) + (LZX_ALIGNED_MAXSYMBOLS * 2)];

        public byte LENGTH_empty { get; set; }

        #endregion

        /// <summary>
        /// This is used purely for doing the intel E8 transform
        /// </summary>
        public byte[] e8_buf { get; set; } = new byte[LZX_FRAME_SIZE];
    }
}