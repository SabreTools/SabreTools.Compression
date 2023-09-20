using static SabreTools.Compression.libmspack.cab;
using static SabreTools.Compression.libmspack.CAB.Constants;

namespace SabreTools.Compression.libmspack.CAB
{
    /// <summary>
    /// A decompressor for .CAB (Microsoft Cabinet) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public unsafe class Decompressor : BaseDecompressor
    {
        public mscabd_decompress_state d { get; set; }

        public int buf_size { get; private set; }

        public int searchbuf_size { get; private set; }

        public int fix_mszip { get; private set; }

        public int salvage { get; private set; }

        public MSPACK_ERR read_error { get; private set; }

        /// <summary>
        /// Creates a new CAB decompressor
        /// </summary>
        public Decompressor()
        {
            this.system = new CABSystem();
            this.d = null;
            this.error = MSPACK_ERR.MSPACK_ERR_OK;

            this.searchbuf_size = 32768;
            this.fix_mszip = 0;
            this.buf_size = 4096;
            this.salvage = 0;
        }

        /// <summary>
        /// Destroys an existing CAB decompressor.
        /// </summary>
        ~Decompressor()
        {
            mspack_system sys = this.system;
            if (this.d != null)
            {
                if (this.d.infh != null) sys.close(this.d.infh);
                cab.cabd_free_decomp(this);
                //sys.free(this.d);
            }

            //sys.free(this);
        }

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

            if ((fh = this.system.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ)) != null)
            {
                cab = new mscabd_cabinet();
                cab.filename = filename;
                MSPACK_ERR error = ReadHeaders(this.system, fh, cab, 0, this.salvage, 0);
                if (error != MSPACK_ERR.MSPACK_ERR_OK)
                {
                    close(cab);
                    cab = null;
                }
                this.error = error;
                this.system.close(fh);
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
        private static MSPACK_ERR ReadHeaders(mspack_system sys, mspack_file fh, mscabd_cabinet cab, long offset, int salvage, int quiet)
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
            if (System.BitConverter.ToInt32(buf, cfhead_Signature) != 0x4643534D)
            {
                return MSPACK_ERR.MSPACK_ERR_SIGNATURE;
            }

            // Some basic header fields
            cab.length = System.BitConverter.ToUInt32(buf, cfhead_CabinetSize);
            cab.set_id = System.BitConverter.ToUInt16(buf, cfhead_SetID);
            cab.set_index = System.BitConverter.ToUInt16(buf, cfhead_CabinetIndex);

            // Get the number of folders
            num_folders = System.BitConverter.ToInt16(buf, cfhead_NumFolders);
            if (num_folders == 0)
            {
                if (quiet == 0) sys.message(fh, "No folders in cabinet.");
                return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Get the number of files
            num_files = System.BitConverter.ToInt16(buf, cfhead_NumFiles);
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
            cab.flags = (MSCAB_HDR)System.BitConverter.ToInt16(buf, cfhead_Flags);

            if (cab.flags.HasFlag(MSCAB_HDR.MSCAB_HDR_RESV))
            {
                if (sys.read(fh, libmspack.system.GetArrayPointer(buf), cfheadext_SIZEOF) != cfheadext_SIZEOF)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }
                cab.header_resv = System.BitConverter.ToUInt16(buf, cfheadext_HeaderReserved);
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
                cab.prevname = ReadString(sys, fh, 0, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
                cab.previnfo = ReadString(sys, fh, 1, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
            }

            // Read name and info of next cabinet in set, if present
            if (cab.flags.HasFlag(MSCAB_HDR.MSCAB_HDR_NEXTCAB))
            {
                cab.nextname = ReadString(sys, fh, 0, out err);
                if (err != MSPACK_ERR.MSPACK_ERR_OK) return err;
                cab.nextinfo = ReadString(sys, fh, 1, out err);
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
                fol.comp_type = (MSCAB_COMP)System.BitConverter.ToInt16(buf, cffold_CompType);
                fol.num_blocks = (uint)System.BitConverter.ToInt16(buf, cffold_NumBlocks);
                fol.data.next = null;
                fol.data.cab = cab;
                fol.data.offset = offset + (long)((uint)System.BitConverter.ToInt32(buf, cffold_DataOffset));
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
                file.length = System.BitConverter.ToUInt32(buf, cffile_UncompressedSize);
                file.attribs = (MSCAB_ATTRIB)System.BitConverter.ToInt16(buf, cffile_Attribs);
                file.offset = System.BitConverter.ToUInt32(buf, cffile_FolderOffset);

                // Set folder pointer
                fidx = System.BitConverter.ToInt16(buf, cffile_FolderIndex);
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
                x = System.BitConverter.ToInt16(buf, cffile_Time);
                file.time_h = (char)(x >> 11);
                file.time_m = (char)((x >> 5) & 0x3F);
                file.time_s = (char)((x << 1) & 0x3E);

                // Get date
                x = System.BitConverter.ToInt16(buf, cffile_Date);
                file.date_d = (char)(x & 0x1F);
                file.date_m = (char)((x >> 5) & 0xF);
                file.date_y = (x >> 9) + 1980;

                // Get filename
                file.filename = ReadString(sys, fh, 0, out err);

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

        private static string ReadString(mspack_system sys, mspack_file fh, int permit_empty, out MSPACK_ERR error)
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
        public mscabd_cabinet search(in string filename)
        {
            mscabd_cabinet cab = null;
            mspack_system sys;
            byte* search_buf;
            mspack_file fh;
            long filelen, firstlen = 0;

            sys = this.system;

            // Allocate a search buffer
            search_buf = (byte*)sys.alloc(this.searchbuf_size);
            if (search_buf == null)
            {
                this.error = MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                return null;
            }

            // Open file and get its full file length
            if ((fh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ)) != null)
            {
                if ((this.error = libmspack.system.mspack_sys_filelen(sys, fh, &filelen)) == MSPACK_ERR.MSPACK_ERR_OK)
                {
                    this.error = Find(search_buf, fh, filename, filelen, ref firstlen, cab);
                }

                // Truncated / extraneous data warning:
                if (firstlen != 0 && (firstlen != filelen) && cab == null || (cab.base_offset == 0))
                {
                    if (firstlen < filelen)
                    {
                        sys.message(fh, $"WARNING; possible {filelen - firstlen} extra bytes at end of file");
                    }
                    else
                    {
                        sys.message(fh, $"WARNING; file possibly truncated by {firstlen - filelen} bytes.");
                    }
                }

                sys.close(fh);
            }
            else
            {
                this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
            }

            // Free the search buffer
            sys.free(search_buf);

            return cab;
        }

        /// <summary>
        /// Find is the inner loop of <see cref="search(in string)">, to make it easier to
        /// break out of the loop and be sure that all resources are freed
        /// </summary>
        private MSPACK_ERR Find(byte* buf, mspack_file fh, in string filename, long flen, ref long firstlen, mscabd_cabinet firstcab)
        {
            mscabd_cabinet cab, link = null;
            long caboff, offset, length;
            mspack_system sys = this.system;
            byte* p, pend;
            byte state = 0;
            uint cablen_u32 = 0, foffset_u32 = 0;
            int false_cabs = 0;

            // Search through the full file length
            for (offset = 0; offset < flen; offset += length)
            {
                // Search length is either the full length of the search buffer, or the
                // amount of data remaining to the end of the file, whichever is less.
                length = flen - offset;
                if (length > this.searchbuf_size)
                {
                    length = this.searchbuf_size;
                }

                // Fill the search buffer with data from disk */
                if (sys.read(fh, buf, (int)length) != (int)length)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }

                // FAQ avoidance strategy
                if ((offset == 0) && (System.BitConverter.ToInt32(buf, 0) == 0x28635349))
                {
                    sys.message(fh, "WARNING; found InstallShield header. Use unshield (https://github.com/twogood/unshield) to unpack this file");
                }

                // Read through the entire buffer.
                for (p = &buf[0], pend = &buf[length]; p < pend;)
                {
                    switch (state)
                    {
                        // Starting state
                        case 0:
                            // we spend most of our time in this while loop, looking for
                            // a leading 'M' of the 'MSCF' signature
                            while (p < pend && *p != 0x4D) p++;
                            // If we found tht 'M', advance state
                            if (p++ < pend) state = 1;
                            break;

                        // Verify that the next 3 bytes are 'S', 'C' and 'F'
                        case 1: state = (byte)((*p++ == 0x53) ? 2 : 0); break;
                        case 2: state = (byte)((*p++ == 0x43) ? 3 : 0); break;
                        case 3: state = (byte)((*p++ == 0x46) ? 4 : 0); break;

                        // We don't care about bytes 4-7 (see default: for action)

                        // Bytes 8-11 are the overall length of the cabinet
                        case 8: cablen_u32 = *p++; state++; break;
                        case 9: cablen_u32 |= (uint)(*p++ << 8); state++; break;
                        case 10: cablen_u32 |= (uint)(*p++ << 16); state++; break;
                        case 11: cablen_u32 |= (uint)(*p++ << 24); state++; break;

                        // We don't care about bytes 12-15 (see default: for action)

                        // Bytes 16-19 are the offset within the cabinet of the filedata
                        case 16: foffset_u32 = *p++; state++; break;
                        case 17: foffset_u32 |= (uint)(*p++ << 8); state++; break;
                        case 18: foffset_u32 |= (uint)(*p++ << 16); state++; break;
                        case 19:
                            foffset_u32 |= (uint)(*p++ << 24);
                            // Now we have recieved 20 bytes of potential cab header. work out
                            // the offset in the file of this potential cabinet */
                            caboff = offset + (p - &buf[0]) - 20;

                            // Should reading cabinet fail, restart search just after 'MSCF'
                            offset = caboff + 4;

                            // Capture the "length of cabinet" field if there is a cabinet at
                            // offset 0 in the file, regardless of whether the cabinet can be
                            // read correctly or not
                            if (caboff == 0) firstlen = cablen_u32;

                            // Check that the files offset is less than the alleged length of
                            // the cabinet, and that the offset + the alleged length are
                            // 'roughly' within the end of overall file length. In salvage
                            // mode, don't check the alleged length, allow it to be garbage
                            if ((foffset_u32 < cablen_u32) &&
                                ((caboff + (long)foffset_u32) < (flen + 32)) &&
                                (((caboff + (long)cablen_u32) < (flen + 32)) || this.salvage != 0))
                            {
                                // Likely cabinet found -- try reading it
                                cab = new mscabd_cabinet();
                                cab.filename = filename;
                                if (ReadHeaders(sys, fh, cab, caboff, this.salvage, 1) != MSPACK_ERR.MSPACK_ERR_OK)
                                {
                                    // Destroy the failed cabinet
                                    close(cab);
                                    false_cabs++;
                                }
                                else
                                {
                                    // Cabinet read correctly!

                                    // Link the cab into the list
                                    if (link == null) firstcab = cab;
                                    else link.next = cab;
                                    link = cab;

                                    // Cause the search to restart after this cab's data.
                                    offset = caboff + (long)cablen_u32;
                                }
                            }

                            // Restart search
                            if (offset >= flen) return MSPACK_ERR.MSPACK_ERR_OK;
                            if (sys.seek(fh, offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
                            {
                                return MSPACK_ERR.MSPACK_ERR_SEEK;
                            }
                            length = 0;
                            p = pend;
                            state = 0;
                            break;

                        /* for bytes 4-7 and 12-15, just advance state/pointer */
                        default:
                            p++; state++;
                            break;
                    } /* switch(state) */
                } /* for (... p < pend ...) */
            } /* for (... offset < length ...) */

            if (false_cabs != 0)
            {
                System.Console.Error.WriteLine("%d false cabinets found", false_cabs);
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }

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
        public MSPACK_ERR append(mscabd_cabinet cab, mscabd_cabinet nextcab)
        {
            return Merge(cab, nextcab);
        }

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
        public MSPACK_ERR prepend(mscabd_cabinet cab, mscabd_cabinet prevcab)
        {
            return Merge(prevcab, cab);
        }

        /// <summary>
        /// Joins cabinets together, also merges split folders between these two
        /// cabinets only. This includes freeing the duplicate folder and file(s)
        /// and allocating a further mscabd_folder_data structure to append to the
        /// merged folder's data parts list.
        /// </summary>
        private MSPACK_ERR Merge(mscabd_cabinet lcab, mscabd_cabinet rcab)
        {
            mscabd_folder_data data, ndata;
            mscabd_folder lfol, rfol;
            mscabd_file fi, rfi, lfi;
            mscabd_cabinet cab;

            mspack_system sys = this.system;

            // Basic args check
            if (lcab == null || rcab == null || (lcab == rcab))
            {
                System.Console.Error.WriteLine("lcab null, rcab null or lcab = rcab");
                return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;
            }

            // Check there's not already a cabinet attached
            if (lcab.nextcab != null || rcab.prevcab != null)
            {
                System.Console.Error.WriteLine("cabs already joined");
                return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;
            }

            // Do not create circular cabinet chains
            for (cab = lcab.prevcab; cab != null; cab = cab.prevcab)
            {
                if (cab == rcab)
                {
                    System.Console.Error.WriteLine("circular!");
                    return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;
                }
            }
            for (cab = rcab.nextcab; cab != null; cab = cab.nextcab)
            {
                if (cab == lcab)
                {
                    System.Console.Error.WriteLine("circular!");
                    return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;
                }
            }

            // Warn about odd set IDs or indices
            if (lcab.set_id != rcab.set_id)
            {
                sys.message(null, "WARNING; merged cabinets with differing Set IDs.");
            }

            if (lcab.set_index > rcab.set_index)
            {
                sys.message(null, "WARNING; merged cabinets with odd order.");
            }

            // Merging the last folder in lcab with the first folder in rcab
            lfol = lcab.folders;
            rfol = rcab.folders;
            while (lfol.next != null)
                lfol = lfol.next;

            // Do we need to merge folders?
            if (lfol.merge_next == null && rfol.merge_prev == null)
            {
                // No, at least one of the folders is not for merging

                // Attach cabs
                lcab.nextcab = rcab;
                rcab.prevcab = lcab;

                // Attach folders
                lfol.next = rfol;

                // Attach files
                fi = lcab.files;
                while (fi.next != null)
                    fi = fi.next;
                fi.next = rcab.files;
            }
            else
            {
                // Folder merge required - do the files match?
                if (CanMergeFolders(sys, lfol, rfol) == 0)
                {
                    return this.error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                }

                // Allocate a new folder data structure
                data = new mscabd_folder_data();

                // Attach cabs
                lcab.nextcab = rcab;
                rcab.prevcab = lcab;

                // Append rfol's data to lfol
                ndata = lfol.data;
                while (ndata.next != null)
                    ndata = ndata.next;
                ndata.next = data;
                data = rfol.data;
                rfol.data.next = null;

                // lfol becomes rfol.
                // NOTE: special case, don't merge if rfol is merge prev and next,
                // rfol.merge_next is going to be deleted, so keep lfol's version
                // instead
                lfol.num_blocks += rfol.num_blocks - 1;
                if ((rfol.merge_next == null) || (rfol.merge_next.folder != rfol))
                {
                    lfol.merge_next = rfol.merge_next;
                }

                // Attach the rfol's folder (except the merge folder)
                while (lfol.next != null)
                    lfol = lfol.next;
                lfol.next = rfol.next;

                // Free disused merge folder
                //sys.free(rfol);

                // Attach rfol's files
                fi = lcab.files;
                while (fi.next != null)
                    fi = fi.next;
                fi.next = rcab.files;

                // Delete all files from rfol's merge folder
                lfi = null;
                for (fi = lcab.files; fi != null; fi = rfi)
                {
                    rfi = fi.next;
                    // If file's folder matches the merge folder, unlink and free it
                    if (fi.folder == rfol)
                    {
                        if (lfi != null)
                            lfi.next = rfi;
                        else
                            lcab.files = rfi;
                        //sys.free(fi.filename);
                        //sys.free(fi);
                    }
                    else
                        lfi = fi;
                }
            }

            // All done! fix files and folders pointers in all cabs so they all
            // point to the same list
            for (cab = lcab.prevcab; cab != null; cab = cab.prevcab)
            {
                cab.files = lcab.files;
                cab.folders = lcab.folders;
            }

            for (cab = lcab.nextcab; cab != null; cab = cab.nextcab)
            {
                cab.files = lcab.files;
                cab.folders = lcab.folders;
            }

            return this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Decides if two folders are OK to merge
        /// </summary>
        private int CanMergeFolders(mspack_system sys, mscabd_folder lfol, mscabd_folder rfol)
        {
            mscabd_file lfi, rfi, l, r;
            int matching = 1;

            // Check that both folders use the same compression method/settings
            if (lfol.comp_type != rfol.comp_type)
            {
                System.Console.Error.WriteLine("folder merge: compression type mismatch");
                return 0;
            }

            /* check there are not too many data blocks after merging */
            if ((lfol.num_blocks + rfol.num_blocks) > CAB_FOLDERMAX)
            {
                System.Console.Error.WriteLine("folder merge: too many data blocks in merged folders");
                return 0;
            }

            if ((lfi = lfol.merge_next) == null || (rfi = rfol.merge_prev) == null)
            {
                System.Console.Error.WriteLine("folder merge: one cabinet has no files to merge");
                return 0;
            }

            // For all files in lfol (which is the last folder in whichever cab and
            // only has files to merge), compare them to the files from rfol. They
            // should be identical in number and order. to verify this, check the
            // offset and length of each file.
            for (l = lfi, r = rfi; l != null; l = l.next, r = r.next)
            {
                if (r == null || (l.offset != r.offset) || (l.length != r.length))
                {
                    matching = 0;
                    break;
                }
            }

            if (matching != 0)
                return 1;

            // If rfol does not begin with an identical copy of the files in lfol, make
            // make a judgement call; if at least ONE file from lfol is in rfol, allow
            // the merge with a warning about missing files.
            matching = 0;
            for (l = lfi; l != null; l = l.next)
            {
                for (r = rfi; r != null; r = r.next)
                {
                    if (l.offset == r.offset && l.length == r.length)
                        break;
                }
                if (r != null)
                    matching = 1;
                else
                    sys.message(null, $"WARNING; merged file {l.filename} not listed in both cabinets");
            }
            return matching;
        }

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
        public MSPACK_ERR extract(mscabd_file file, in string filename)
        {
            mspack_file fh;
            uint filelen;

            if (file == null)
                return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;

            mspack_system sys = this.system;
            mscabd_folder fol = file.folder;

            // If offset is beyond 2GB, nothing can be extracted
            if (file.offset > CAB_LENGTHMAX)
            {
                return this.error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            /* if file claims to go beyond 2GB either error out,
             * or in salvage mode reduce file length so it fits 2GB limit
             */
            filelen = file.length;
            if (filelen > (CAB_LENGTHMAX - file.offset))
            {
                if (this.salvage != 0)
                {
                    filelen = CAB_LENGTHMAX - file.offset;
                }
                else
                {
                    return this.error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                }
            }

            // Extraction impossible if no folder, or folder needs predecessor
            if (fol == null || fol.merge_prev != null)
            {
                sys.message(null, $"ERROR; file \"{file.filename}\" cannot be extracted, cabinet set is incomplete");
                return this.error = MSPACK_ERR.MSPACK_ERR_DECRUNCH;
            }

            // If file goes beyond what can be decoded, given an error.
            // In salvage mode, don't assume block sizes, just try decoding
            if (this.salvage == 0)
            {
                uint maxlen = fol.num_blocks * CAB_BLOCKMAX;
                if (file.offset > maxlen || filelen > (maxlen - file.offset))
                {
                    sys.message(null, $"ERROR; file \"{file.filename}\" cannot be extracted, cabinet set is incomplete");
                    return this.error = MSPACK_ERR.MSPACK_ERR_DECRUNCH;
                }
            }

            // Allocate generic decompression state
            if (this.d == null)
            {
                this.d = new None.DecompressState();
                this.d.sys = sys as CABSystem;
            }

            // Do we need to change folder or reset the current folder?
            if ((this.d.folder != fol) || (this.d.offset > file.offset) || this.d.state == null)
            {
                // Free any existing decompressor
                cabd_free_decomp(this);

                // Do we need to open a new cab file?
                if (this.d.infh == null || (fol.data.cab != this.d.incab))
                {
                    // Close previous file handle if from a different cab
                    if (this.d.infh != null)
                        sys.close(this.d.infh);
                    this.d.incab = fol.data.cab;
                    this.d.infh = sys.open(fol.data.cab.filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ);
                    if (this.d.infh == null)
                        return this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
                }
                // Seek to start of data blocks
                if (sys.seek(this.d.infh, fol.data.offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
                {
                    return this.error = MSPACK_ERR.MSPACK_ERR_SEEK;
                }

                // Set up decompressor
                if (cabd_init_decomp(this, fol.comp_type) != MSPACK_ERR.MSPACK_ERR_OK)
                {
                    return this.error;
                }

                // Initialise new folder state
                this.d.folder = fol;
                this.d.data = fol.data;
                this.d.offset = 0;
                this.d.block = 0;
                this.d.outlen = 0;
                this.d.i_ptr = this.d.i_end = libmspack.system.GetArrayPointer(d.input);

                // Read_error lasts for the lifetime of a decompressor
                this.read_error = MSPACK_ERR.MSPACK_ERR_OK;
            }

            // Open file for output
            if ((fh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_WRITE)) == null)
            {
                return this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
            }

            this.error = MSPACK_ERR.MSPACK_ERR_OK;

            // If file has more than 0 bytes
            if (filelen != 0)
            {
                long bytes;
                MSPACK_ERR error;
                // Get to correct offset.
                // - use null fh to say 'no writing' to cabd_sys_write()
                // - if cabd_sys_read() has an error, it will set this.read_error
                //   and pass back MSPACK_ERR_READ
                this.d.outfh = null;
                if ((bytes = file.offset - this.d.offset) != 0)
                {
                    error = this.d.decompress(this.d.state, bytes);
                    this.error = (error == MSPACK_ERR.MSPACK_ERR_READ) ? this.read_error : error;
                }

                // If getting to the correct offset was error free, unpack file
                if (this.error == MSPACK_ERR.MSPACK_ERR_OK)
                {
                    this.d.outfh = fh;
                    error = this.d.decompress(this.d.state, filelen);
                    this.error = (error == MSPACK_ERR.MSPACK_ERR_READ) ? this.read_error : error;
                }
            }

            // Close output file
            sys.close(fh);
            this.d.outfh = null;

            return this.error;
        }

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