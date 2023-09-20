namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents the LZX compressed section of a CHM helpfile.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public class mschmd_sec_mscompressed : mschmd_section
    {
        /// <summary>
        /// A pointer to the meta-file which represents all LZX compressed data.
        /// </summary>
        public mschmd_file content { get; set; }

        /// <summary>
        /// A pointer to the file which contains the LZX control data.
        /// </summary>
        public mschmd_file control { get; set; }

        /// <summary>
        /// A pointer to the file which contains the LZX reset table.
        /// </summary>
        public mschmd_file rtable { get; set; }

        /// <summary>
        /// A pointer to the file which contains the LZX span information.
        /// Available only in CHM decoder version 2 and above.
        /// </summary>
        public mschmd_file spaninfo { get; set; }
    }
}