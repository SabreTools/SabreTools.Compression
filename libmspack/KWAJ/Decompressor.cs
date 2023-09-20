using System;
using static SabreTools.Compression.libmspack.KWAJ.Constants;
using static SabreTools.Compression.libmspack.macros;

namespace SabreTools.Compression.libmspack.KWAJ
{
    /// <summary>
    /// A decompressor for KWAJ compressed files.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public unsafe class Decompressor : BaseDecompressor
    {
        /// <summary>
        /// Creates a new KWAJ decompressor.
        /// </summary>
        public Decompressor()
        {
            this.system = new mspack_default_system();
            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Destroys an existing KWAJ decompressor
        /// </summary>
        ~Decompressor()
        {
            mspack_system sys = this.system;
            //sys.free(this);
        }

        /// <summary>
        /// Opens a KWAJ file and reads the header.
        ///
        /// If the file opened is a valid KWAJ file, all headers will be read and
        /// a mskwajd_header structure will be returned.
        ///
        /// In the case of an error occuring, null is returned and the error code
        /// is available from last_error().
        ///
        /// The filename pointer should be considered "in use" until close() is
        /// called on the KWAJ file.
        /// </summary>
        /// <param name="filename">
        /// The filename of the KWAJ compressed file. This is
        /// passed directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a mskwajd_header structure, or null on failure</returns>
        /// <see cref="Close(mskwajd_header)"/>
        public mskwajd_header Open(in string filename)
        {
            mspack_system sys = this.system;

            mspack_file fh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ);
            if (fh == null)
            {
                this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
                return null;
            }

            mskwajd_header hdr = new mskwajd_header();
            hdr.fh = fh;

            MSPACK_ERR err;
            if ((err = ReadHeaders(sys, fh, hdr)) != MSPACK_ERR.MSPACK_ERR_OK)
            {
                Close(hdr);
                this.error = err;
                return null;
            }

            return hdr;
        }

        /// <summary>
        /// Closes a previously opened KWAJ file.
        ///
        /// This closes a KWAJ file and frees the mskwajd_header associated
        /// with it. The KWAJ header pointer is now invalid and cannot be
        /// used again.
        /// </summary>
        /// <param name="kwaj">The KWAJ file to close</param>
        /// <see cref="Open(in string)"/> 
        public void Close(mskwajd_header kwaj)
        {
            if (this.system == null)
                return;

            // Close the file handle associated
            this.system.close(kwaj.fh);

            // Free the memory associated
            //this.system.free(hdr.filename);
            //this.system.free(hdr.extra);
            //this.system.free(hdr);

            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Reads the headers of a KWAJ format file
        /// </summary>
        private MSPACK_ERR ReadHeaders(mspack_system sys, mspack_file fh, mskwajd_header hdr)
        {
            FixedArray<byte> buf = new FixedArray<byte>(16);
            int i;

            // Read in the header
            if (sys.read(fh, buf, kwajh_SIZEOF) != kwajh_SIZEOF)
            {
                return MSPACK_ERR.MSPACK_ERR_READ;
            }

            // Check for "KWAJ" signature
            if ((BitConverter.ToUInt32(buf, kwajh_Signature1) != 0x4A41574B) ||
                (BitConverter.ToUInt32(buf, kwajh_Signature2) != 0xD127F088))
            {
                return MSPACK_ERR.MSPACK_ERR_SIGNATURE;
            }

            // Basic header fields
            hdr.comp_type = (MSKWAJ_COMP)BitConverter.ToUInt16(buf, kwajh_CompMethod);
            hdr.data_offset = BitConverter.ToUInt16(buf, kwajh_DataOffset);
            hdr.headers = (MSKWAJ_HDR)BitConverter.ToUInt16(buf, kwajh_Flags);
            hdr.length = 0;
            hdr.filename = null;
            hdr.extra = null;
            hdr.extra_length = 0;

            // Optional headers

            // 4 bytes: length of unpacked file
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASLENGTH))
            {
                if (sys.read(fh, buf, 4) != 4)
                    return MSPACK_ERR.MSPACK_ERR_READ;

                hdr.length = BitConverter.ToUInt32(buf, 0);
            }

            // 2 bytes: unknown purpose
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASUNKNOWN1))
            {
                if (sys.read(fh, buf, 2) != 2)
                    return MSPACK_ERR.MSPACK_ERR_READ;
            }

            // 2 bytes: length of section, then [length] bytes: unknown purpose
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASUNKNOWN2))
            {
                if (sys.read(fh, buf, 2) != 2)
                    return MSPACK_ERR.MSPACK_ERR_READ;
                i = BitConverter.ToUInt16(buf, 0);
                if (sys.seek(fh, i, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                    return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Filename and extension
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASFILENAME | MSKWAJ_HDR.MSKWAJ_HDR_HASFILEEXT))
            {
                int len;

                // Allocate memory for maximum length filename
                char* fn = (char*)sys.alloc(13);
                if ((hdr.extra = fn) == null)
                    return MSPACK_ERR.MSPACK_ERR_NOMEMORY;

                // Copy filename if present
                if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASFILENAME))
                {
                    // Read and copy up to 9 bytes of a null terminated string
                    if ((len = sys.read(fh, buf, 9)) < 2)
                        return MSPACK_ERR.MSPACK_ERR_READ;

                    for (i = 0; i < len; i++)
                        if ((*fn++ = (char)buf[i]) == '\0')
                            break;

                    // If string was 9 bytes with no null terminator, reject it
                    if (i == 9 && buf[8] != '\0')
                        return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;

                    // Seek to byte after string ended in file
                    if (sys.seek(fh, i + 1 - len, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                        return MSPACK_ERR.MSPACK_ERR_SEEK;

                    fn--; // Remove the null terminator
                }

                // Copy extension if present
                if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASFILEEXT))
                {
                    *fn++ = '.';

                    // Read and copy up to 4 bytes of a null terminated string
                    if ((len = sys.read(fh, buf, 4)) < 2)
                        return MSPACK_ERR.MSPACK_ERR_READ;

                    for (i = 0; i < len; i++)
                        if ((*fn++ = (char)buf[i]) == '\0')
                            break;

                    // If string was 4 bytes with no null terminator, reject it
                    if (i == 4 && buf[3] != '\0')
                        return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;

                    // Seek to byte after string ended in file
                    if (sys.seek(fh, i + 1 - len, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                        return MSPACK_ERR.MSPACK_ERR_SEEK;

                    fn--; // Remove the null terminator
                }
                *fn = '\0';
            }

            // 2 bytes: extra text length then [length] bytes of extra text data
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASEXTRATEXT))
            {
                if (sys.read(fh, buf, 2) != 2)
                    return MSPACK_ERR.MSPACK_ERR_READ;

                i = EndGetI16(buf, 0);
                hdr.extra = (char*)sys.alloc(i + 1);
                if (hdr.extra == null)
                    return MSPACK_ERR.MSPACK_ERR_NOMEMORY;

                if (sys.read(fh, hdr.extra, i) != i)
                    return MSPACK_ERR.MSPACK_ERR_READ;

                hdr.extra[i] = '\0';
                hdr.extra_length = (ushort)i;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Extracts the compressed data from a KWAJ file.
        ///
        /// This decompresses the compressed KWAJ data stream and writes it to
        /// an output file.
        /// </summary>
        /// <param name="kwaj">The KWAJ file to extract data from</param>
        /// <param name="filename">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR Extract(mskwajd_header kwaj, in string filename)
        {
            if (kwaj == null)
                return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;

            mspack_system sys = this.system;
            mspack_file fh = kwaj.fh;

            // Seek to the compressed data
            if (sys.seek(fh, kwaj.data_offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                return this.error = MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Open file for output
            mspack_file outfh;
            if ((outfh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_WRITE)) == null)
            {
                return this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
            }

            this.error = MSPACK_ERR.MSPACK_ERR_OK;

            // Decompress based on format
            if (kwaj.comp_type == MSKWAJ_COMP.MSKWAJ_COMP_NONE || kwaj.comp_type == MSKWAJ_COMP.MSKWAJ_COMP_XOR)
            {
                // NONE is a straight copy. XOR is a copy xored with 0xFF
                byte* buf = (byte*)sys.alloc(KWAJ_INPUT_SIZE);
                if (buf != null)
                {
                    int read, i;
                    while ((read = sys.read(fh, buf, KWAJ_INPUT_SIZE)) > 0)
                    {
                        if (kwaj.comp_type == MSKWAJ_COMP.MSKWAJ_COMP_XOR)
                        {
                            for (i = 0; i < read; i++)
                                buf[i] ^= 0xFF;
                        }

                        if (sys.write(outfh, buf, read) != read)
                        {
                            this.error = MSPACK_ERR.MSPACK_ERR_WRITE;
                            break;
                        }
                    }

                    if (read < 0)
                        this.error = MSPACK_ERR.MSPACK_ERR_READ;

                    sys.free(buf);
                }
                else
                {
                    this.error = MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                }
            }
            else if (kwaj.comp_type == MSKWAJ_COMP.MSKWAJ_COMP_SZDD)
            {
                this.error = lzss_decompress(sys, fh, outfh, KWAJ_INPUT_SIZE, LZSS_MODE.LZSS_MODE_QBASIC);
            }
            else if (kwaj.comp_type == MSKWAJ_COMP.MSKWAJ_COMP_LZH)
            {
                kwajd_stream lzh = lzh_init(sys, fh, outfh);
                this.error = (lzh != null) ? lzh_decompress(lzh) : MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                lzh_free(lzh);
            }
            else if (kwaj.comp_type == MSKWAJ_COMP.MSKWAJ_COMP_MSZIP)
            {
                mszipd_stream zip = mszipd_init(sys, fh, outfh, KWAJ_INPUT_SIZE, 0);
                this.error = (zip != null) ? mszipd_decompress_kwaj(zip) : MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                mszipd_free(zip);
            }
            else
            {
                this.error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }

            // Close output file
            sys.close(outfh);

            return this.error;
        }

        /// <summary>
        /// Decompresses an KWAJ file to an output file in one step.
        ///
        /// This opens an KWAJ file as input, reads the header, then decompresses
        /// the compressed data immediately to an output file, finally closing
        /// both the input and output file. It is more convenient to use than
        /// open() then extract() then close(), if you do not need to know the
        /// KWAJ output size or output filename.
        /// </summary>
        /// <param name="input">
        /// The filename of the input KWAJ file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR Decompress(in string input, in string output)
        {
            mskwajd_header hdr;
            if ((hdr = Open(input)) == null)
                return this.error;

            MSPACK_ERR error = Extract(hdr, output);
            Close(hdr);
            return this.error = error;
        }

        /// <summary>
        /// Returns the error code set by the most recently called method.
        ///
        /// This is useful for open() which does not return an
        /// error code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="Open(in string)"/> 
        /// <see cref="search()"/> 
        public MSPACK_ERR LastError()
        {
            return this.error;
        }
    }
}