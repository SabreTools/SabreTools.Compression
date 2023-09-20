namespace SabreTools.Compression.libmspack
{
    public class mschmd_decompress_state
    {
        /// <summary>
        /// CHM file being decompressed
        /// </summary>
        public mschmd_header chm { get; set; }

        /// <summary>
        /// Uncompressed length of LZX stream
        /// </summary>
        public long length { get; set; }

        /// <summary>
        /// Uncompressed offset within stream
        /// </summary>
        public long offset { get; set; }

        /// <summary>
        /// Offset in input file
        /// </summary>
        public long inoffset { get; set; }

        /// <summary>
        /// LZX decompressor state
        /// </summary>
        public lzxd_stream state { get; set; }

        /// <summary>
        /// Special I/O code for decompressor
        /// </summary>
        public mspack_system sys { get; set; }

        /// <summary>
        /// Input file handle
        /// </summary>
        public mspack_file infh { get; set; }

        /// <summary>
        /// Output file handle
        /// </summary>
        public mspack_file outfh { get; set; }
    }
}