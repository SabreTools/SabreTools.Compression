using static SabreTools.Compression.libmspack.kwaj;
using static SabreTools.Compression.libmspack.lzss;

namespace SabreTools.Compression.libmspack
{
    public unsafe class kwajd_stream
    {
        #region I/O buffering

        public mspack_system sys { get; set; }

        public mspack_file input { get; set; }

        public mspack_file output { get; set; }

        public byte* i_ptr { get; set; }

        public byte* i_end { get; set; }

        public uint bit_buffer { get; set; }

        public uint bits_left { get; set; }

        public int input_end { get; set; }

        #endregion

        #region Huffman code lengths

        public byte[] MATCHLEN1_len { get; set; } = new byte[KWAJ_MATCHLEN1_SYMS];

        public byte[] MATCHLEN2_len { get; set; } = new byte[KWAJ_MATCHLEN2_SYMS];

        public byte[] LITLEN_len { get; set; } = new byte[KWAJ_LITLEN_SYMS];

        public byte[] OFFSET_len { get; set; } = new byte[KWAJ_OFFSET_SYMS];

        public byte[] LITERAL_len { get; set; } = new byte[KWAJ_LITERAL_SYMS];

        #endregion

        #region Huffman decoding tables

        public ushort[] MATCHLEN1_table { get; set; } = new ushort[KWAJ_MATCHLEN1_TBLSIZE];

        public ushort[] MATCHLEN2_table { get; set; } = new ushort[KWAJ_MATCHLEN2_TBLSIZE];

        public ushort[] LITLEN_table { get; set; } = new ushort[KWAJ_LITLEN_TBLSIZE];

        public ushort[] OFFSET_table { get; set; } = new ushort[KWAJ_OFFSET_TBLSIZE];

        public ushort[] LITERAL_table { get; set; } = new ushort[KWAJ_LITERAL_TBLSIZE];

        #endregion

        #region Input buffer

        public byte[] inbuf { get; set; } = new byte[KWAJ_INPUT_SIZE];

        #endregion

        #region History window

        public byte[] window { get; set; } = new byte[LZSS_WINDOW_SIZE];

        #endregion
    }
}