namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A compressor for the KWAJ file format.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_kwaj_compressor()"/>
    /// <see cref="mspack_destroy_kwaj_compressor()"/>
    public unsafe abstract class mskwaj_compressor : Compressor
    {
        public mspack_system system { get; set; }

        public int[] param { get; set; } = new int[2];

        public MSPACK_ERR error { get; set; }

        /// <summary>
        /// Reads an input file and creates a compressed output file in the
        /// KWAJ compressed file format. The KWAJ compression format is quick
        /// but gives poor compression. It is possible for the compressed output
        /// file to be larger than the input file.
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
        /// <see cref="set_param(int, int)" />
        public abstract MSPACK_ERR compress(in string input, in string output, long length);

        /// <summary>
        /// Sets an KWAJ compression engine parameter.
        ///
        /// The following parameters are defined:
        ///
        /// - #MSKWAJC_PARAM_COMP_TYPE: the compression method to use. Must
        ///   be one of #MSKWAJC_COMP_NONE, #MSKWAJC_COMP_XOR, #MSKWAJ_COMP_SZDD
        ///   or #MSKWAJ_COMP_LZH. The default is #MSKWAJ_COMP_LZH.
        ///
        /// - #MSKWAJC_PARAM_INCLUDE_LENGTH: a boolean; should the compressed
        ///   output file should include the uncompressed length of the input
        ///   file in the header? This adds 4 bytes to the size of the output
        ///   file. A value of zero says "no", non-zero says "yes". The default
        ///   is "no".
        /// </summary>
        /// <param name="param">The parameter to set</param>
        /// <param name="value">The value to set the parameter to</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        /// <see cref="generate()"/> 
        public abstract MSPACK_ERR set_param(MSKWAJC_PARAM param, int value);

        /// <summary>
        /// Sets the original filename of the file before compression,
        /// which will be stored in the header of the output file.
        ///
        /// The filename should be a null-terminated string, it must be an
        /// MS-DOS "8.3" type filename (up to 8 bytes for the filename, then
        /// optionally a "." and up to 3 bytes for a filename extension).
        ///
        /// If null is passed as the filename, no filename is included in the
        /// header. This is the default.
        /// </summary>
        /// <param name="filename">The original filename to use</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS if the
        /// filename is too long
        /// </returns>
        public abstract MSPACK_ERR set_filename(in string filename);

        /// <summary>
        /// Sets arbitrary data that will be stored in the header of the
        /// output file, uncompressed. It can be up to roughly 64 kilobytes,
        /// as the overall size of the header must not exceed 65535 bytes.
        /// The data can contain null bytes if desired.
        ///
        /// If null is passed as the data pointer, or zero is passed as the
        /// length, no extra data is included in the header. This is the
        /// default.
        /// </summary>
        /// <param name="data">A pointer to the data to be stored in the header</param>
        /// <param name="bytes">the length of the data in bytes</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS extra data
        /// is too long
        /// </returns>
        public abstract MSPACK_ERR set_extra_data(void* data, int bytes);

        /// <summary>
        /// Returns the error code set by the most recently called method.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="compress(in string, in string, long)"/> 
        public abstract MSPACK_ERR last_error();
    }
}