using static SabreTools.Compression.libmspack.KWAJ.Constants;
using static SabreTools.Compression.libmspack.lzss;

namespace SabreTools.Compression.libmspack
{
    public unsafe class kwajd_stream : readbits
    {
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

        public new byte[] inbuf { get; set; } = new byte[KWAJ_INPUT_SIZE];

        #endregion

        #region History window

        public byte[] window { get; set; } = new byte[LZSS_WINDOW_SIZE];

        #endregion

        public override void READ_BYTES()
        {
            if (i_ptr >= i_end)
            {
                if ((err = lzh_read_input(lzh)))
                    return err;
                i_ptr = lzh.i_ptr;
                i_end = lzh.i_end;
            }
            INJECT_BITS_MSB(*i_ptr++, 8);
        }
    }
}