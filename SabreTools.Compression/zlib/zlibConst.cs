namespace SabreTools.Compression.zlib
{
    public static class zlibConst
    {
        public const int Z_NO_FLUSH = 0;
        public const int Z_PARTIAL_FLUSH = 1;
        public const int Z_SYNC_FLUSH = 2;
        public const int Z_FULL_FLUSH = 3;
        public const int Z_FINISH = 4;
        public const int Z_BLOCK = 5;
        public const int Z_TREES = 6;

        public const int Z_OK = 0;
        public const int Z_STREAM_END = 1;
        public const int Z_NEED_DICT = 2;
        public const int Z_ERRNO = (-1);
        public const int Z_STREAM_ERROR = (-2);
        public const int Z_DATA_ERROR = (-3);
        public const int Z_MEM_ERROR = (-4);
        public const int Z_BUF_ERROR = (-5);
        public const int Z_VERSION_ERROR = (-6);

        /// <summary>
        /// Get the zlib result name from an integer
        /// </summary>
        /// <param name="result">Integer to translate to the result name</param>
        /// <returns>Name of the result, the integer as a string otherwise</returns>
        public static string ToZlibConstName(this int result)
        {
            return result switch
            {
                0 => "Z_OK",
                1 => "Z_STREAM_END",
                2 => "Z_NEED_DICT",

                -1 => "Z_ERRNO",
                -2 => "Z_STREAM_ERROR",
                -3 => "Z_DATA_ERROR",
                -4 => "Z_MEM_ERROR",
                -5 => "Z_BUF_ERROR",
                -6 => "Z_VERSION_ERROR",

                _ => result.ToString(),
            };
        }
    }
}