namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which abstracts file I/O and memory management.
    /// 
    /// The library always uses the mspack_system structure for interaction
    /// with the file system and to allocate, free and copy all memory. It also
    /// uses it to send literal messages to the library user.
    /// 
    /// When the library is compiled normally, passing NULL to a compressor or
    /// decompressor constructor will result in a default mspack_system being
    /// used, where all methods are implemented with the standard C library.
    /// 
    /// However, all constructors support being given a custom created
    /// mspack_system structure, with the library user's own methods. This
    /// allows for more abstract interaction, such as reading and writing files
    /// directly to memory, or from a network socket or pipe.
    /// 
    /// Implementors of an mspack_system structure should read all
    /// documentation entries for every structure member, and write methods
    /// which conform to those standards.
    /// </summary>
    public unsafe abstract class mspack_system
    {
        /// <summary>
        /// Opens a file for reading, writing, appending or updating
        /// </summary>
        /// <param name="filename">
        /// The file to be opened. It is passed directly from the
        /// library caller without being modified, so it is up to
        /// the caller what this parameter actually represents.
        /// </param>
        /// <param name="mode">One of <see cref="MSPACK_SYS_OPEN"/> values</param>
        /// <returns>
        /// A pointer to a mspack_file structure. This structure officially
        /// contains no members, its true contents are up to the
        /// mspack_system implementor. It should contain whatever is needed
        /// for other mspack_system methods to operate. Returning the NULL
        /// pointer indicates an error condition.
        /// </returns>
        /// <see cref="close(mspack_file)"/> 
        /// <see cref="read(mspack_file, void*, int)"/> 
        /// <see cref="write(mspack_file, void*, int)"/> 
        /// <see cref="seek(mspack_file, int, MSPACK_SYS_SEEK)"/> 
        /// <see cref="tell(mspack_file)"/> 
        /// <see cref="message(mspack_file, in string, string[])"/> 
        public abstract mspack_file open(in string filename, MSPACK_SYS_OPEN mode);

        /// <summary>
        /// Closes a previously opened file. If any memory was allocated for this
        /// particular file handle, it should be freed at this time.
        /// </summary>
        /// <param name="file">The file to close</param>
        /// <see cref="open(in string, MSPACK_SYS_OPEN)"/> 
        public abstract void close(mspack_file file);

        /// <summary>
        /// Reads a given number of bytes from an open file.
        /// </summary>
        /// <param name="file">The file to read from</param>
        /// <param name="buffer">The location where the read bytes should be stored</param>
        /// <param name="bytes">The number of bytes to read from the file</param>
        /// <returns>
        /// The number of bytes successfully read (this can be less than
        /// the number requested), zero to mark the end of file, or less
        /// than zero to indicate an error. The library does not "retry"
        /// reads and assumes short reads are due to EOF, so you should
        /// avoid returning short reads because of transient errors.
        /// </returns>
        /// <see cref="open(in string, MSPACK_SYS_OPEN)"/> 
        /// <see cref="write(mspack_file, void*, int)"/> 
        public abstract int read(mspack_file file, void* buffer, int bytes);

        /// <summary>
        /// Writes a given number of bytes to an open file.
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="buffer">The location where the written bytes should be read from</param>
        /// <param name="bytes">The number of bytes to write to the file</param>
        /// <returns>
        /// The number of bytes successfully written, this can be less
        /// than the number requested. Zero or less can indicate an error
        /// where no bytes at all could be written. All cases where less
        /// bytes were written than requested are considered by the library
        /// to be an error.
        /// </returns>
        /// <see cref="open(in string, MSPACK_SYS_OPEN)"/> 
        /// <see cref="read(mspack_file, void*, int)"/> 
        public abstract int write(mspack_file file, void* buffer, int bytes);

        /// <summary>
        /// Seeks to a specific file offset within an open file.
        /// 
        /// Sometimes the library needs to know the length of a file. It does
        /// this by seeking to the end of the file with seek(file, 0,
        /// MSPACK_SYS_SEEK_END), then calling tell(). Implementations may want
        /// to make a special case for this.
        /// 
        /// Due to the potentially varying 32/64 bit datatype off_t on some
        /// architectures, the #MSPACK_SYS_SELFTEST macro MUST be used before
        /// using the library. If not, the error caused by the library passing an
        /// inappropriate stackframe to seek() is subtle and hard to trace.
        /// </summary>
        /// <param name="file">The file to be seeked</param>
        /// <param name="offset">An offset to seek, measured in bytes</param>
        /// <param name="mode">One of <see cref="MSPACK_SYS_SEEK"/> values</param>
        /// <returns>Zero for success, non-zero for an error</returns>
        /// <see cref="open(in string, MSPACK_SYS_OPEN)"/> 
        /// <see cref="tell(mspack_file)"/> 
        public abstract int seek(mspack_file file, long offset, MSPACK_SYS_SEEK mode);

        /// <summary>
        /// Returns the current file position (in bytes) of the given file.
        /// </summary>
        /// <param name="file">The file whose file position is wanted</param>
        /// <returns>The current file position of the file</returns>
        /// <see cref="open(in string, MSPACK_SYS_OPEN)"/> 
        /// <see cref="seek(mspack_file, int, MSPACK_SYS_SEEK)"/> 
        public abstract long tell(mspack_file file);

        /// <summary>
        /// Used to send messages from the library to the user.
        /// 
        /// Occasionally, the library generates warnings or other messages in
        /// plain english to inform the human user. These are informational only
        /// and can be ignored if not wanted.
        /// </summary>
        /// <param name="file">
        /// May be a file handle returned from open() if this message
        /// pertains to a specific open file, or NULL if not related to
        /// a specific file.
        /// </param>
        /// <param name="format">
        /// a printf() style format string. It does NOT include a
        /// trailing newline.
        /// </param>
        public abstract void message(mspack_file file, in string format, params string[] args);

        /// <summary>
        /// Allocates memory
        /// </summary>
        /// <param name="bytes">The number of bytes to allocate</param>
        /// <returns>
        /// A pointer to the requested number of bytes, or NULL if
        /// not enough memory is available
        /// </returns>
        /// <see cref="free(void*)"/> 
        public abstract void* alloc(int bytes);

        /// <summary>
        /// Frees memory
        /// </summary>
        /// <param name="ptr">The memory to be freed. NULL is accepted and ignored.</param>
        /// <see cref="alloc(int)"/> 
        public abstract void free(void* ptr);

        /// <summary>
        /// Copies from one region of memory to another.
        /// 
        /// The regions of memory are guaranteed not to overlap, are usually less
        /// than 256 bytes, and may not be aligned. Please note that the source
        /// parameter comes before the destination parameter, unlike the standard
        /// C function memcpy().
        /// </summary>
        /// <param name="src">The region of memory to copy from</param>
        /// <param name="dest">The region of memory to copy to</param>
        /// <param name="bytes">The size of the memory region, in bytes</param>
        public abstract void copy(void* src, void* dest, int bytes);
    }
}