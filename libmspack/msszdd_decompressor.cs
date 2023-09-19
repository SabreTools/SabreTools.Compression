using System.Linq;
using static SabreTools.Compression.libmspack.szdd;

namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for SZDD compressed files.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_szdd_decompressor()"/>
    /// <see cref="mspack_destroy_szdd_decompressor()"/>
    public unsafe class msszdd_decompressor
    {
        public mspack_system system { get; set; }

        public MSPACK_ERR error { get; set; }

        /// <summary>
        /// Opens a SZDD file and reads the header.
        ///
        /// If the file opened is a valid SZDD file, all headers will be read and
        /// a msszddd_header structure will be returned.
        ///
        /// In the case of an error occuring, null is returned and the error code
        /// is available from last_error().
        ///
        /// The filename pointer should be considered "in use" until close() is
        /// called on the SZDD file.
        /// </summary>
        /// <param name="filename">
        /// The filename of the SZDD compressed file. This is
        /// passed directly to mspack_system::open().
        /// </param>
        /// <returns>A pointer to a msszddd_header structure, or null on failure</returns>
        /// <see cref="close(msszddd_header)"/> 
        public msszddd_header open(in string filename)
        {
            mspack_system sys = this.system;

            mspack_file fh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ);
            msszddd_header hdr = new msszddd_header();
            hdr.fh = fh;
            this.error = szddd_read_headers(sys, fh, hdr);

            if (this.error != MSPACK_ERR.MSPACK_ERR_OK)
            {
                if (fh != null) sys.close(fh);
                //sys.free(hdr);
                hdr = null;
            }

            return hdr;
        }

        /// <summary>
        /// Closes a previously opened SZDD file.
        ///
        /// This closes a SZDD file and frees the msszddd_header associated with
        /// it.
        ///
        /// The SZDD header pointer is now invalid and cannot be used again.
        /// </summary>
        /// <param name="szdd">The SZDD file to close</param>
        public void close(msszddd_header szdd)
        {
            if (this.system == null) return;

            // Close the file handle associated
            this.system.close(szdd.fh);

            // Free the memory associated
            //this.system.free(hdr);

            this.error = MSPACK_ERR.MSPACK_ERR_OK;
        }

        private static readonly byte[] szdd_signature_expand = new byte[8]
        {
            0x53, 0x5A, 0x44, 0x44, 0x88, 0xF0, 0x27, 0x33
        };

        private static readonly byte[] szdd_signature_qbasic = new byte[8]
        {
            0x53, 0x5A, 0x20, 0x88, 0xF0, 0x27, 0x33, 0xD1
        };

        /// <summary>
        /// Reads the headers of an SZDD format file
        /// </summary>
        /// <param name="sys"></param>
        /// <param name="fh"></param>
        /// <param name="hdr"></param>
        /// <returns></returns>
        private static MSPACK_ERR szddd_read_headers(mspack_system sys, mspack_file fh, msszddd_header hdr)
        {
            byte[] buf = new byte[8];
            byte* bufPtr = libmspack.system.GetArrayPointer<byte>(buf);

            // Read and check signature
            if (sys.read(fh, buffer: bufPtr, 8) != 8) return MSPACK_ERR.MSPACK_ERR_READ;

            if (buf.SequenceEqual(szdd_signature_expand))
            {
                // Common SZDD
                hdr.format = MSSZDD_FMT.MSSZDD_FMT_NORMAL;

                // Read the rest of the header
                if (sys.read(fh, bufPtr, 6) != 6) return MSPACK_ERR.MSPACK_ERR_READ;
                if (buf[0] != 0x41) return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                hdr.missing_char = (char)buf[1];
                hdr.length = EndGetI32(&buf[2]);
            }
            else if (buf.SequenceEqual(szdd_signature_qbasic))
            {
                // Special QBasic SZDD
                hdr.format = MSSZDD_FMT.MSSZDD_FMT_QBASIC;
                if (sys.read(fh, bufPtr, 4) != 4) return MSPACK_ERR.MSPACK_ERR_READ;
                hdr.missing_char = '\0';
                hdr.length = EndGetI32(buf);
            }
            else
            {
                return MSPACK_ERR.MSPACK_ERR_SIGNATURE;
            }
            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Extracts the compressed data from a SZDD file.
        ///
        /// This decompresses the compressed SZDD data stream and writes it to
        /// an output file.
        /// </summary>
        /// <param name="szdd">The SZDD file to extract data from</param>
        /// <param name="filename">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR extract(msszddd_header szdd, in string filename)
        {
            if (szdd == null) return this.error = MSPACK_ERR.MSPACK_ERR_ARGS;
            mspack_system sys = this.system;

            mspack_file fh = szdd.fh;

            // Seek to the compressed data
            long data_offset = (szdd.format == MSSZDD_FMT.MSSZDD_FMT_NORMAL) ? 14 : 12;
            if (sys.seek(fh, data_offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                return this.error = MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Open file for output
            mspack_file outfh;
            if ((outfh = sys.open(filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_WRITE)) == null)
            {
                return this.error = MSPACK_ERR.MSPACK_ERR_OPEN;
            }

            // Decompress the data
            this.error = lzss_decompress(sys, fh, outfh, SZDD_INPUT_SIZE,
                                          szdd.format == MSSZDD_FMT.MSSZDD_FMT_NORMAL
                                          ? LZSS_MODE.LZSS_MODE_EXPAND
                                          : LZSS_MODE.LZSS_MODE_QBASIC);

            // Close output file
            sys.close(outfh);

            return this.error;
        }

        /// <summary>
        /// Decompresses an SZDD file to an output file in one step.
        ///
        /// This opens an SZDD file as input, reads the header, then decompresses
        /// the compressed data immediately to an output file, finally closing
        /// both the input and output file. It is more convenient to use than
        /// open() then extract() then close(), if you do not need to know the
        /// SZDD output size or missing character.
        /// </summary>
        /// <param name="input">
        /// The filename of the input SZDD file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename to write the decompressed data to. This
        /// is passed directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR decompress(in string input, in string output)
        {
            msszddd_header hdr;

            if ((hdr = open(input)) == null) return this.error;
            MSPACK_ERR error = extract(hdr, output);
            close(hdr);
            return this.error = error;
        }

        /// <summary>
        /// Returns the error code set by the most recently called method.
        ///
        /// This is useful for open() which does not return an
        /// error code directly.
        /// </summary>
        /// <returns>The most recent error code</returns>
        /// <see cref="open(in string)"/> 
        /// <see cref="extract(msszddd_header, in string)"/> 
        /// <see cref="decompress(in string, in string)"/> 
        public MSPACK_ERR last_error()
        {
            return this.error;
        }
    }
}