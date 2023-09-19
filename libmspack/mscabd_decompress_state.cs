using static SabreTools.Compression.libmspack.cab;

namespace SabreTools.Compression.libmspack
{
    public abstract unsafe class mscabd_decompress_state
    {
        /// <summary>
        /// Current folder we're extracting from
        /// </summary>
        public mscabd_folder folder { get; set; }

        /// <summary>
        /// Current folder split we're in
        /// </summary>
        public mscabd_folder_data data { get; set; }

        /// <summary>
        /// Uncompressed offset within folder
        /// </summary>
        public uint offset { get; set; }

        /// <summary>
        /// Which block are we decompressing?
        /// </summary>
        public uint block { get; set; }

        /// <summary>
        /// Cumulative sum of block output sizes
        /// </summary>
        public long outlen { get; set; }

        /// <summary>
        /// Special I/O code for decompressor
        /// </summary>
        public mspack_system sys { get; set; }

        /// <summary>
        /// Type of compression used by folder
        /// </summary>
        public MSCAB_COMP comp_type { get; set; }

        /// <summary>
        /// Decompressor code
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public abstract int decompress(void* data, long offset);

        /// <summary>
        /// Decompressor state
        /// </summary>
        public void* state { get; set; }

        /// <summary>
        /// Cabinet where input data comes from
        /// </summary>
        public mscabd_cabinet incab { get; set; }

        /// <summary>
        /// Input file handle
        /// </summary>
        public mspack_file infh { get; set; }

        /// <summary>
        /// Output file handle
        /// </summary>
        public mspack_file outfh { get; set; }

        /// <summary>
        /// Input data consumed
        /// </summary>
        public byte* i_ptr { get; set; }

        /// <summary>
        /// Input data end
        /// </summary>
        public byte* i_end { get; set; }

        /// <summary>
        /// One input block of data
        /// </summary>
        public byte[] input { get; set; } = new byte[CAB_INPUTBUF];
    }
}