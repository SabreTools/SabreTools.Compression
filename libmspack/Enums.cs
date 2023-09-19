using System;

namespace SabreTools.Compression.libmspack
{
    #region mspack.h

    /// <summary>
    /// Pass to mspack_version()
    /// </summary>
    public enum MSPACK_VER : int
    {
        /// <summary>
        /// Get the overall library version
        /// </summary>
        MSPACK_VER_LIBRARY = 0,

        /// <summary>
        /// Get the mspack_system version
        /// </summary>
        MSPACK_VER_SYSTEM = 1,

        /// <summary>
        /// Get the mscab_decompressor version
        /// </summary>
        MSPACK_VER_MSCABD = 2,

        /// <summary>
        /// Get the mscab_compressor version
        /// </summary>
        MSPACK_VER_MSCABC = 3,

        /// <summary>
        /// Get the mschm_decompressor version
        /// </summary>
        MSPACK_VER_MSCHMD = 4,

        /// <summary>
        /// Get the mschm_compressor version
        /// </summary>
        MSPACK_VER_MSCHMC = 5,

        /// <summary>
        /// Get the mslit_decompressor version
        /// </summary>
        MSPACK_VER_MSLITD = 6,

        /// <summary>
        /// Get the mslit_compressor version
        /// </summary>
        MSPACK_VER_MSLITC = 7,

        /// <summary>
        /// Get the mshlp_decompressor version
        /// </summary>
        MSPACK_VER_MSHLPD = 8,

        /// <summary>
        /// Get the mshlp_compressor version
        /// </summary>
        MSPACK_VER_MSHLPC = 9,

        /// <summary>
        /// Get the msszdd_decompressor version
        /// </summary>
        MSPACK_VER_MSSZDDD = 10,

        /// <summary>
        /// Get the msszdd_compressor version
        /// </summary>
        MSPACK_VER_MSSZDDC = 11,

        /// <summary>
        /// Get the mskwaj_decompressor version
        /// </summary>
        MSPACK_VER_MSKWAJD = 12,

        /// <summary>
        /// Get the mskwaj_compressor version
        /// </summary>
        MSPACK_VER_MSKWAJC = 13,

        /// <summary>
        /// Get the msoab_decompressor version
        /// </summary>
        MSPACK_VER_MSOABD = 14,

        /// <summary>
        /// Get the msoab_compressor version
        /// </summary>
        MSPACK_VER_MSOABC = 15,
    }

    /// <summary>
    /// mspack_system::open() mode
    /// </summary>
    public enum MSPACK_SYS_OPEN : int
    {
        /// <summary>
        /// Open existing file for reading
        /// </summary>
        MSPACK_SYS_OPEN_READ = 0,

        /// <summary>
        /// Open new file for writing
        /// </summary>
        MSPACK_SYS_OPEN_WRITE = 1,

        /// <summary>
        /// Open existing file for writing
        /// </summary>
        MSPACK_SYS_OPEN_UPDATE = 2,

        /// <summary>
        /// Open existing file for writing
        /// </summary>
        MSPACK_SYS_OPEN_APPEND = 3,
    }

    /// <summary>
    /// mspack_system::seek() mode
    /// </summary>
    public enum MSPACK_SYS_SEEK : int
    {
        /// <summary>
        /// Seek relative to start of file
        /// </summary>
        MSPACK_SYS_SEEK_START = 0,

        /// <summary>
        /// Seek relative to current offset
        /// </summary>
        MSPACK_SYS_SEEK_CUR = 1,

        /// <summary>
        /// Seek relative to end of file
        /// </summary>
        MSPACK_SYS_SEEK_END = 2,
    }

    /// <summary>
    /// Error code
    /// </summary>
    public enum MSPACK_ERR : int
    {
        MSPACK_ERR_OK = 0,

        /// <summary>
        /// Bad arguments to method
        /// </summary>
        MSPACK_ERR_ARGS = 1,

        /// <summary>
        /// Error opening file
        /// </summary>
        MSPACK_ERR_OPEN = 2,

        /// <summary>
        /// Error reading file
        /// </summary>
        MSPACK_ERR_READ = 3,

        /// <summary>
        /// Error writing file
        /// </summary>
        MSPACK_ERR_WRITE = 4,

        /// <summary>
        /// Seek error
        /// </summary>
        MSPACK_ERR_SEEK = 5,

