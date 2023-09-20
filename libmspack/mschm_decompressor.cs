namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for .CHM (Microsoft HTMLHelp) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_chm_decompressor()"/>
    /// <see cref="mspack_destroy_chm_decompressor()"/>
    public abstract class mschm_decompressor : BaseDecompressor
    {
        public mschmd_decompress_state d { get; set; }

        /// <summary>
        /// Opens a CHM helpfile and reads its contents.
        ///
        /// If the file opened is a valid CHM helpfile, all headers will be read
        /// and a mschmd_header structure will be returned, with a full list of
        /// files.
        ///
        /// In the case of an error occuring, null is returned and the error code
        /// is available from last_error().
        ///
        /// The filename pointer should be considered "in use" until close() is
        /// called on the CHM helpfile.
        /// </summary>
        /// <param name="filename">
        /// The filename of the CHM helpfile. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mschmd_header structure, or null on failure</returns>
        /// <see cref="close(mschmd_header)"/>
        public abstract mschmd_header open(in string filename);

        /// <summary>
        /// Closes a previously opened CHM helpfile.
        ///
        /// This closes a CHM helpfile, frees the mschmd_header and all
        /// mschmd_file structures associated with it (if any). This works on
        /// both helpfiles opened with open() and helpfiles opened with
        /// fast_open().
        ///
        /// The CHM header pointer is now invalid and cannot be used again. All
        /// mschmd_file pointers referencing that CHM are also now invalid, and
        /// cannot be used again.
        /// </summary>
        /// <param name="chm">The CHM helpfile to close</param>
        /// <see cref="open(in string)"/>
        /// <see cref="fast_open(in string)"/>
        public abstract void close(mschmd_header chm);

        /// <summary>
        /// Extracts a file from a CHM helpfile.
        ///
        /// This extracts a file from a CHM helpfile and writes it to the given
        /// filename. The filename of the file, mscabd_file::filename, is not
        /// used by extract(), but can be used by the caller as a guide for
        /// constructing an appropriate filename.
        ///
        /// This method works both with files found in the mschmd_header::files
        /// and mschmd_header::sysfiles list and mschmd_file structures generated
        /// on the fly by fast_find().
        /// </summary>
        /// <param name="file">The file to be decompressed</param>
        /// <param name="filename">The filename of the file being written to</param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR extract(mschmd_file file, in string filename);

        /// <summary>
        /// Returns the error code set by the most recently called method.
        ///
        /// This is useful for open() and fast_open(), which do not return an
        /// error code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/>
        /// <see cref="extract(mschmd_file, in string)"/>
        public abstract MSPACK_ERR last_error();

        /// <summary>
        /// Opens a CHM helpfile quickly.
        ///
        /// If the file opened is a valid CHM helpfile, only essential headers
        /// will be read. A mschmd_header structure will be still be returned, as
        /// with open(), but the mschmd_header::files field will be null. No
        /// files details will be automatically read.  The fast_find() method
        /// must be used to obtain file details.
        ///
        /// In the case of an error occuring, null is returned and the error code
        /// is available from last_error().
        ///
        /// The filename pointer should be considered "in use" until close() is
        /// called on the CHM helpfile.
        /// </summary>
        /// <param name="filename">
        /// The filename of the CHM helpfile. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mschmd_header structure, or null on failure</returns>
        /// <see cref="open(in string)"/> 
        /// <see cref="close(mschmd_header)"/> 
        /// <see cref="fast_find(mschmd_header, in string, mschmd_file, int)"/> 
        /// <see cref="extract(mschmd_file, in string)"/>
        public abstract mschmd_header fast_open(in string filename);

        /// <summary>
        /// Finds file details quickly.
        ///
        /// Instead of reading all CHM helpfile headers and building a list of
        /// files, fast_open() and fast_find() are intended for finding file
        /// details only when they are needed. The CHM file format includes an
        /// on-disk file index to allow this.
        ///
        /// Given a case-sensitive filename, fast_find() will search the on-disk
        /// index for that file.
        ///
        /// If the file was found, the caller-provided mschmd_file structure will
        /// be filled out like so:
        /// - section: the correct value for the found file
        /// - offset: the correct value for the found file
        /// - length: the correct value for the found file
        /// - all other structure elements: null or 0
        ///
        /// If the file was not found, MSPACK_ERR_OK will still be returned as the
        /// result, but the caller-provided structure will be filled out like so:
        /// - section: null
        /// - offset: 0
        /// - length: 0
        /// - all other structure elements: null or 0
        ///
        /// This method is intended to be used in conjunction with CHM helpfiles
        /// opened with fast_open(), but it also works with helpfiles opened
        /// using the regular open().
        /// </summary>
        /// <param name="chm">The CHM helpfile to search for the file</param>
        /// <param name="filename">The filename of the file to search for</param>
        /// <param name="f_ptr">A pointer to a caller-provded mschmd_file structure</param>
        /// <param name="f_size"><tt>sizeof(struct mschmd_file)</tt></param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="open(in string)"/> 
        /// <see cref="close(mschmd_header)"/> 
        /// <see cref="fast_find(mschmd_header, in string, mschmd_file, int)"/> 
        /// <see cref="extract(mschmd_file, in string)"/>
        public abstract MSPACK_ERR fast_find(mschmd_header chm, in string filename, mschmd_file f_ptr, int f_size);
    }
}