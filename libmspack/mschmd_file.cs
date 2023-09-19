namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a file stored in a CHM helpfile.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public class mschmd_file
    {
        /// <summary>
        /// A pointer to the next file in the list, or NULL if this is the final
        /// file.
        /// </summary>
        public mschmd_file next { get; set; }

        /// <summary>
        /// A pointer to the section that this file is located in. Indirectly,
        /// it also points to the CHM helpfile the file is located in.
        /// </summary>
        public mschmd_section section { get; set; }

        /// <summary>
        /// The offset within the section data that this file is located at.
        /// </summary>
        public long offset { get; set; }

        /// <summary>
        /// The length of this file, in bytes
        /// </summary>
        public long length { get; set; }

        /// <summary>
        /// The filename of this file -- a null terminated string in UTF-8.
        /// </summary>
        public string filename { get; set; }
    }
}