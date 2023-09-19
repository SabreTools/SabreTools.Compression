namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents an SZDD compressed file.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public class msszddd_header
    {
        /// <summary>
        /// The file format
        /// </summary>
        public MSSZDD_FMT format { get; set; }

        /// <summary>
        /// The amount of data in the SZDD file once uncompressed.
        /// </summary>
        public long length { get; set; }

        /// <summary>
        /// The last character in the filename, traditionally replaced with an
        /// underscore to show the file is compressed. The null character is used
        /// to show that this character has not been stored (e.g. because the
        /// filename is not known). Generally, only characters that may appear in
        /// an MS-DOS filename (except ".") are valid.
        /// </summary>
        public char missing_char { get; set; }
    }
}