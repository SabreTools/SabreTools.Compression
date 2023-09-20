namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a section of a CHM helpfile.
    /// 
    /// All fields are READ ONLY.
    /// 
    /// Not used directly, but used as a generic base type for
    /// mschmd_sec_uncompressed and mschmd_sec_mscompressed.
    /// </summary>
    public class mschmd_section
    {
        /// <summary>
        /// A pointer to the CHM helpfile that contains this section.
        /// </summary>
        public mschmd_header chm { get; set; }

        /// <summary>
        /// The section ID. Either 0 for the uncompressed section
        /// mschmd_sec_uncompressed, or 1 for the LZX compressed section
        /// mschmd_sec_mscompressed. No other section IDs are known.
        /// </summary>
        public uint id { get; set; }
    }
}