namespace SabreTools.Compression.MSZIP
{
    public static class Constants
    {
        /// <summary>
        /// Alphabet for fixed Huffman encoding
        /// </summary>
        public static readonly byte[] FixedAlphabet = new byte[19]
        {
            16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15,
        };

        /// <summary>
        /// Extra bits for length codes 257-285
        /// </summary>
        public static readonly byte[] MatchExtraBits = new byte[29]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
            1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
            4, 4, 4, 4, 5, 5, 5, 5, 0,
        };

        /// <summary>
        /// Initial lengths for length codes 257-285
        /// </summary>
        public static readonly ushort[] MatchLengths = new ushort[29]
        {
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
            15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
            67, 83, 99, 115, 131, 163, 195, 227, 258,
        };

        /// <summary>
        /// Extra bits for distance codes 0-29
        /// </summary>
        public static readonly byte[] DistanceExtraBits = new byte[30]
        {
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
            4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
            9, 9, 10, 10, 11, 11, 12, 12, 13, 13,
        };

        /// <summary>
        /// Initial lengths for distance codes 0-29
        /// </summary>
        public static readonly ushort[] DistanceLengths = new ushort[30]
        {
            1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
            33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
            1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
        };
    }
}