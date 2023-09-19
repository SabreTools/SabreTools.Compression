namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A compressor for .CHM (Microsoft HTMLHelp) files.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_chm_compressor()"/> 
    /// <see cref="mspack_destroy_chm_compressor()"/> 
    public abstract class mschm_compressor
    {
        /// <summary>
        /// Generates a CHM help file.
        /// 
        /// The help file will contain up to two sections, an Uncompressed
        /// section and potentially an MSCompressed (LZX compressed)
        /// section.
        /// 
        /// While the contents listing of a CHM file is always in lexical order,
        /// the file list passed in will be taken as the correct order for files
        /// within the sections.  It is in your interest to place similar files
        /// together for better compression.
        /// 
        /// There are two modes of generation, to use a temporary file or not to
        /// use one. See use_temporary_file() for the behaviour of generate() in
        /// these two different modes.
        /// </summary>
        /// <param name="file_list">
        /// An array of mschmc_file structures, terminated
        /// with an entry whose mschmc_file::section field is
        /// #MSCHMC_ENDLIST. The order of the list is
        /// preserved within each section. The length of any
        /// mschmc_file::chm_filename string cannot exceed
        /// roughly 4096 bytes. Each source file must be able
        /// to supply as many bytes as given in the
        /// mschmc_file::length field.
        /// </param>
        /// <param name="output_file">
        /// The file to write the generated CHM helpfile to.
        /// This is passed directly to mspack_system::open()
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="use_temporary_file()"/>
        /// <see cref="set_param()"/>
        public abstract MSPACK_ERR generate(mschmc_file[] file_list, in string output_file);

        /// <summary>
        /// Specifies whether a temporary file is used during CHM generation.
        ///
        /// The CHM file format includes data about the compressed section (such
        /// as its overall size) that is stored in the output CHM file prior to
        /// the compressed section itself. This unavoidably requires that the
        /// compressed section has to be generated, before these details can be
        /// set. There are several ways this can be handled. Firstly, the
        /// compressed section could be generated entirely in memory before
        /// writing any of the output CHM file. This approach is not used in
        /// libmspack, as the compressed section can exceed the addressable
        /// memory space on most architectures.
        ///
        /// libmspack has two options, either to write these unknowable sections
        /// with blank data, generate the compressed section, then re-open the
        /// output file for update once the compressed section has been
        /// completed, or to write the compressed section to a temporary file,
        /// then write the entire output file at once, performing a simple
        /// file-to-file copy for the compressed section.
        ///
        /// The simple solution of buffering the entire compressed section in
        /// memory can still be used, if desired. As the temporary file's
        /// filename is passed directly to mspack_system::open(), it is possible
        /// for a custom mspack_system implementation to hold this file in memory,
        /// without writing to a disk.
        ///
        /// If a temporary file is set, generate() performs the following
        /// sequence of events: the temporary file is opened for writing, the
        /// compression algorithm writes to the temporary file, the temporary
        /// file is closed.  Then the output file is opened for writing and the
        /// temporary file is re-opened for reading. The output file is written
        /// and the temporary file is read from. Both files are then closed. The
        /// temporary file itself is not deleted. If that is desired, the
        /// temporary file should be deleted after the completion of generate(),
        /// if it exists.
        ///
        /// If a temporary file is set not to be used, generate() performs the
        /// following sequence of events: the output file is opened for writing,
        /// then it is written and closed. The output file is then re-opened for
        /// update, the appropriate sections are seek()ed to and re-written, then
        /// the output file is closed.
        /// </summary>
        /// <param name="use_temp_file">
        /// Non-zero if the temporary file should be used,
        /// zero if the temporary file should not be used.
        /// </param>
        /// <param name="temp_file">
        /// A file to temporarily write compressed data to,
        /// before opening it for reading and copying the
        /// contents to the output file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="generate(mschmc_file[], in string)"/>
        public abstract MSPACK_ERR use_temporary_file(int use_temp_file, in string temp_file);

        /// <summary>
        /// Sets a CHM compression engine parameter.
        ///
        /// The following parameters are defined:
        /// 
        /// - #MSCHMC_PARAM_TIMESTAMP: Sets the "timestamp" of the CHM file
        ///   generated. This is not a timestamp, see mschmd_header::timestamp
        ///   for a description. If this timestamp is 0, generate() will use its
        ///   own algorithm for making a unique ID, based on the lengths and
        ///   names of files in the CHM itself. Defaults to 0, any value between
        ///   0 and (2^32)-1 is valid.
        /// - #MSCHMC_PARAM_LANGUAGE: Sets the "language" of the CHM file
        ///   generated.  This is not the language used in the CHM file, but the
        ///   language setting of the user who ran the HTMLHelp compiler. It
        ///   defaults to 0x0409. The valid range is between 0x0000 and 0x7F7F.
        /// - #MSCHMC_PARAM_LZXWINDOW: Sets the size of the LZX history window,
        ///   which is also the interval at which the compressed data stream can be
        ///   randomly accessed. The value is not a size in bytes, but a power of
        ///   two. The default value is 16 (which makes the window 2^16 bytes, or
        ///   64 kilobytes), the valid range is from 15 (32 kilobytes) to 21 (2
        ///   megabytes).
        /// - #MSCHMC_PARAM_DENSITY: Sets the "density" of quick reference
        ///   entries stored at the end of directory listing chunk. Each chunk is
        ///   4096 bytes in size, and contains as many file entries as there is
        ///   room for. At the other end of the chunk, a list of "quick reference"
        ///   pointers is included. The offset of every 'N'th file entry is given a
        ///   quick reference, where N = (2^density) + 1. The default density is
        ///   2. The smallest density is 0 (N=2), the maximum is 10 (N=1025). As
        ///   each file entry requires at least 5 bytes, the maximum number of
        ///   entries in a single chunk is roughly 800, so the maximum value 10
        ///   can be used to indicate there are no quickrefs at all.
        /// - #MSCHMC_PARAM_INDEX: Sets whether or not to include quick lookup
        ///   index chunk(s), in addition to normal directory listing chunks. A
        ///   value of zero means no index chunks will be created, a non-zero value
        ///   means index chunks will be created. The default is zero, "don't
        ///   create an index".
        /// </summary>
        /// <param name="param">The parameter to set</param>
        /// <param name="value">The value to set the parameter to</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        /// <see cref="generate(mschmc_file[], in string)"/>
        public abstract MSPACK_ERR set_param(MSCHMC_PARAM param, int value);

        /// <summary>
        /// Returns the error code set by the most recently called method.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="set_param(int, int)"/> 
        /// <see cref="generate(mschmc_file[], in string)"/>
        public abstract MSPACK_ERR last_error();
    }
}