        /// <summary>
        /// Out of memory
        /// </summary>
        MSPACK_ERR_NOMEMORY = 6,

        /// <summary>
        /// Bad "magic id" in file
        /// </summary>
        MSPACK_ERR_SIGNATURE = 7,

        /// <summary>
        /// Bad or corrupt file format
        /// </summary>
        MSPACK_ERR_DATAFORMAT = 8,

        /// <summary>
        /// Bad checksum or CRC
        /// </summary>
        MSPACK_ERR_CHECKSUM = 9,

        /// <summary>
        /// Error during compression
        /// </summary>
        MSPACK_ERR_CRUNCH = 10,

        /// <summary>
        /// Error during decompression
        /// </summary>
        MSPACK_ERR_DECRUNCH = 11,
    }

    /// <summary>
    /// Cabinet header flag
    /// </summary>
    [Flags]
    public enum MSCAB_HDR : int
    {
        /// <summary>
        /// Cabinet has a predecessor
        /// </summary>
        MSCAB_HDR_PREVCAB = 0x01,

        /// <summary>
        /// Cabinet has a successor
        /// </summary>
        MSCAB_HDR_NEXTCAB = 0x02,

        /// <summary>
        /// Cabinet has reserved header space
        /// </summary>
        MSCAB_HDR_RESV = 0x04,
    }

    /// <summary>
    /// Compression mode
    /// </summary>
    public enum MSCAB_COMP : int
    {
        /// <summary>
        /// No compression
        /// </summary>
        MSCAB_COMP_NONE = 0,

        /// <summary>
        /// MSZIP (deflate) compression
        /// </summary>
        MSCAB_COMP_MSZIP = 1,

        /// <summary>
        /// Quantum compression
        /// </summary>
        MSCAB_COMP_QUANTUM = 2,

        /// <summary>
        /// LZX compression
        /// </summary>
        MSCAB_COMP_LZX = 3,
    }

    /// <summary>
    /// mscabd_file::attribs attribute
    /// </summary>
    [Flags]
    public enum MSCAB_ATTRIB : int
    {
        /// <summary>
        /// File is read-only
        /// </summary>
        MSCAB_ATTRIB_RDONLY = 0x01,

        /// <summary>
        /// File is hidden
        /// </summary>
        MSCAB_ATTRIB_HIDDEN = 0x02,

        /// <summary>
        /// File is an operating system file
        /// </summary>
        MSCAB_ATTRIB_SYSTEM = 0x04,

        /// <summary>
        /// File is "archived"
        /// </summary>
        MSCAB_ATTRIB_ARCH = 0x20,

        /// <summary>
        /// File is an executable program
        /// </summary>
        MSCAB_ATTRIB_EXEC = 0x40,

        /// <summary>
        /// Filename is UTF8, not ISO-8859-1
        /// </summary>
        MSCAB_ATTRIB_UTF_NAME = 0x80,
    }

    /// <summary>
    /// mschmc_file::section value
    /// </summary>
    public enum MSCHMC : int
    {
        /// <summary>
        /// End of CHM file list
        /// </summary>
        MSCHMC_ENDLIST = 0,

        /// <summary>
        /// This file is in the Uncompressed section
        /// </summary>
        MSCHMC_UNCOMP = 1,

        /// <summary>
        /// This file is in the MSCompressed section
        /// </summary>
        MSCHMC_MSCOMP = 2,
    }

    /// <summary>
    /// msszddd_header::format value
    /// </summary>
    public enum MSSZDD_FMT : int
    {
        /// <summary>
        /// A regular SZDD file
        /// </summary>
        MSSZDD_FMT_NORMAL = 0,

        /// <summary>
        /// A special QBasic SZDD file
        /// </summary>
        MSSZDD_FMT_QBASIC = 1,
    }

    /// <summary>
    /// WAJ compression type
    /// </summary>
    public enum MSKWAJ_COMP : int
    {
        /// <summary>
        /// No compression
        /// </summary>
        MSKWAJ_COMP_NONE = 0,

        /// <summary>
        /// No compression, 0xFF XOR "encryption"
        /// </summary>
        MSKWAJ_COMP_XOR = 1,

        /// <summary>
        /// LZSS (same method as SZDD)
        /// </summary>
        MSKWAJ_COMP_SZDD = 2,

