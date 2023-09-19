namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for .CAB (Microsoft Cabinet) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_cab_decompressor()"/>
    /// <see cref="mspack_destroy_cab_decompressor()"/>
    public abstract class mscab_decompressor
    {
        /// <summary>
        /// Opens a cabinet file and reads its contents.
        /// 
        /// If the file opened is a valid cabinet file, all headers will be read
        /// and a mscabd_cabinet structure will be returned, with a full list of
        /// folders and files.
        /// 
        /// In the case of an error occuring, NULL is returned and the error code
        /// is available from last_error().
        /// 
        /// The filename pointer should be considered "in use" until close() is
        /// called on the cabinet.
        /// </summary>
        /// <param name="filename">
        /// The filename of the cabinet file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mscabd_cabinet structure, or NULL on failure</returns>
        /// <see cref="close(mscabd_cabinet)"/>
        /// <see cref="search(in string)"/>
        /// <see cref="last_error()"/>
        public abstract mscabd_cabinet open(in string filename);

        /// <summary>
        /// Closes a previously opened cabinet or cabinet set.
        /// 
        /// This closes a cabinet, all cabinets associated with it via the
        /// mscabd_cabinet::next, mscabd_cabinet::prevcab and
        /// mscabd_cabinet::nextcab pointers, and all folders and files. All
        /// memory used by these entities is freed.
        /// 
        /// The cabinet pointer is now invalid and cannot be used again. All
        /// mscabd_folder and mscabd_file pointers from that cabinet or cabinet
        /// set are also now invalid, and cannot be used again.
        /// 
        /// If the cabinet pointer given was created using search(), it MUST be
        /// the cabinet pointer returned by search() and not one of the later
        /// cabinet pointers further along the mscabd_cabinet::next chain.
        /// 
        /// If extra cabinets have been added using append() or prepend(), these
        /// will all be freed, even if the cabinet pointer given is not the first
        /// cabinet in the set. Do NOT close() more than one cabinet in the set.
        /// 
        /// The mscabd_cabinet::filename is not freed by the library, as it is
        /// not allocated by the library. The caller should free this itself if
        /// necessary, before it is lost forever.
        /// </summary>
        /// <param name="cab">The cabinet to close</param>
        /// <see cref="open(in string)"/>
        /// <see cref="search(in string)"/>
        /// <see cref="append(mscabd_cabinet, mscabd_cabinet)"/>
        /// <see cref="prepend(mscabd_cabinet, mscabd_cabinet)"/>
        public abstract void close(mscabd_cabinet cab);

        /// <summary>
        /// Searches a regular file for embedded cabinets.
        /// 
        /// This opens a normal file with the given filename and will search the
        /// entire file for embedded cabinet files
        /// 
        /// If any cabinets are found, the equivalent of open() is called on each
        /// potential cabinet file at the offset it was found. All successfully
        /// open()ed cabinets are kept in a list.
        /// 
        /// The first cabinet found will be returned directly as the result of
        /// this method. Any further cabinets found will be chained in a list
        /// using the mscabd_cabinet::next field.
        /// 
        /// In the case of an error occuring anywhere other than the simulated
        /// open(), NULL is returned and the error code is available from
        /// last_error().
        /// 
        /// If no error occurs, but no cabinets can be found in the file, NULL is
        /// returned and last_error() returns MSPACK_ERR_OK.
        /// 
        /// The filename pointer should be considered in use until close() is
        /// called on the cabinet.
        /// 
        /// close() should only be called on the result of search(), not on any
        /// subsequent cabinets in the mscabd_cabinet::next chain.
        /// </summary>
        /// <param name="filename">
        /// The filename of the file to search for cabinets. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mscabd_cabinet structure, or NULL</returns>
        /// <see cref="close(mscabd_cabinet)"/>
        /// <see cref="open(in string)"/>
        /// <see cref="last_error()"/>
        public abstract mscabd_cabinet search(in string filename);

        /// <summary>
        /// Appends one mscabd_cabinet to another, forming or extending a cabinet
        /// set.
        /// 
        /// This will attempt to append one cabinet to another such that
        /// <tt>(cab->nextcab == nextcab) && (nextcab->prevcab == cab)</tt> and
        /// any folders split between the two cabinets are merged.
        /// 
        /// The cabinets MUST be part of a cabinet set -- a cabinet set is a
        /// cabinet that spans more than one physical cabinet file on disk -- and
        /// must be appropriately matched.
        /// 
        /// It can be determined if a cabinet has further parts to load by
        /// examining the mscabd_cabinet::flags field:
        /// 
        /// - if <tt>(flags & MSCAB_HDR_PREVCAB)</tt> is non-zero, there is a
        ///   predecessor cabinet to open() and prepend(). Its MS-DOS
        ///   case-insensitive filename is mscabd_cabinet::prevname
        /// - if <tt>(flags & MSCAB_HDR_NEXTCAB)</tt> is non-zero, there is a
        ///   successor cabinet to open() and append(). Its MS-DOS case-insensitive
        ///   filename is mscabd_cabinet::nextname
        /// 
        /// If the cabinets do not match, an error code will be returned. Neither
        /// cabinet has been altered, and both should be closed seperately.
        /// 
        /// Files and folders in a cabinet set are a single entity. All cabinets
        /// in a set use the same file list, which is updated as cabinets in the
        /// set are added. All pointers to mscabd_folder and mscabd_file
        /// structures in either cabinet must be discarded and re-obtained after
        /// merging.
        /// </summary>
        /// <param name="cab">The cabinet which will be appended to, predecessor of nextcab</param>
        /// <param name="nextcab">The cabinet which will be appended, successor of cab</param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="prepend(mscabd_cabinet, mscabd_cabinet)"/>
        /// <see cref="open(in string)"/>
        /// <see cref="close(mscabd_cabinet)"/>
        public abstract MSPACK_ERR append(mscabd_cabinet cab, mscabd_cabinet nextcab);

        /// <summary>
        /// Prepends one mscabd_cabinet to another, forming or extending a
        /// cabinet set.
        /// 
        /// This will attempt to prepend one cabinet to another, such that
        /// <tt>(cab->prevcab == prevcab) && (prevcab->nextcab == cab)</tt>. In
        /// all other respects, it is identical to append(). See append() for the
        /// full documentation.
        /// </summary>
        /// <param name="cab">The cabinet which will be prepended to, successor of prevcab</param>
        /// <param name="prevcab">The cabinet which will be prepended, predecessor of cab</param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="append(mscabd_cabinet, mscabd_cabinet)"/>
        /// <see cref="open(in string)"/>
        /// <see cref="close(mscabd_cabinet)"/>
        public abstract MSPACK_ERR prepend(mscabd_cabinet cab, mscabd_cabinet prevcab);

        /// <summary>
        /// Extracts a file from a cabinet or cabinet set.
        /// 
        /// This extracts a compressed file in a cabinet and writes it to the given
        /// filename.
        /// 
        /// The MS-DOS filename of the file, mscabd_file::filename, is NOT USED
        /// by extract(). The caller must examine this MS-DOS filename, copy and
        /// change it as necessary, create directories as necessary, and provide
        /// the correct filename as a parameter, which will be passed unchanged
        /// to the decompressor's mspack_system::open()
        /// 
        /// If the file belongs to a split folder in a multi-part cabinet set,
        /// and not enough parts of the cabinet set have been loaded and appended
        /// or prepended, an error will be returned immediately.
        /// </summary>
        /// <param name="file">The file to be decompressed</param>
        /// <param name="filename">The filename of the file being written to</param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public abstract MSPACK_ERR extract(mscabd_file file, in string filename);

        /// <summary>
        /// Sets a CAB decompression engine parameter.
        /// 
        /// The following parameters are defined:
        /// - #MSCABD_PARAM_SEARCHBUF: How many bytes should be allocated as a
        ///   buffer when using search()? The minimum value is 4.  The default
        ///   value is 32768.
        /// - #MSCABD_PARAM_FIXMSZIP: If non-zero, extract() will ignore bad
        ///   checksums and recover from decompression errors in MS-ZIP
        ///   compressed folders. The default value is 0 (don't recover).
        /// - #MSCABD_PARAM_DECOMPBUF: How many bytes should be used as an input
        ///   bit buffer by decompressors? The minimum value is 4. The default
        ///   value is 4096.
        /// </summary>
        /// <param name="param">The parameter to set</param>
        /// <param name="value">The value to set the parameter to</param>
        /// <returns>
        /// MSPACK_ERR_OK if all is OK, or MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        /// <see cref="search(in string)"/>
        /// <see cref="extract(mscabd_file, in string)"/>
        public abstract MSPACK_ERR set_param(MSCABD_PARAM param, int value);

        /// <summary>
        /// Returns the error code set by the most recently called method.
        /// 
        /// This is useful for open() and search(), which do not return an error
        /// code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/>
        /// <see cref="search(in string)"/>
        public abstract MSPACK_ERR last_error();
    }
}