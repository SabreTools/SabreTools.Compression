namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a single folder in a cabinet or cabinet set.
    /// 
    /// All fields are READ ONLY.
    /// 
    /// A folder is a single compressed stream of data. When uncompressed, it
    /// holds the data of one or more files. A folder may be split across more
    /// than one cabinet.
    /// </summary>
    public class mscabd_folder
    {
        /// <summary>
        /// A pointer to the next folder in this cabinet or cabinet set, or NULL
        /// if this is the final folder.
        /// </summary>
        public mscabd_folder next { get; set; }

        /// <summary>
        /// The compression format used by this folder.
        /// 
        /// The macro MSCABD_COMP_METHOD() should be used on this field to get
        /// the algorithm used. The macro MSCABD_COMP_LEVEL() should be used to get
        /// the "compression level".
        /// </summary>
        /// <see cref="MSCABD_COMP_METHOD()"/> 
        /// <see cref="MSCABD_COMP_LEVEL()"/> 
        public MSCAB_COMP comp_type { get; set; }

        /// <summary>
        /// The total number of data blocks used by this folder. This includes
        /// data blocks present in other files, if this folder spans more than
        /// one cabinet.
        /// </summary>
        public uint num_blocks { get; set; }

        /// <summary>
        /// Where are the data blocks?
        /// </summary>
        public mscabd_folder_data data { get; set; }

        /// <summary>
        /// First file needing backwards merge
        /// </summary>
        public mscabd_file merge_prev { get; set; }

        /// <summary>
        /// First file needing forwards merge
        /// </summary>
        public mscabd_file merge_next { get; set; }
    }
}