using System;
using static SabreTools.Compression.libmspack.kwaj;

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
        /// <see cref="close(mskwajd_header)"/>
        public mskwajd_header open(in string filename)
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
                close(hdr);
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
        /// <see cref="open(in string)"/> 
        public void close(mskwajd_header kwaj)
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
            byte[] buf = new byte[16];
            int i;

            // Read in the header
            if (sys.read(fh, libmspack.system.GetArrayPointer(buf), kwajh_SIZEOF) != kwajh_SIZEOF)
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
                if (sys.read(fh, &buf[0], 4) != 4)
                    return MSPACK_ERR.MSPACK_ERR_READ;

                hdr.length = BitConverter.ToUInt32(buf, 0);
            }

            // 2 bytes: unknown purpose
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASUNKNOWN1))
            {
                if (sys.read(fh, &buf[0], 2) != 2)
                    return MSPACK_ERR.MSPACK_ERR_READ;
            }

            // 2 bytes: length of section, then [length] bytes: unknown purpose
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASUNKNOWN2))
            {
                if (sys.read(fh, &buf[0], 2) != 2)
                    return MSPACK_ERR.MSPACK_ERR_READ;
                i = BitConverter.ToUInt16(buf, 0);
                if (sys.seek(fh, i, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR))
                    return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Filename and extension
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASFILENAME | MSKWAJ_HDR.MSKWAJ_HDR_HASFILEEXT))
            {
                int len;

                // Allocate memory for maximum length filename
                char* fn = (char*)sys.alloc(sys, 13);
                if (!(hdr.filename = fn))
                    return MSPACK_ERR.MSPACK_ERR_NOMEMORY;

                // Copy filename if present
                if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASFILENAME))
                {
                    // Read and copy up to 9 bytes of a null terminated string
                    if ((len = sys.read(fh, &buf[0], 9)) < 2)
                        return MSPACK_ERR.MSPACK_ERR_READ;
                    for (i = 0; i < len; i++)
                        if (!(*fn++ = buf[i]))
                            break;

                    // If string was 9 bytes with no null terminator, reject it
                    if (i == 9 && buf[8] != '\0')
                        return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;

                    // Seek to byte after string ended in file
                    if (sys.seek(fh, i + 1 - len, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR))
                        return MSPACK_ERR.MSPACK_ERR_SEEK;

                    fn--; // Remove the null terminator
                }

                // Copy extension if present
                if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASFILEEXT))
                {
                    *fn++ = '.';

                    // Read and copy up to 4 bytes of a null terminated string
                    if ((len = sys.read(fh, &buf[0], 4)) < 2)
                        return MSPACK_ERR.MSPACK_ERR_READ;

                    for (i = 0; i < len; i++)
                        if (!(*fn++ = buf[i]))
                            break;

                    // If string was 4 bytes with no null terminator, reject it
                    if (i == 4 && buf[3] != '\0')
                        return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;

                    // Seek to byte after string ended in file
                    if (sys.seek(fh, i + 1 - len, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR))
                        return MSPACK_ERR.MSPACK_ERR_SEEK;

                    fn--; // Remove the null terminator
                }
                *fn = '\0';
            }

            // 2 bytes: extra text length then [length] bytes of extra text data
            if (hdr.headers.HasFlag(MSKWAJ_HDR.MSKWAJ_HDR_HASEXTRATEXT))
            {
                if (sys.read(fh, &buf[0], 2) != 2)
                    return MSPACK_ERR.MSPACK_ERR_READ;

                i = BitConverter.ToUInt16(&buf[0]);
                hdr.extra = (char*)sys.alloc(sys, i + 1);
                if (!hdr.extra)
                    return MSPACK_ERR.MSPACK_ERR_NOMEMORY;

                if (sys.read(fh, hdr.extra, i) != i)
                    return MSPACK_ERR.MSPACK_ERR_READ;

                hdr.extra[i] = '\0';
                hdr.extra_length = i;
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
        public MSPACK_ERR extract(mskwajd_header kwaj, in string filename) => throw new NotImplementedException();

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
        public MSPACK_ERR decompress(in string input, in string output) => throw new NotImplementedException();

        /// <summary>
        /// Returns the error code set by the most recently called method.
        ///
        /// This is useful for open() which does not return an
        /// error code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/> 
        /// <see cref="search()"/> 
        public MSPACK_ERR last_error() => throw new NotImplementedException();
    }
}