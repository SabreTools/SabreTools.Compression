namespace SabreTools.Compression.libmspack
{
    public static class kwaj
    {
        public const byte kwajh_Signature1 = 0x00;
        public const byte kwajh_Signature2 = 0x04;
        public const byte kwajh_CompMethod = 0x08;
        public const byte kwajh_DataOffset = 0x0a;
        public const byte kwajh_Flags = 0x0c;
        public const byte kwajh_SIZEOF = 0x0e;

        /// <summary>
        /// Input buffer size during decompression - not worth parameterising IMHO
        /// </summary>
        public const int KWAJ_INPUT_SIZE = 2048;

        /// <summary>
        /// Huffman codes that are 9 bits or less are decoded immediately
        /// </summary>
        public const int KWAJ_TABLEBITS = 9;

        // Number of codes in each huffman table
        public const int KWAJ_MATCHLEN1_SYMS = 16;
        public const int KWAJ_MATCHLEN2_SYMS = 16;
        public const int KWAJ_LITLEN_SYMS = 32;
        public const int KWAJ_OFFSET_SYMS = 64;
        public const int KWAJ_LITERAL_SYMS = 256;

        // Define decoding table sizes
        public const int KWAJ_TABLESIZE = 1 << KWAJ_TABLEBITS;
        public const int KWAJ_MATCHLEN1_TBLSIZE = KWAJ_TABLESIZE + (KWAJ_MATCHLEN1_SYMS * 2);
        public const int KWAJ_MATCHLEN2_TBLSIZE = KWAJ_TABLESIZE + (KWAJ_MATCHLEN2_SYMS * 2);
        public const int KWAJ_LITLEN_TBLSIZE = KWAJ_TABLESIZE + (KWAJ_LITLEN_SYMS * 2);
        public const int KWAJ_OFFSET_TBLSIZE = KWAJ_TABLESIZE + (KWAJ_OFFSET_SYMS * 2);
        public const int KWAJ_LITERAL_TBLSIZE = KWAJ_TABLESIZE + (KWAJ_LITERAL_SYMS * 2);
    }
}