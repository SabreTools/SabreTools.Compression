using System;
using System.Linq;
using System.Runtime.InteropServices;
using static SabreTools.Compression.libmspack.CHM.Constants;
using static SabreTools.Compression.libmspack.macros;
using static SabreTools.Compression.libmspack.system;

namespace SabreTools.Compression.libmspack.CHM
{
    /// <summary>
    /// A decompressor for .CHM (Microsoft HTMLHelp) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack.DestroyCHMDecomperssor(Decompressor)"/>
    public unsafe class Decompressor : BaseDecompressor
    {
        public mschmd_decompress_state d { get; private set; }

        // Filenames of the system files used for decompression.
        // Content and ControlData are essential.
        // ResetTable is preferred, but SpanInfo can be used if not available
        private const string content_name = "::DataSpace/Storage/MSCompressed/Content";
        private const string control_name = "::DataSpace/Storage/MSCompressed/ControlData";
        private const string spaninfo_name = "::DataSpace/Storage/MSCompressed/SpanInfo";
        private const string rtable_name = "::DataSpace/Storage/MSCompressed/Transform/{7FC28940-9D31-11D0-9B27-00A0C91E9C7C}/InstanceData/ResetTable";

        // The GUIDs found in CHM header
        private static readonly byte[] guids = new byte[32]
        {
            // {7C01FD10-7BAA-11D0-9E0C-00A0-C922-E6EC}
            0x10, 0xFD, 0x01, 0x7C, 0xAA, 0x7B, 0xD0, 0x11,
            0x9E, 0x0C, 0x00, 0xA0, 0xC9, 0x22, 0xE6, 0xEC,

            // {7C01FD11-7BAA-11D0-9E0C-00A0-C922-E6EC}
            0x11, 0xFD, 0x01, 0x7C, 0xAA, 0x7B, 0xD0, 0x11,
            0x9E, 0x0C, 0x00, 0xA0, 0xC9, 0x22, 0xE6, 0xEC
        };

        /// <summary>
        /// Creates a new CHM decompressor
        /// </summary>
        public Decompressor()
        {
            this.system = new mspack_default_system();
            error = MSPACK_ERR.MSPACK_ERR_OK;
            d = null;
        }

        /// <summary>
        /// Destroys an existing CHM decompressor
        /// </summary>
        ~Decompressor()
        {
            mspack_system sys = this.system;
            if (this.d != null)
            {
                if (this.d.infh != null) sys.close(this.d.infh);
                if (this.d.state != null) lzxd_free(this.d.state);
                //sys.free(this.d);
            }
            //sys.free(this);
        }

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
        public mschmd_header open(in string filename)
        {
            return chmd_real_open(filename, 1);
        }

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
        public void close(mschmd_header chm)
        {
            mschmd_file fi, nfi;
            mspack_system sys;
            uint i;

            sys = this.system;

            this.error = MSPACK_ERR.MSPACK_ERR_OK;

            // Free files
            for (fi = chm.files; fi != null; fi = nfi)
            {
                nfi = fi.next;
                //sys.free(fi);
            }
            for (fi = chm.sysfiles; fi != null; fi = nfi)
            {
                nfi = fi.next;
                //sys.free(fi);
            }

            // If this CHM was being decompressed, free decompression state
            if (this.d != null && (this.d.chm == chm))
            {
                if (this.d.infh != null) sys.close(this.d.infh);
                if (this.d.state != null) lzxd_free(this.d.state);
                //sys.free(this.d);
                this.d = null;
            }

            // If this CHM had a chunk cache, free it and contents
            if (chm.chunk_cache != null)
            {
                for (i = 0; i < chm.num_chunks; i++) sys.free(chm.chunk_cache[i]);
                sys.free(chm.chunk_cache);
            }

            //sys.free(chm);
        }