        /// <summary>
        /// LZ+Huffman compression
        /// </summary>
        MSKWAJ_COMP_LZH = 3,

        /// <summary>
        /// MSZIP
        /// </summary>
        MSKWAJ_COMP_MSZIP = 4,
    }

    /// <summary>
    /// KWAJ optional header flag
    /// </summary>
    [Flags]
    public enum MSKWAJ_HDR : int
    {
        /// <summary>
        /// Decompressed file length is included
        /// </summary>
        MSKWAJ_HDR_HASLENGTH = 0x01,

        /// <summary>
        /// Unknown 2-byte structure is included
        /// </summary>
        MSKWAJ_HDR_HASUNKNOWN1 = 0x02,

        /// <summary>
        /// Unknown multi-sized structure is included
        /// </summary>
        MSKWAJ_HDR_HASUNKNOWN2 = 0x04,

        /// <summary>
        /// File name (no extension) is included
        /// </summary>
        MSKWAJ_HDR_HASFILENAME = 0x08,

        /// <summary>
        /// File extension is included
        /// </summary>
        MSKWAJ_HDR_HASFILEEXT = 0x10,

        /// <summary>
        /// Extra text is included
        /// </summary>
        MSKWAJ_HDR_HASEXTRATEXT = 0x20,
    }

    #region Parameters

    /// <summary>
    /// mscab_decompressor::set_param() parameter
    /// </summary>
    public enum MSCABD_PARAM : int
    {
        /// <summary>
        /// Search buffer size
        /// </summary>
        MSCABD_PARAM_SEARCHBUF = 0,

        /// <summary>
        /// Repair MS-ZIP streams?
        /// </summary>
        MSCABD_PARAM_FIXMSZIP = 1,

        /// <summary>
        /// Size of decompression buffer
        /// </summary>
        MSCABD_PARAM_DECOMPBUF = 2,

        /// <summary>
        /// Salvage data from bad cabinets?
        /// If enabled, open() will skip file with bad folder indices or filenames
        /// rather than reject the whole cabinet, and extract() will limit rather than
        /// reject files with invalid offsets and lengths, and bad data block checksums
        /// will be ignored. Available only in CAB decoder version 2 and above.
        /// </summary>
        MSCABD_PARAM_SALVAGE = 3,
    }

    /// <summary>
    /// mschm_compressor::set_param() parameter
    /// </summary>
    public enum MSCHMC_PARAM : int
    {
        /// <summary>
        /// "timestamp" header
        /// </summary>
        MSCHMC_PARAM_TIMESTAMP = 0,

        /// <summary>
        /// "language" header
        /// </summary>
        MSCHMC_PARAM_LANGUAGE = 1,

        /// <summary>
        /// LZX window size
        /// </summary>
        MSCHMC_PARAM_LZXWINDOW = 2,

        /// <summary>
        /// Intra-chunk quickref density
        /// </summary>
        MSCHMC_PARAM_DENSITY = 3,

        /// <summary>
        /// Whether to create indices
        /// </summary>
        MSCHMC_PARAM_INDEX = 4,
    }

    /// <summary>
    /// msszdd_compressor::set_param() parameter
    /// </summary>
    public enum MSSZDDC_PARAM : int
    {
        /// <summary>
        /// The missing character
        /// </summary>
        MSSZDDC_PARAM_MISSINGCHAR = 0,
    }

    /// <summary>
    /// mskwaj_compressor::set_param() parameter
    /// </summary>
    public enum MSKWAJC_PARAM : int
    {
        /// <summary>
        /// Compression type
        /// </summary>
        MSKWAJC_PARAM_COMP_TYPE = 0,

        /// <summary>
        /// Include the length of the uncompressed file in the header?
        /// </summary>
        MSKWAJC_PARAM_INCLUDE_LENGTH = 1,
    }

    /// <summary>
    /// msoab_decompressor::set_param() parameter
    /// </summary>
    public enum MSOABD_PARAM : int
    {
        /// <summary>
        /// Size of decompression buffer
        /// </summary>
        MSOABD_PARAM_DECOMPBUF = 0,
    }

    #endregion

    #endregion

    #region lzss.h

    public enum LZSS_MODE : int
    {
        LZSS_MODE_EXPAND = 0,
        LZSS_MODE_MSHELP = 1,
        LZSS_MODE_QBASIC = 2,
    }

    #endregion
}