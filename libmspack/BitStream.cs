namespace SabreTools.Compression.libmspack
{
    public unsafe abstract class BitStream
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
        /// Decompression offset within window
        /// </summary>
        public uint window_posn { get; set; }

        #region I/O buffering

        public byte* inbuf { get; set; }

        public byte* i_ptr { get; set; }

        public byte* i_end { get; set; }

        public byte* o_ptr { get; set; }
        
        public byte* o_end { get; set; }

        public int input_end { get; set; }

        public uint bit_buffer { get; set; }

        public uint bits_left { get; set; }

        public uint inbuf_size { get; set; }

        #endregion

        public MSPACK_ERR error { get; set; }
    }
}