using static SabreTools.Compression.libmspack.lzx;
using static SabreTools.Compression.libmspack.mszip;

namespace SabreTools.Compression.libmspack
{
    public unsafe class mszipd_stream : readbits
    {
        /// <summary>
        /// inflate() will call this whenever the window should be emptied.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int flush_window(uint val) => 0;

        public int repair_mode { get; set; }

        public int bytes_output { get; set; }

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

        public override void READ_BYTES()
        {
            READ_IF_NEEDED;
            INJECT_BITS_LSB(*i_ptr++, 8);
        }
    }
}