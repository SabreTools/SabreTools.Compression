namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents the uncompressed section of a CHM helpfile.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public class mschmd_sec_uncompressed : mschmd_section
    {
        /// <summary>
        /// The file offset of where this section begins in the CHM helpfile.
        /// </summary>
        public long offset { get; set; }
    }
}