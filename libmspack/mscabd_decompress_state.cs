using SabreTools.Compression.libmspack.CAB;
using static SabreTools.Compression.libmspack.CAB.Constants;

namespace SabreTools.Compression.libmspack
{
    public unsafe class mscabd_decompress_state
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
        public CABSystem sys { get; set; }

        /// <summary>
        /// Type of compression used by folder
        /// </summary>
        public MSCAB_COMP comp_type { get; set; }

        /// <summary>
        /// Decompressor code
        /// </summary>
        public virtual MSPACK_ERR decompress(object data, long offset) => MSPACK_ERR.MSPACK_ERR_NOMEMORY;

        /// <summary>
        /// Decompressor state
        /// </summary>
        public object state { get; set; }

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

    public unsafe class mscabd_noned_decompress_state : mscabd_decompress_state
    {
        /// <inheritdoc/>
        public override unsafe MSPACK_ERR decompress(object data, long bytes)
        {
            noned_state s = data as noned_state;
            while (bytes > 0)
            {
                int run = (bytes > s.bufsize) ? s.bufsize : (int)bytes;
                {
                    if (s.sys.read(s.i, &s.buf[0], run) != run) return MSPACK_ERR.MSPACK_ERR_READ;
                    if (s.sys.write(s.o, &s.buf[0], run) != run) return MSPACK_ERR.MSPACK_ERR_WRITE;
                    bytes -= run;
                }
                return MSPACK_ERR.MSPACK_ERR_OK;
            }

            return MSPACK_ERR.MSPACK_ERR_DECRUNCH;
        }
    }
}