        /// <summary>
        /// Reads the basic CHM file headers. If the "entire" parameter is
        /// non-zero, all file entries will also be read. fills out a pre-existing
        /// mschmd_header structure, allocates memory for files as necessary
        /// </summary>
        private MSPACK_ERR chmd_read_headers(mspack_system sys, mspack_file fh, mschmd_header chm, int entire)
        {
            uint errors, num_chunks;
            FixedArray<byte> buf = new FixedArray<byte>(0x54);
            FixedArray<byte> chunk = null;
            byte* name, p, end;
            mschmd_file fi, link = null;
            long offset_hs0, filelen;
            int num_entries;
            MSPACK_ERR err = MSPACK_ERR.MSPACK_ERR_OK;

            // Initialise pointers
            chm.files = null;
            chm.sysfiles = null;
            chm.chunk_cache = null;
            chm.sec0.chm = chm;
            chm.sec0.id = 0;
            chm.sec1.chm = chm;
            chm.sec1.id = 1;
            chm.sec1.content = null;
            chm.sec1.control = null;
            chm.sec1.spaninfo = null;
            chm.sec1.rtable = null;

            // Read the first header
            if (sys.read(fh, buf, chmhead_SIZEOF) != chmhead_SIZEOF)
            {
                return MSPACK_ERR.MSPACK_ERR_READ;
            }

            // Check ITSF signature
            if (EndGetI32(buf, chmhead_Signature) != 0x46535449)
            {
                return MSPACK_ERR.MSPACK_ERR_SIGNATURE;
            }

            // Check both header GUIDs
            if (!buf.ToArray().Skip(chmhead_GUID1).Take(guids.Length).SequenceEqual(guids))
            {
                Console.Error.WriteLine("Incorrect GUIDs");
                return MSPACK_ERR.MSPACK_ERR_SIGNATURE;
            }

            chm.version = EndGetI32(buf, chmhead_Version);
            chm.timestamp = EndGetM32(buf, chmhead_Timestamp);
            chm.language = EndGetI32(buf, chmhead_LanguageID);
            if (chm.version > 3)
            {
                sys.message(fh, "WARNING; CHM version > 3");
            }

            // Read the header section table
            if (sys.read(fh, buf, chmhst3_SIZEOF) != chmhst3_SIZEOF)
            {
                return MSPACK_ERR.MSPACK_ERR_READ;
            }

            // chmhst3_OffsetCS0 does not exist in version 1 or 2 CHM files.
            // The offset will be corrected later, once HS1 is read.
            if (read_off64(&offset_hs0, &buf[chmhst_OffsetHS0], sys, fh) ||
                read_off64(&chm.dir_offset, &buf[chmhst_OffsetHS1], sys, fh) ||
                read_off64(&chm.sec0.offset, &buf[chmhst3_OffsetCS0], sys, fh))
            {
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Seek to header section 0
            if (sys.seek(fh, offset_hs0, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Read header section 0
            if (sys.read(fh, buf, chmhs0_SIZEOF) != chmhs0_SIZEOF)
            {
                return MSPACK_ERR.MSPACK_ERR_READ;
            }

            if (read_off64(&chm.length, &buf[chmhs0_FileLen], sys, fh))
            {
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Compare declared CHM file size against actual size
            if (mspack_sys_filelen(sys, fh, &filelen) == 0)
            {
                if (chm.length > filelen)
                {
                    sys.message(fh, $"WARNING; file possibly truncated by {chm.length - filelen} bytes");
                }
                else if (chm.length < filelen)
                {
                    sys.message(fh, $"WARNING; possible {filelen - chm.length} extra bytes at end of file");
                }
            }

            // Seek to header section 1
            if (sys.seek(fh, chm.dir_offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Read header section 1
            if (sys.read(fh, buf, chmhs1_SIZEOF) != chmhs1_SIZEOF)
            {
                return MSPACK_ERR.MSPACK_ERR_READ;
            }

            chm.dir_offset = sys.tell(fh);
            chm.chunk_size = EndGetI32(buf, chmhs1_ChunkSize);
            chm.density = EndGetI32(buf, chmhs1_Density);
            chm.depth = EndGetI32(buf, chmhs1_Depth);
            chm.index_root = EndGetI32(buf, chmhs1_IndexRoot);
            chm.num_chunks = EndGetI32(buf, chmhs1_NumChunks);
            chm.first_pmgl = EndGetI32(buf, chmhs1_FirstPMGL);
            chm.last_pmgl = EndGetI32(buf, chmhs1_LastPMGL);

            if (chm.version < 3)
            {
                // Versions before 3 don't have chmhst3_OffsetCS0
                chm.sec0.offset = chm.dir_offset + (chm.chunk_size * chm.num_chunks);
            }

            // Check if content offset or file size is wrong
            if (chm.sec0.offset > chm.length)
            {
                Console.Error.WriteLine("content section begins after file has ended");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Ensure there are chunks and that chunk size is
            // large enough for signature and num_entries
            if (chm.chunk_size < (pmgl_Entries + 2))
            {
                Console.Error.WriteLine("chunk size not large enough");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }
            if (chm.num_chunks == 0)
            {
                Console.Error.WriteLine("no chunks");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // The chunk_cache data structure is not great; large values for num_chunks
            // or num_chunks*chunk_size can exhaust all memory. Until a better chunk
            // cache is implemented, put arbitrary limits on num_chunks and chunk size.
            if (chm.num_chunks > 100000)
            {
                Console.Error.WriteLine("more than 100,000 chunks");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }
            if (chm.chunk_size > 8192)
            {
                Console.Error.WriteLine("chunk size over 8192 (get in touch if this is valid)");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }
            if ((long)chm.chunk_size * (long)chm.num_chunks > chm.length)
            {
                Console.Error.WriteLine("chunks larger than entire file");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Common sense checks on header section 1 fields
            if (chm.chunk_size != 4096)
            {
                sys.message(fh, "WARNING; chunk size is not 4096");
            }
            if (chm.first_pmgl != 0)
            {
                sys.message(fh, "WARNING; first PMGL chunk is not zero");
            }
            if (chm.first_pmgl > chm.last_pmgl)
            {
                Console.Error.WriteLine("first pmgl chunk is after last pmgl chunk");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }
            if (chm.index_root != 0xFFFFFFFF && chm.index_root >= chm.num_chunks)
            {
                Console.Error.WriteLine("index_root outside valid range");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // If we are doing a quick read, stop here!
            if (entire == 0)
            {
                return MSPACK_ERR.MSPACK_ERR_OK;
            }

            // Seek to the first PMGL chunk, and reduce the number of chunks to read
            if (chm.first_pmgl != 0)
            {
                long pmgl_offset = (long)chm.first_pmgl * (long)chm.chunk_size;
                if (sys.seek(fh, pmgl_offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                {
                    return MSPACK_ERR.MSPACK_ERR_SEEK;
                }
            }
            num_chunks = chm.last_pmgl - chm.first_pmgl + 1;

            chunk = new FixedArray<byte>((int)chm.chunk_size);

            // Read and process all chunks from FirstPMGL to LastPMGL
            errors = 0;
            while (num_chunks-- > 0)
            {
                // Read next chunk
                if (sys.read(fh, chunk, (int)chm.chunk_size) != (int)chm.chunk_size)
                {
                    sys.free(chunk);
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }

                // Process only directory (PMGL) chunks
                if (EndGetI32(chunk, pmgl_Signature) != 0x4C474D50) continue;

                if (EndGetI32(chunk, pmgl_QuickRefSize) < 2)
                {
                    sys.message(fh, "WARNING; PMGL quickref area is too small");
                }
                if (EndGetI32(chunk, pmgl_QuickRefSize) >
                    (chm.chunk_size - pmgl_Entries))
                {
                    sys.message(fh, "WARNING; PMGL quickref area is too large");
                }

                p = (byte*)chunk.Pointer + pmgl_Entries;
                end = (byte*)chunk.Pointer + chm.chunk_size - 2;
                num_entries = EndGetI16(chunk, (int)(chm.chunk_size - 2));

                while (num_entries-- > 0)
                {
                    uint name_len, section;
                    long offset, length;
                    name_len = read_encint(&p, end, &err);
                    if (err != MSPACK_ERR.MSPACK_ERR_OK || (name_len > (uint)(end - p))) goto encint_err;
                    name = p; p += name_len;
                    section = read_encint(&p, end, &err);
                    offset = read_encint(&p, end, &err);
                    length = read_encint(&p, end, &err);
                    if (err != MSPACK_ERR.MSPACK_ERR_OK) goto encint_err;

                    // Ignore blank or one-char (e.g. "/") filenames we'd return as blank */
                    if (name_len < 2 || name[0] == 0x00 || name[1] == 0x00) continue;

                    // Empty files and directory names are stored as a file entry at
                    // offset 0 with length 0. We want to keep empty files, but not
                    // directory names, which end with a "/"
                    if ((offset == 0) && (length == 0))
                    {
                        if ((name_len > 0) && (name[name_len - 1] == '/')) continue;
                    }

                    if (section > 1)
                    {
                        sys.message(fh, $"Invalid section number '{section}'.");
                        continue;
                    }

                    fi = new mschmd_file();
                    fi.next = null;
                    fi.section = (section == 0 ? (mschmd_section)chm.sec0 : (mschmd_section)chm.sec1);
                    fi.offset = offset;
                    fi.length = length;

                    char[] filenameArr = new char[name_len];
                    Marshal.Copy((IntPtr)name, filenameArr, 0, (int)name_len);
                    filenameArr[(int)name_len] = '\0';
                    fi.filename = new string(filenameArr);

                    if (name[0] == ':' && name[1] == ':')
                    {
                        // System file
                        if (name_len == 40 && fi.filename.StartsWith(content_name))
                        {
                            chm.sec1.content = fi;
                        }
                        else if (name_len == 44 && fi.filename.StartsWith(control_name))
                        {
                            chm.sec1.control = fi;
                        }
                        else if (name_len == 41 && fi.filename.StartsWith(spaninfo_name))
                        {
                            chm.sec1.spaninfo = fi;
                        }
                        else if (name_len == 105 && fi.filename.StartsWith(rtable_name))
                        {
                            chm.sec1.rtable = fi;
                        }
                        fi.next = chm.sysfiles;
                        chm.sysfiles = fi;
                    }
                    else
                    {
                        // Normal file
                        if (link != null) link.next = fi; else chm.files = fi;
                        link = fi;
                    }
                }

            // This is reached either when num_entries runs out, or if
            // an ENCINT is badly encoded
            encint_err:
                if (num_entries >= 0)
                {
                    Console.Error.WriteLine("bad encint before all entries could be read");
                    errors++;
                }
            }

            sys.free(chunk);
            return (errors > 0) ? MSPACK_ERR.MSPACK_ERR_DATAFORMAT : MSPACK_ERR.MSPACK_ERR_OK;
        }

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
        public MSPACK_ERR extract(mschmd_file file, in string filename) => throw new NotImplementedException();

        /// <summary>
        /// Returns the error code set by the most recently called method.
        ///
        /// This is useful for open() and fast_open(), which do not return an
        /// error code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/>
        /// <see cref="extract(mschmd_file, in string)"/>
        public MSPACK_ERR last_error() => throw new NotImplementedException();

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
        /// <see cref="fast_find(mschmd_header, in string, ref mschmd_file, int)"/> 
        /// <see cref="extract(mschmd_file, in string)"/>
        public mschmd_header fast_open(in string filename)
        {
            return chmd_real_open(filename, 0);
        }

        /// <summary>
        /// The real implementation of chmd_open() and chmd_fast_open(). It simply
        /// passes the "entire" parameter to chmd_read_headers(), which will then
        /// either read all headers, or a bare mininum.
        /// </summary>
        private mschmd_header chmd_real_open(in string filename, int entire)
        {
            mschmd_header chm = null;
            MSPACK_ERR error;

            mspack_system sys = this.system;

            mspack_file fh;
            if ((fh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ)) != null)
            {
                chm = new mschmd_header();
                chm.filename = filename;
                error = chmd_read_headers(sys, fh, chm, entire);
                if (error != MSPACK_ERR.MSPACK_ERR_OK)
                {
                    // If the error is DATAFORMAT, and there are some results, return
                    // partial results with a warning, rather than nothing
                    if (error == MSPACK_ERR.MSPACK_ERR_DATAFORMAT && (chm.files != null || chm.sysfiles != null))
                    {
                        sys.message(fh, "WARNING; contents are corrupt");
                        error = MSPACK_ERR.MSPACK_ERR_OK;
                    }
                    else
                    {
                        close(chm);
                        chm = null;
                    }
                }

                this.error = error;
                sys.close(fh);
            }
            else
            {
                this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
            }
            return chm;
        }

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
        /// <param name="f_size"><tt>sizeof(mschmd_file)</tt></param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        /// <see cref="open(in string)"/> 
        /// <see cref="close(mschmd_header)"/> 
        /// <see cref="fast_find(mschmd_header, in string, ref mschmd_file, int)"/> 
        /// <see cref="extract(mschmd_file, in string)"/>
        public MSPACK_ERR fast_find(mschmd_header chm, in string filename, ref mschmd_file f_ptr, int f_size)
        {
            mspack_system sys;
            mspack_file fh;

            // p and end are initialised to prevent MSVC warning about "potentially"
            // uninitialised usage. This is provably untrue, but MS won't fix:
            // https://developercommunity.visualstudio.com/content/problem/363489/c4701-false-positive-warning.html
            FixedArray<byte> chunk;
            byte* p = null, end = null;
            MSPACK_ERR err = MSPACK_ERR.MSPACK_ERR_OK;
            int result = -1;
            uint n, sec;

            if (chm == null || f_ptr == null)
            {
                return MSPACK_ERR.MSPACK_ERR_ARGS;
            }

            sys = this.system;

            // Clear the results structure
            f_ptr = new mschmd_file();

            if ((fh = sys.open(chm.filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ)) == null)
            {
                return MSPACK_ERR.MSPACK_ERR_OPEN;
            }

            // Go through PMGI chunk hierarchy to reach PMGL chunk
            if (chm.index_root < chm.num_chunks)
            {
                n = chm.index_root;
                for (; ; )
                {
                    if ((chunk = read_chunk(chm, fh, n)) == null)
                    {
                        sys.close(fh);
                        return this.error;
                    }

                    // Search PMGI/PMGL chunk. exit early if no entry found
                    if ((result = search_chunk(chm, chunk, filename, &p, &end)) <= 0)
                    {
                        break;
                    }

                    // Found result. loop around for next chunk if this is PMGI
                    if (chunk[3] == 0x4C) break;

                    n = read_encint(&p, end, &err);
                    if (err != MSPACK_ERR.MSPACK_ERR_OK) goto encint_err;
                }
            }
            else
            {
                // PMGL chunks only, search from first_pmgl to last_pmgl
                for (n = chm.first_pmgl; n <= chm.last_pmgl; n = EndGetI32(chunk, pmgl_NextChunk))
                {
                    if ((chunk = read_chunk(chm, fh, n)) == null)
                    {
                        err = this.error;
                        break;
                    }

                    // Search PMGL chunk. exit if file found
                    if ((result = search_chunk(chm, chunk, filename, &p, &end)) > 0)
                    {
                        break;
                    }

                    // Stop simple infinite loops: can't visit the same chunk twice
                    if (n == EndGetI32(chunk, pmgl_NextChunk))
                    {
                        break;
                    }
                }
            }

            // If we found a file, read it
            if (result > 0)
            {
                sec = read_encint(&p, end, &err);
                f_ptr.section = (sec == 0) ? (mschmd_section)chm.sec0 : (mschmd_section)chm.sec1;
                f_ptr.offset = read_encint(&p, end, &err);
                f_ptr.length = read_encint(&p, end, &err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) goto encint_err;
            }
            else if (result < 0)
            {
                err = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            sys.close(fh);
            return this.error = err;

        encint_err:
            Console.Error.WriteLine("Bad encint in PGMI/PGML chunk");
            sys.close(fh);
            return this.error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
        }

        /// <summary>
        /// Reads the given chunk into memory, storing it in a chunk cache
        /// so it doesn't need to be read from disk more than once
        /// </summary>
        /// <returns></returns>
        private FixedArray<byte> read_chunk(mschmd_header chm, mspack_file fh, uint chunk_num)
        {
            mspack_system sys = this.system;
            FixedArray<byte> buf;

            // Check arguments - most are already checked by chmd_fast_find
            if (chunk_num >= chm.num_chunks) return null;

            // Ensure chunk cache is available
            if (chm.chunk_cache == null)
            {
                chm.chunk_cache = new FixedArray<byte>[chm.num_chunks];
                if (chm.chunk_cache == null)
                {
                    this.error = MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                    return null;
                }
            }

            // Try to answer out of chunk cache
            if (chm.chunk_cache[chunk_num] != null) return chm.chunk_cache[chunk_num];

            // Need to read chunk - allocate memory for it
            buf = new FixedArray<byte>((int)chm.chunk_size);

            // Seek to block and read it
            if (sys.seek(fh, chm.dir_offset + (chunk_num * chm.chunk_size), MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                this.error = MSPACK_ERR.MSPACK_ERR_SEEK;
                sys.free(buf);
                return null;
            }
            if (sys.read(fh, buf, (int)chm.chunk_size) != (int)chm.chunk_size)
            {
                this.error = MSPACK_ERR.MSPACK_ERR_READ;
                sys.free(buf);
                return null;
            }

            // Check the signature. Is is PMGL or PMGI?
            if (!((buf[0] == 0x50) && (buf[1] == 0x4D) && (buf[2] == 0x47) && ((buf[3] == 0x4C) || (buf[3] == 0x49))))
            {
                this.error = MSPACK_ERR.MSPACK_ERR_SEEK;
                sys.free(buf);
                return null;
            }

            // All OK. Store chunk in cache and return it
            return chm.chunk_cache[chunk_num] = buf;
        }

        /// <summary>
        /// Searches a PMGI/PMGL chunk for a given filename entry. Returns -1 on
        /// data format error, 0 if entry definitely not found, 1 if entry
        /// found. In the latter case, *result and *result_end are set pointing
        /// to that entry's data (either the "next chunk" ENCINT for a PMGI or
        /// the section, offset and length ENCINTs for a PMGL).
        /// 
        /// In the case of PMGL chunks, the entry has definitely been
        /// found. In the case of PMGI chunks, the entry which points to the
        /// chunk that may eventually contain that entry has been found.
        /// </summary>
        /// <returns></returns>
        private int search_chunk(mschmd_header chm, in FixedArray<byte> chunk, in string filename, byte** result, byte** result_end)
        {
            byte* p;
            uint qr_size, num_entries, qr_entries, qr_density, name_len;
            uint L, R, M, entries_off, is_pmgl;
            int cmp;
            MSPACK_ERR err = MSPACK_ERR.MSPACK_ERR_OK;

            uint fname_len = (uint)filename.Length;

            // PMGL chunk or PMGI chunk? (note: read_chunk() has already
            // checked the rest of the characters in the chunk signature)
            if (chunk[3] == 0x4C)
            {
                is_pmgl = 1;
                entries_off = pmgl_Entries;
            }
            else
            {
                is_pmgl = 0;
                entries_off = pmgi_Entries;
            }

            //  Step 1: binary search first filename of each QR entry
            //  - target filename == entry
            //    found file
            //  - target filename < all entries
            //    file not found
            //  - target filename > all entries
            //    proceed to step 2 using final entry
            //  - target filename between two searched entries
            //    proceed to step 2
            qr_size = EndGetI32(chunk, pmgl_QuickRefSize);
            int start = (int)(chm.chunk_size - 2);
            int end = (int)(chm.chunk_size - qr_size);
            num_entries = EndGetI16(chunk, (int)(chm.chunk_size - 2));
            qr_density = (uint)(1 + (1 << (int)chm.density));
            qr_entries = (num_entries + qr_density - 1) / qr_density;

            if (num_entries == 0)
            {
                Console.Error.WriteLine("Chunk has no entries");
                return -1;
            }

            if (qr_size > chm.chunk_size)
            {
                Console.Error.WriteLine("Quickref size > chunk size");
                return -1;
            }

            *result_end = &chunk[end];

            if (((int)qr_entries * 2) > (start - end))
            {
                Console.Error.WriteLine("WARNING; more quickrefs than quickref space");
                qr_entries = 0; // But we can live with it
            }

            if (qr_entries > 0)
            {
                L = 0;
                R = qr_entries - 1;
                do
                {
                    // Pick new midpoint
                    M = (L + R) >> 1;

                    // Compare filename with entry QR points to
                    p = &chunk[entries_off + (M != 0 ? EndGetI16(chunk, start - (int)(M << 1)) : 0)];
                    name_len = read_encint(&p, end, &err);
                    if (err != MSPACK_ERR.MSPACK_ERR_OK || (name_len > (uint)(end - p))) goto encint_err;
                    cmp = compare(filename, (char*)p, fname_len, name_len);

                    if (cmp == 0) break;
                    else if (cmp < 0) { if (M) R = M - 1; else return 0; }
                    else if (cmp > 0) L = M + 1;
                } while (L <= R);
                M = (L + R) >> 1;

                if (cmp == 0)
                {
                    /* exact match! */
                    p += name_len;
                    *result = p;
                    return 1;
                }

                /* otherwise, read the group of entries for QR entry M */
                p = &chunk[entries_off + (M ? EndGetI16(chunk, start - (M << 1)) : 0)];
                num_entries -= (M * qr_density);
                if (num_entries > qr_density) num_entries = qr_density;
            }
            else
            {
                p = &chunk[entries_off];
            }

            /* Step 2: linear search through the set of entries reached in step 1.
             * - filename == any entry
             *   found entry
             * - filename < all entries (PMGI) or any entry (PMGL)
             *   entry not found, stop now
             * - filename > all entries
             *   entry not found (PMGL) / maybe found (PMGI)
             * - 
             */
            *result = null;
            while (num_entries-- > 0)
            {
                name_len = read_encint(&p, end, &err);
                if (err || (name_len > (uint)(end - p))) goto encint_err;
                cmp = compare(filename, (char*)p, fname_len, name_len);
                p += name_len;

                if (cmp == 0)
                {
                    /* entry found */
                    *result = p;
                    return 1;
                }

                if (cmp < 0)
                {
                    /* entry not found (PMGL) / maybe found (PMGI) */
                    break;
                }

                /* read and ignore the rest of this entry */
                if (is_pmgl)
                {
                    while (p < end && (*p++ & 0x80)) ; /* skip section ENCINT */
                    while (p < end && (*p++ & 0x80)) ; /* skip offset ENCINT */
                    while (p < end && (*p++ & 0x80)) ; /* skip length ENCINT */
                }
                else
                {
                    *result = p; /* store potential final result */
                    while (p < end && (*p++ & 0x80)) ; /* skip chunk number ENCINT */
                }
            }

            /* PMGL? not found. PMGI? maybe found */
            return (is_pmgl) ? 0 : (*result ? 1 : 0);

        encint_err:
            Console.Error.WriteLine("bad encint while searching");
            return -1;
        }
    }
}