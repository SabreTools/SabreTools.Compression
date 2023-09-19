namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A compressor for the SZDD file format.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_szdd_compressor()"/>
    /// <see cref="mspack_destroy_szdd_compressor()"/>
    public abstract class msszdd_compressor
    {
        /// <summary>
        /// Reads an input file and creates a compressed output file in the
        /// SZDD compressed file format. The SZDD compression format is quick
        /// but gives poor compression. It is possible for the compressed output
        /// file to be larger than the input file.
        ///
        /// Conventionally, SZDD compressed files have the final character in
        /// their filename replaced with an underscore, to show they are
        /// compressed.  The missing character is stored in the compressed file
        /// itself. This is due to the restricted filename conventions of MS-DOS,
        /// most operating systems, such as UNIX, simply append another file
        /// extension to the existing filename. As mspack does not deal with
        /// filenames, this is left up to you. If you wish to set the missing
        /// character stored in the file header, use set_param() with the
        /// #MSSZDDC_PARAM_MISSINGCHAR parameter.
        ///
        /// "Stream" compression (where the length of the input data is not
        /// known) is not possible. The length of the input data is stored in the
        /// header of the SZDD file and must therefore be known before any data
        /// is compressed. Due to technical limitations of the file format, the
        /// maximum size of uncompressed file that will be accepted is 2147483647
        /// bytes.
        /// </summary>
        /// <param name="input">
        /// The name of the file to compressed. This is passed
        /// passed directly to mspack_system::open()
        /// </param>
        /// <param name="output">
        /// The name of the file to write compressed data to.
        /// This is passed directly to mspack_system::open().
        /// </param>
        /// <param name="length">
        /// The length of the uncompressed file, or -1 to indicate
        /// that this should be determined automatically by using
        /// mspack_system::seek() on the input file.
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="set_param(int, int)"/> 
        public abstract MSPACK_ERR compress(in string input, in string output, long length);

        /// <summary>
        /// Sets an SZDD compression engine parameter.
        /// 
        /// The following parameters are defined:
        /// 
        /// - #MSSZDDC_PARAM_CHARACTER: the "missing character", the last character
        ///   in the uncompressed file's filename, which is traditionally replaced
        ///   with an underscore to show the file is compressed. Traditionally,
        ///   this can only be a character that is a valid part of an MS-DOS,
        ///   filename, but libmspack permits any character between 0x00 and 0xFF
        ///   to be stored. 0x00 is the default, and it represents "no character
        ///   stored".
        /// </summary>
        /// <param name="param">The parameter to set</param>
        /// <param name="value">The value to set the parameter to</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        /// <see cref="compress(in string, in string, long)"/> 
        public abstract MSPACK_ERR set_param(MSSZDDC_PARAM param, int value);

        /// <summary>
        /// Returns the error code set by the most recently called method.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="compress(in string, in string, long)"/> 
        public abstract MSPACK_ERR last_error();
    }
}