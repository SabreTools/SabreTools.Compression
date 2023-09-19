namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a file to be placed in a CHM helpfile.
    /// 
    /// A contiguous array of these structures should be passed to
    /// mschm_compressor::generate(). The array list is terminated with an
    /// entry whose mschmc_file::section field is set to #MSCHMC_ENDLIST, the
    /// other fields in this entry are ignored.
    /// </summary>
    public class mschmc_file
    {
        /// <summary>
        /// One of <see cref="MSCHMC"/> values.
        /// </summary>
        public MSCHMC section { get; set; }

        /// <summary>
        /// The filename of the source file that will be added to the CHM. This
        /// is passed directly to mspack_system::open().
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// The full path and filename of the file within the CHM helpfile, a
        /// UTF-1 encoded null-terminated string.
        /// </summary>
        public string chm_filename { get; set; }

        /// <summary>
        /// The length of the file, in bytes. This will be adhered to strictly
        /// and a read error will be issued if this many bytes cannot be read
        /// from the real file at CHM generation time.
        /// </summary>
        public long length { get; set; }
    }
}