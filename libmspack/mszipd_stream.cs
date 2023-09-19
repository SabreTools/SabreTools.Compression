using static SabreTools.Compression.libmspack.lzx;
using static SabreTools.Compression.libmspack.mszip;

namespace SabreTools.Compression.libmspack
{
    public unsafe class mszipd_stream
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
        /// Offset within window
        /// </summary>
        public uint window_posn { get; set; }

        /// <summary>
        /// inflate() will call this whenever the window should be emptied.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int flush_window(uint val) => 0;

        public MSPACK_ERR error { get; set; }

        public int repair_mode { get; set; }

        public int bytes_output { get; set; }

        #region I/O buffering

        public byte* inbuf { get; set; }

        public byte* i_ptr { get; set; }

        public byte* i_end { get; set; }

        public byte* o_ptr { get; set; }

        public byte* o_end { get; set; }

        public byte input_end { get; set; }

        public uint bit_buffer { get; set; }

        public uint bits_left { get; set; }

        public uint inbuf_size { get; set; }

        #endregion

        #region Huffman code lengths

        public byte[] LITERAL_len { get; set; } = new byte[MSZIP_LITERAL_MAXSYMBOLS + LZX_LENTABLE_SAFETY];

        public byte[] DISTANCE_len { get; set; } = new byte[MSZIP_DISTANCE_MAXSYMBOLS + LZX_LENTABLE_SAFETY];

        #endregion

        #region Huffman decoding tables

        public ushort[] LITERAL_table { get; set; } = new ushort[(1 << MSZIP_LITERAL_TABLESIZE) + (LZX_PRETREE_MAXSYMBOLS * 2)];

        public ushort[] DISTANCE_table { get; set; } = new ushort[(1 << MSZIP_DISTANCE_TABLESIZE) + (LZX_MAINTREE_MAXSYMBOLS * 2)];

        #endregion

        /// <summary>
        /// 32kb history window
        /// </summary>
        public byte[] window { get; set; } = new byte[MSZIP_FRAME_SIZE];
    }
}