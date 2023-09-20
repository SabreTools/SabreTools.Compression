namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a single cabinet file.
    /// 
    /// All fields are READ ONLY.
    /// 
    /// If this cabinet is part of a merged cabinet set, the #files and #folders
    /// fields are common to all cabinets in the set, and will be identical.
    /// </summary>
    /// <see cref="mscab_decompressor::open()"/> 
    /// <see cref="mscab_decompressor::close()"/> 
    /// <see cref="mscab_decompressor::search()"/> 
    public unsafe class mscabd_cabinet
    {
        /// <summary>
        /// The next cabinet in a chained list, if this cabinet was opened with
        /// mscab_decompressor::search(). May be null to mark the end of the
        /// list.
        /// </summary>
        public mscabd_cabinet next { get; set; }

        /// <summary>
        /// The filename of the cabinet. More correctly, the filename of the
        /// physical file that the cabinet resides in. This is given by the
        /// library user and may be in any format.
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// The file offset of cabinet within the physical file it resides in.
        /// </summary>
        public long base_offset { get; set; }

        /// <summary>
        /// The length of the cabinet file in bytes.
        /// </summary>
        public uint length { get; set; }

        /// <summary>
        /// The previous cabinet in a cabinet set, or null.
        /// </summary>
        public mscabd_cabinet prevcab { get; set; }

        /// <summary>
        /// The next cabinet in a cabinet set, or null.
        /// </summary>
        public mscabd_cabinet nextcab { get; set; }

        /// <summary>
        /// The filename of the previous cabinet in a cabinet set, or null.
        /// </summary>
        public string prevname { get; set; }

        /// <summary>
        /// The filename of the next cabinet in a cabinet set, or null.
        /// </summary>
        public string nextname { get; set; }

        /// <summary>
        /// The name of the disk containing the previous cabinet in a cabinet
        /// set, or null.
        /// </summary>
        public string previnfo { get; set; }

        /// <summary>
        /// The name of the disk containing the next cabinet in a cabinet set,
        /// or null.
        /// </summary>
        public string nextinfo { get; set; }

        /// <summary>
        /// A list of all files in the cabinet or cabinet set.
        /// </summary>
        public mscabd_file files { get; set; }

        /// <summary>
        /// A list of all folders in the cabinet or cabinet set.
        /// </summary>
        public mscabd_folder folders { get; set; }

        /// <summary>
        /// The set ID of the cabinet. All cabinets in the same set should have
        /// the same set ID.
        /// </summary>
        public ushort set_id { get; set; }

        /// <summary>
        /// The index number of the cabinet within the set. Numbering should
        /// start from 0 for the first cabinet in the set, and increment by 1 for
        /// each following cabinet.
        /// </summary>
        public ushort set_index { get; set; }

        /// <summary>
        /// The number of bytes reserved in the header area of the cabinet.
        /// 
        /// If this is non-zero and flags has MSCAB_HDR_RESV set, this data can
        /// be read by the calling application. It is of the given length,
        /// located at offset (base_offset + MSCAB_HDR_RESV_OFFSET) in the
        /// cabinet file.
        /// </summary>
        public ushort header_resv { get; set; }

        /// <summary>
        /// Header flags.
        /// </summary>
        /// <see cref="prevname"/> 
        /// <see cref="previnfo"/> 
        /// <see cref="nextname"/> 
        /// <see cref="nextinfo"/> 
        /// <see cref="header_resv"/> 
        public MSCAB_HDR flags { get; set; }

        /// <summary>
        /// Offset to data blocks
        /// </summary>
        public long blocks_off { get; set; }

        /// <summary>
        /// Reserved space in data blocks
        /// </summary>
        public int block_resv { get; set; }
    }
}