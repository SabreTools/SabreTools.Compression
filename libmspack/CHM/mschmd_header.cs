namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a CHM helpfile.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public unsafe class mschmd_header
    {
        /// <summary>
        /// The version of the CHM file format used in this file.
        /// </summary>
        public uint version { get; set; }

        /// <summary>
        /// The "timestamp" of the CHM helpfile.
        /// 
        /// It is the lower 32 bits of a 64-bit value representing the number of
        /// centiseconds since 1601-01-01 00:00:00 UTC, plus 42. It is not useful
        /// as a timestamp, but it is useful as a semi-unique ID.
        /// </summary>
        public uint timestamp { get; set; }

        /// <summary>
        /// The default Language and Country ID (LCID) of the user who ran the
        /// HTMLHelp Compiler. This is not the language of the CHM file itself.
        /// </summary>
        public uint language { get; set; }

        /// <summary>
        /// The filename of the CHM helpfile. This is given by the library user
        /// and may be in any format.
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// The length of the CHM helpfile, in bytes.
        /// </summary>
        public long length { get; set; }

        /// <summary>
        /// A list of all non-system files in the CHM helpfile.
        /// </summary>
        public mschmd_file files { get; set; }

        /// <summary>
        /// A list of all system files in the CHM helpfile.
        /// 
        /// System files are files which begin with "::". They are meta-files
        /// generated by the CHM creation process.
        /// </summary>
        public mschmd_file sysfiles { get; set; }

        /// <summary>
        /// The section 0 (uncompressed) data in this CHM helpfile.
        /// </summary>
        public mschmd_sec_uncompressed sec0 { get; set; }

        /// <summary>
        /// The section 1 (MSCompressed) data in this CHM helpfile.
        /// </summary>
        public mschmd_sec_mscompressed sec1 { get; set; }

        /// <summary>
        /// The file offset of the first PMGL/PMGI directory chunk.
        /// </summary>
        public long dir_offset { get; set; }

        /// <summary>
        /// The number of PMGL/PMGI directory chunks in this CHM helpfile.
        /// </summary>
        public uint num_chunks { get; set; }

        /// <summary>
        /// The size of each PMGL/PMGI chunk, in bytes.
        /// </summary>
        public uint chunk_size { get; set; }

        /// <summary>
        /// The "density" of the quick-reference section in PMGL/PMGI chunks.
        /// </summary>
        public uint density { get; set; }

        /// <summary>
        /// The depth of the index tree.
        /// 
        /// - if 1, there are no PMGI chunks, only PMGL chunks.
        /// - if 2, there is 1 PMGI chunk. All chunk indices point to PMGL chunks.
        /// - if 3, the root PMGI chunk points to secondary PMGI chunks, which in
        ///   turn point to PMGL chunks.
        /// - and so on...
        /// </summary>
        public uint depth { get; set; }

        /// <summary>
        /// The number of the root PMGI chunk.
        /// 
        /// If there is no index in the CHM helpfile, this will be 0xFFFFFFFF.
        /// </summary>
        public uint index_root { get; set; }

        /// <summary>
        /// The number of the first PMGL chunk. Usually zero.
        /// Available only in CHM decoder version 2 and above.
        /// </summary>
        public uint first_pmgl { get; set; }

        /// <summary>
        /// The number of the last PMGL chunk. Usually num_chunks-1.
        /// Available only in CHM decoder version 2 and above.
        /// </summary>
        public uint last_pmgl { get; set; }

        /// <summary>
        /// A cache of loaded chunks, filled in by mschm_decoder::fast_find().
        /// Available only in CHM decoder version 2 and above.
        /// </summary>
        public FixedArray<byte>[] chunk_cache { get; set; }
    }
}