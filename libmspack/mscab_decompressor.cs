using static SabreTools.Compression.libmspack.cab;
using static SabreTools.Compression.libmspack.CAB.Constants;

namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for .CAB (Microsoft Cabinet) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_cab_decompressor()"/>
    /// <see cref="mspack_destroy_cab_decompressor()"/>
    public unsafe class mscab_decompressor : Decompressor
    {
        public mscabd_decompress_state d { get; set; }

        public int buf_size { get; set; }

        public int searchbuf_size { get; set; }

        public int fix_mszip { get; set; }

        public int salvage { get; set; }

        public MSPACK_ERR read_error { get; set; }

        /// <summary>
        /// Opens a cabinet file and reads its contents.
        /// 
        /// If the file opened is a valid cabinet file, all headers will be read
        /// and a mscabd_cabinet structure will be returned, with a full list of
        /// folders and files.
        /// 
        /// In the case of an error occuring, null is returned and the error code
        /// is available from last_error().
        /// 
        /// The filename pointer should be considered "in use" until close() is
        /// called on the cabinet.
        /// </summary>
        /// <param name="filename">
        /// The filename of the cabinet file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mscabd_cabinet structure, or null on failure</returns>
        /// <see cref="close(mscabd_cabinet)"/>
        /// <see cref="search(in string)"/>
        /// <see cref="last_error()"/>
        public mscabd_cabinet open(in string filename)
        {
            mscabd_cabinet cab = null;
            mspack_file fh;
            MSPACK_ERR error;

            mspack_system sys = this.system;
            if ((fh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ)) != null)
            {
                cab = new mscabd_cabinet();
                cab.filename = filename;
                error = cabd_read_headers(sys, fh, cab, 0, this.salvage, 0);
                if (error != MSPACK_ERR.MSPACK_ERR_OK)
                {
                    close(cab);
                    cab = null;
                }
                this.error = error;
                sys.close(fh);
            }
            else
            {
                this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
            }

            return cab;
        }

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
        public void close(mscabd_cabinet origcab)
        {
            mscabd_folder_data dat, ndat;
            mscabd_cabinet cab, ncab;
            mscabd_folder fol, nfol;
            mscabd_file fi, nfi;

            mspack_system sys = this.system;

            this.error = MSPACK_ERR.MSPACK_ERR_OK;

            while (origcab != null)
            {
                // Free files
                for (fi = origcab.files; fi != null; fi = nfi)
                {
                    nfi = fi.next;
                    //sys.free(fi.filename);
                    //sys.free(fi);
                }

                // Free folders
                for (fol = origcab.folders; fol != null; fol = nfol)
                {
                    nfol = fol.next;

                    // Free folder decompression state if it has been decompressed
                    if (this.d != null && (this.d.folder == fol))
                    {
                        if (this.d.infh != null) sys.close(this.d.infh);
                        cabd_free_decomp(this);
                        //sys.free(this.d);
                        this.d = null;
                    }

                    // Free folder data segments
                    for (dat = fol.data.next; dat != null; dat = ndat)
                    {
                        ndat = dat.next;
                        //sys.free(dat);
                    }

                    //sys.free(fol);
                }

                // Free predecessor cabinets (and the original cabinet's strings)
                for (cab = origcab; cab != null; cab = ncab)
                {
                    ncab = cab.prevcab;
                    //sys.free(cab.prevname);
                    //sys.free(cab.nextname);
                    //sys.free(cab.previnfo);
                    //sys.free(cab.nextinfo);
                    //if (cab != origcab) sys.free(cab);
                }

                // Free successor cabinets
                for (cab = origcab.nextcab; cab != null; cab = ncab)
                {
                    ncab = cab.nextcab;
                    //sys.free(cab.prevname);
                    //sys.free(cab.nextname);
                    //sys.free(cab.previnfo);
                    //sys.free(cab.nextinfo);
                    //sys.free(cab);
                }

                // Free actual cabinet structure
                cab = origcab.next;
                //sys.free(origcab);

                // Repeat full procedure again with the cab.next pointer (if set)
                origcab = cab;
            }
        }

        /// <summary>
        /// Reads the cabinet file header, folder list and file list.
        /// Fills out a pre-existing mscabd_cabinet structure, allocates memory
        /// for folders and files as necessary
        /// </summary>
        private static MSPACK_ERR cabd_read_headers(mspack_system sys, mspack_file fh, mscabd_cabinet cab, long offset, int salvage, int quiet)
        {
            int num_folders, num_files, folder_resv, i, x, fidx;
            MSPACK_ERR err;
            mscabd_folder fol, linkfol = null;
            mscabd_file file, linkfile = null;
            byte[] buf = new byte[64];

            // Initialise pointers
            cab.next = null;
            cab.files = null;
            cab.folders = null;
            cab.prevcab = cab.nextcab = null;
            cab.prevname = cab.nextname = null;
            cab.previnfo = cab.nextinfo = null;

            cab.base_offset = offset;

            // Seek to CFHEADER
            if (sys.seek(fh, offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Read in the CFHEADER
            if (sys.read(fh, libmspack.system.GetArrayPointer(buf), cfhead_SIZEOF) != cfhead_SIZEOF)
            {
                return MSPACK_ERR.MSPACK_ERR_READ;
            }

            // Check for "MSCF" signature
            if (EndGetI32((byte*)buf[cfhead_Signature]) != 0x4643534D)
            {
                return MSPACK_ERR.MSPACK_ERR_SIGNATURE;
            }

            // Some basic header fields
            cab.length = EndGetI32((byte*)buf[cfhead_CabinetSize]);
            cab.set_id = EndGetI16((byte*)buf[cfhead_SetID]);
            cab.set_index = EndGetI16((byte*)buf[cfhead_CabinetIndex]);

            // Get the number of folders
            num_folders = EndGetI16((byte*)buf[cfhead_NumFolders]);
            if (num_folders == 0)
            {
                if (quiet == 0) sys.message(fh, "No folders in cabinet.");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Get the number of files
            num_files = EndGetI16((byte*)buf[cfhead_NumFiles]);
            if (num_files == 0)
            {
                if (quiet == 0) sys.message(fh, "no files in cabinet.");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Check cabinet version
            if ((buf[cfhead_MajorVersion] != 1) && (buf[cfhead_MinorVersion] != 3))
            {
                if (quiet == 0) sys.message(fh, "WARNING; cabinet version is not 1.3");
            }

            // Read the reserved-sizes part of header, if present
            cab.flags = EndGetI16((byte*)buf[cfhead_Flags]);

            if (cab.flags.HasFlag(MSCAB_HDR.MSCAB_HDR_RESV))
            {
                if (sys.read(fh, libmspack.system.GetArrayPointer(buf), cfheadext_SIZEOF) != cfheadext_SIZEOF)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }
                cab.header_resv = EndGetI16((byte*)buf[cfheadext_HeaderReserved]);
                folder_resv = buf[cfheadext_FolderReserved];
                cab.block_resv = buf[cfheadext_DataReserved];

                if (cab.header_resv > 60000)
                {
                    if (quiet == 0) sys.message(fh, "WARNING; reserved header > 60000.");
                }

                // Skip the reserved header
                if (cab.header_resv != 0)
                {
                    if (sys.seek(fh, cab.header_resv, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                    {
                        return MSPACK_ERR.MSPACK_ERR_SEEK;
                    }
                }
            }
            else
            {
                cab.header_resv = 0;
                folder_resv = 0;
                cab.block_resv = 0;
            }

            // Read name and info of preceeding cabinet in set, if present
            if (cab.flags.HasFlag(MSCAB_HDR.MSCAB_HDR_PREVCAB))
            {
                cab.prevname = cabd_read_string(sys, fh, 0, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
                cab.previnfo = cabd_read_string(sys, fh, 1, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
            }

            // Read name and info of next cabinet in set, if present
            if (cab.flags.HasFlag(MSCAB_HDR.MSCAB_HDR_NEXTCAB))
            {
                cab.nextname = cabd_read_string(sys, fh, 0, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
                cab.nextinfo = cabd_read_string(sys, fh, 1, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
            }

            // Read folders
            for (i = 0; i < num_folders; i++)
            {
                if (sys.read(fh, libmspack.system.GetArrayPointer(buf), cffold_SIZEOF) != cffold_SIZEOF)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }
                if (folder_resv != 0)
                {
                    if (sys.seek(fh, folder_resv, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                    {
                        return MSPACK_ERR.MSPACK_ERR_SEEK;
                    }
                }

                fol = new mscabd_folder();

                fol.next = null;
                fol.comp_type = EndGetI16((byte*)buf[cffold_CompType]);
                fol.num_blocks = EndGetI16((byte*)buf[cffold_NumBlocks]);
                fol.data.next = null;
                fol.data.cab = cab;
                fol.data.offset = offset + (long)((uint)EndGetI32((byte*)buf[cffold_DataOffset]));
                fol.merge_prev = null;
                fol.merge_next = null;

                // Link folder into list of folders
                if (linkfol == null) cab.folders = fol;
                else linkfol.next = fol;
                linkfol = fol;
            }

            // Read files
            for (i = 0; i < num_files; i++)
            {
                if (sys.read(fh, libmspack.system.GetArrayPointer(buf), cffile_SIZEOF) != cffile_SIZEOF)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }

                file = new mscabd_file();

                file.next = null;
                file.length = EndGetI32((byte*)buf[cffile_UncompressedSize]);
                file.attribs = EndGetI16((byte*)buf[cffile_Attribs]);
                file.offset = EndGetI32((byte*)buf[cffile_FolderOffset]);

                // Set folder pointer
                fidx = EndGetI16((byte*)buf[cffile_FolderIndex]);
                if (fidx < cffileCONTINUED_FROM_PREV)
                {
                    /* normal folder index; count up to the correct folder */
                    if (fidx < num_folders)
                    {
                        mscabd_folder ifol = cab.folders;
                        while (fidx-- > 0) if (ifol != null) ifol = ifol.next;
                        file.folder = ifol;
                    }
                    else
                    {
                        System.Console.Error.WriteLine("Invalid folder index");
                        file.folder = null;
                    }
                }
                else
                {
                    // either CONTINUED_TO_NEXT, CONTINUED_FROM_PREV or CONTINUED_PREV_AND_NEXT
                    if ((fidx == cffileCONTINUED_TO_NEXT) || (fidx == cffileCONTINUED_PREV_AND_NEXT))
                    {
                        // Get last folder
                        mscabd_folder ifol = cab.folders;
                        while (ifol.next != null) ifol = ifol.next;
                        file.folder = ifol;

                        // Set "merge next" pointer
                        fol = ifol;
                        if (fol.merge_next == null) fol.merge_next = file;
                    }

                    if ((fidx == cffileCONTINUED_FROM_PREV) || (fidx == cffileCONTINUED_PREV_AND_NEXT))
                    {
                        // Get first folder
                        file.folder = cab.folders;

                        // Set "merge prev" pointer
                        fol = file.folder;
                        if (fol.merge_prev == null) fol.merge_prev = file;
                    }
                }

                // Get time
                x = EndGetI16((byte*)buf[cffile_Time]);
                file.time_h = (char)(x >> 11);
                file.time_m = (char)((x >> 5) & 0x3F);
                file.time_s = (char)((x << 1) & 0x3E);

                // Get date
                x = EndGetI16((byte*)buf[cffile_Date]);
                file.date_d = (char)(x & 0x1F);
                file.date_m = (char)((x >> 5) & 0xF);
                file.date_y = (x >> 9) + 1980;

                // Get filename
                file.filename = cabd_read_string(sys, fh, 0, out err);

                // If folder index or filename are bad, either skip it or fail
                if (err != MSPACK_ERR.MSPACK_ERR_OK || file.folder == null)
                {
                    //sys.free(file.filename);
                    //sys.free(file);
                    if (salvage != 0) continue;
                    return err != MSPACK_ERR.MSPACK_ERR_OK ? err : MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                }

                // Link file entry into file list
                if (linkfile == null) cab.files = file;
                else linkfile.next = file;
                linkfile = file;
            }

            if (cab.files == null)
            {
                // We never actually added any files to the file list.  Something went wrong.
                // The file header may have been invalid

                System.Console.Error.WriteLine($"No files found, even though header claimed to have {num_files} files");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        private static string cabd_read_string(mspack_system sys, mspack_file fh, int permit_empty, out MSPACK_ERR error)
        {
            long @base = sys.tell(fh);
            byte[] buf = new byte[256];
            string str;
            int len, i, ok;

            // Read up to 256 bytes */
            if ((len = sys.read(fh, libmspack.system.GetArrayPointer(buf), 256)) <= 0)
            {
                error = MSPACK_ERR.MSPACK_ERR_READ;
                return null;
            }

            // Search for a null terminator in the buffer
            for (i = 0, ok = 0; i < len; i++) if (buf[i] == 0) { ok = 1; break; }
            /* optionally reject empty strings */
            if (i == 0 && permit_empty == 0) ok = 0;

            if (ok == 0)
            {
                error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                return null;
            }

            len = i + 1;

            /* set the data stream to just after the string and return */
            if (sys.seek(fh, @base + len, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                error = MSPACK_ERR.MSPACK_ERR_SEEK;
                return null;
            }

            char[] strchr = new char[len];
            sys.copy(libmspack.system.GetArrayPointer(buf), libmspack.system.GetArrayPointer(strchr), len);
            str = new string(strchr);
            error = MSPACK_ERR.MSPACK_ERR_OK;
            return str;
        }

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
        /// open(), null is returned and the error code is available from
        /// last_error().
        /// 
        /// If no error occurs, but no cabinets can be found in the file, null is
        /// returned and last_error() returns MSPACK_ERR.MSPACK_ERR_OK.
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
        /// <returns>A pointer to a mscabd_cabinet structure, or null</returns>
        /// <see cref="close(mscabd_cabinet)"/>
        /// <see cref="open(in string)"/>
        /// <see cref="last_error()"/>
        public mscabd_cabinet search(in string filename) => null;

        /// <summary>
        /// Appends one mscabd_cabinet to another, forming or extending a cabinet
        /// set.
        /// 
        /// This will attempt to append one cabinet to another such that
        /// <tt>(cab.nextcab == nextcab) && (nextcab.prevcab == cab)</tt> and
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
        /// <returns>An error code, or MSPACK_ERR.MSPACK_ERR_OK if successful</returns>
        /// <see cref="prepend(mscabd_cabinet, mscabd_cabinet)"/>
        /// <see cref="open(in string)"/>
        /// <see cref="close(mscabd_cabinet)"/>
        public MSPACK_ERR append(mscabd_cabinet cab, mscabd_cabinet nextcab) => MSPACK_ERR.MSPACK_ERR_OK;

        /// <summary>
        /// Prepends one mscabd_cabinet to another, forming or extending a
        /// cabinet set.
        /// 
        /// This will attempt to prepend one cabinet to another, such that
        /// <tt>(cab.prevcab == prevcab) && (prevcab.nextcab == cab)</tt>. In
        /// all other respects, it is identical to append(). See append() for the
        /// full documentation.
        /// </summary>
        /// <param name="cab">The cabinet which will be prepended to, successor of prevcab</param>
        /// <param name="prevcab">The cabinet which will be prepended, predecessor of cab</param>
        /// <returns>An error code, or MSPACK_ERR.MSPACK_ERR_OK if successful</returns>
        /// <see cref="append(mscabd_cabinet, mscabd_cabinet)"/>
        /// <see cref="open(in string)"/>
        /// <see cref="close(mscabd_cabinet)"/>
        public MSPACK_ERR prepend(mscabd_cabinet cab, mscabd_cabinet prevcab) => MSPACK_ERR.MSPACK_ERR_OK;

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
        /// <returns>An error code, or MSPACK_ERR.MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR extract(mscabd_file file, in string filename) => MSPACK_ERR.MSPACK_ERR_OK;

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
        /// MSPACK_ERR.MSPACK_ERR_OK if all is OK, or MSPACK_ERR.MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        /// <see cref="search(in string)"/>
        /// <see cref="extract(mscabd_file, in string)"/>
        public MSPACK_ERR set_param(MSCABD_PARAM param, int value)
        {
            switch (param)
            {
                case MSCABD_PARAM.MSCABD_PARAM_SEARCHBUF:
                    if (value < 4) return MSPACK_ERR.MSPACK_ERR_ARGS;
                    this.searchbuf_size = value;
                    break;
                case MSCABD_PARAM.MSCABD_PARAM_FIXMSZIP:
                    this.fix_mszip = value;
                    break;
                case MSCABD_PARAM.MSCABD_PARAM_DECOMPBUF:
                    if (value < 4) return MSPACK_ERR.MSPACK_ERR_ARGS;
                    this.buf_size = value;
                    break;
                case MSCABD_PARAM.MSCABD_PARAM_SALVAGE:
                    this.salvage = value;
                    break;
                default:
                    return MSPACK_ERR.MSPACK_ERR_ARGS;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Returns the error code set by the most recently called method.
        /// 
        /// This is useful for open() and search(), which do not return an error
        /// code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/>
        /// <see cref="search(in string)"/>
        public MSPACK_ERR last_error()
        {
            return this.error;
        }
    }
}