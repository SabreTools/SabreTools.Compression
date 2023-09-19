namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents an KWAJ compressed file.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public class mskwajd_header
    {
        /// <summary>
        /// The compression type
        /// </summary>
        public MSKWAJ_COMP comp_type { get; set; }

        /// <summary>
        /// The offset in the file where the compressed data stream begins
        /// </summary>
        public long data_offset { get; set; }

        /// <summary>
        /// Flags indicating which optional headers were included.
        /// </summary>
        public MSKWAJ_HDR headers { get; set; }

        /// <summary>
        /// The amount of uncompressed data in the file, or 0 if not present.
        /// </summary>
        public long length { get; set; }

        /// <summary>
        /// Output filename, or NULL if not present
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// Extra uncompressed data (usually text) in the header.
        /// This data can contain nulls so use extra_length to get the size.
        /// </summary>
        public string extra { get; set; }

        /// <summary>
        /// Length of extra uncompressed data in the header
        /// </summary>
        public ushort extra_length { get; set; }
    }
}