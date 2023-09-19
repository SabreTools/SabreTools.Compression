using static SabreTools.Compression.libmspack.oab;

namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A decompressor for .LZX (Offline Address Book) files
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    /// <see cref="mspack_create_oab_decompressor()"/> 
    /// <see cref="mspack_destroy_oab_decompressor()"/> 
    public unsafe class msoab_decompressor
    {
        public mspack_system system { get; set; }

        public int buf_size { get; set; }

        /// <summary>
        /// Decompresses a full Offline Address Book file.
        ///
        /// If the input file is a valid compressed Offline Address Book file, 
        /// it will be read and the decompressed contents will be written to
        /// the output file.
        /// </summary>
        /// <param name="input">
        /// The filename of the input file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename of the output file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR.MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR decompress(in string input, in string output)
        {
            msoab_decompressor_p* self = (msoab_decompressor_p*)_self;
            mspack_system* sys;
            mspack_file* infh = NULL;
            mspack_file* outfh = NULL;
            byte* buf = NULL;
            byte[] hdrbuf = new byte[oabhead_SIZEOF];
            uint block_max, target_size;
            lzxd_stream* lzx = NULL;
            mspack_system oabd_sys;
            oabd_file in_ofh, out_ofh;
            uint window_bits;
            int ret = MSPACK_ERR_OK;

            if (!self) return MSPACK_ERR_ARGS;
            sys = self->system;

            infh = sys->open(sys, input, MSPACK_SYS_OPEN_READ);
            if (!infh)
            {
                ret = MSPACK_ERR_OPEN;
                goto outlbl;
            }

            if (sys->read(infh, hdrbuf, oabhead_SIZEOF) != oabhead_SIZEOF)
            {
                ret = MSPACK_ERR_READ;
                goto outlbl;
            }

            if (EndGetI32(&hdrbuf[oabhead_VersionHi]) != 3 ||
                EndGetI32(&hdrbuf[oabhead_VersionLo]) != 1)
            {
                ret = MSPACK_ERR_SIGNATURE;
                goto outlbl;
            }

            block_max = EndGetI32(&hdrbuf[oabhead_BlockMax]);
            target_size = EndGetI32(&hdrbuf[oabhead_TargetSize]);

            outfh = sys->open(sys, output, MSPACK_SYS_OPEN_WRITE);
            if (!outfh)
            {
                ret = MSPACK_ERR_OPEN;
                goto outlbl;
            }

            buf = sys->alloc(sys, self->buf_size);
            if (!buf)
            {
                ret = MSPACK_ERR_NOMEMORY;
                goto outlbl;
            }

            oabd_sys = *sys;
            oabd_sys.read = oabd_sys_read;
            oabd_sys.write = oabd_sys_write;

            in_ofh.orig_sys = sys;
            in_ofh.orig_file = infh;

            out_ofh.orig_sys = sys;
            out_ofh.orig_file = outfh;

            while (target_size)
            {
                uint blk_csize, blk_dsize, blk_crc, blk_flags;

                if (sys->read(infh, buf, oabblk_SIZEOF) != oabblk_SIZEOF)
                {
                    ret = MSPACK_ERR_READ;
                    goto outlbl;
                }
                blk_flags = EndGetI32(&buf[oabblk_Flags]);
                blk_csize = EndGetI32(&buf[oabblk_CompSize]);
                blk_dsize = EndGetI32(&buf[oabblk_UncompSize]);
                blk_crc = EndGetI32(&buf[oabblk_CRC]);

                if (blk_dsize > block_max || blk_dsize > target_size || blk_flags > 1)
                {
                    ret = MSPACK_ERR_DATAFORMAT;
                    goto outlbl;
                }

                if (!blk_flags)
                {
                    /* Uncompressed block */
                    if (blk_dsize != blk_csize)
                    {
                        ret = MSPACK_ERR_DATAFORMAT;
                        goto outlbl;
                    }
                    ret = copy_fh(sys, infh, outfh, blk_dsize, buf, self->buf_size);
                    if (ret) goto outlbl;
                }
                else
                {
                    /* LZX compressed block */
                    window_bits = 17;

                    while (window_bits < 25 && (1U << window_bits) < blk_dsize)
                        window_bits++;

                    in_ofh.available = blk_csize;
                    out_ofh.crc = 0xffffffff;

                    lzx = lzxd_init(&oabd_sys, (void*)&in_ofh, (void*)&out_ofh, window_bits,
                                    0, self->buf_size, blk_dsize, 1);
                    if (!lzx)
                    {
                        ret = MSPACK_ERR_NOMEMORY;
                        goto outlbl;
                    }

                    ret = lzxd_decompress(lzx, blk_dsize);
                    if (ret != MSPACK_ERR_OK)
                        goto outlbl;

                    lzxd_free(lzx);
                    lzx = NULL;

                    /* Consume any trailing padding bytes before the next block */
                    ret = copy_fh(sys, infh, NULL, in_ofh.available, buf, self->buf_size);
                    if (ret) goto outlbl;

                    if (out_ofh.crc != blk_crc)
                    {
                        ret = MSPACK_ERR_CHECKSUM;
                        goto outlbl;
                    }
                }
                target_size -= blk_dsize;
            }

        outlbl:
            if (lzx) lzxd_free(lzx);
            if (outfh) sys->close(outfh);
            if (infh) sys->close(infh);
            sys->free(buf);

            return ret;
        }

        /// <summary>
        /// Decompresses an Offline Address Book with an incremental patch file.
        ///
        /// This requires both a full UNCOMPRESSED Offline Address Book file to
        /// act as the "base", and a compressed incremental patch file as input.
        /// If the input file is valid, it will be decompressed with reference to
        /// the base file, and the decompressed contents will be written to the
        /// output file.
        ///
        /// There is no way to tell what the right base file is for the given
        /// incremental patch, but if you get it wrong, this will usually result
        /// in incorrect data being decompressed, which will then fail a checksum
        /// test.
        /// </summary>
        /// <param name="input">
        /// The filename of the input file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="base">
        /// The filename of the base file to which the
        /// incremental patch shall be applied. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <param name="output">
        /// The filename of the output file. This is passed
        /// directly to mspack_system::open().
        /// </param>
        /// <returns>An error code, or MSPACK_ERR.MSPACK_ERR_OK if successful</returns>
        public MSPACK_ERR decompress_incremental(in string input, in string @base, in string output)
        {
            lzxd_stream lzx = null;
            uint window_bits, window_size;
            MSPACK_ERR ret = MSPACK_ERR.MSPACK_ERR_OK;

            mspack_system sys = this.system;

            mspack_file infh = sys.open(input, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ);
            if (infh == null)
            {
                ret = MSPACK_ERR.MSPACK_ERR_OPEN;
                goto outlbl;
            }

            byte[] hdrbuf = new byte[patchhead_SIZEOF];
            byte* hdrbufPtr = libmspack.system.GetArrayPointer(hdrbuf);
            if (sys.read(infh, hdrbufPtr, patchhead_SIZEOF) != patchhead_SIZEOF)
            {
                ret = MSPACK_ERR.MSPACK_ERR_READ;
                goto outlbl;
            }

            if (EndGetI32(&hdrbuf[patchhead_VersionHi]) != 3 ||
                EndGetI32(&hdrbuf[patchhead_VersionLo]) != 2)
            {
                ret = MSPACK_ERR.MSPACK_ERR_SIGNATURE;
                goto outlbl;
            }

            uint block_max = EndGetI32(&hdrbuf[patchhead_BlockMax]);
            uint target_size = EndGetI32(&hdrbuf[patchhead_TargetSize]);

            // We use it for reading block headers too
            if (block_max < patchblk_SIZEOF)
                block_max = patchblk_SIZEOF;

            mspack_file basefh = sys.open(@base, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ);
            if (basefh == null)
            {
                ret = MSPACK_ERR.MSPACK_ERR_OPEN;
                goto outlbl;
            }

            mspack_file outfh = sys.open(output, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_WRITE);
            if (outfh == null)
            {
                ret = MSPACK_ERR.MSPACK_ERR_OPEN;
                goto outlbl;
            }

            byte* buf = (byte*)sys.alloc(this.buf_size);
            if (buf == null)
            {
                ret = MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                goto outlbl;
            }

            mspack_oab_system oabd_sys = sys as mspack_oab_system;

            oabd_file in_ofh = new oabd_file();
            in_ofh.orig_sys = sys;
            in_ofh.orig_file = infh;

            oabd_file out_ofh = new oabd_file();
            out_ofh.orig_sys = sys;
            out_ofh.orig_file = outfh;

            while (target_size > 0)
            {
                if (sys.read(infh, buf, patchblk_SIZEOF) != patchblk_SIZEOF)
                {
                    ret = MSPACK_ERR.MSPACK_ERR_READ;
                    goto outlbl;
                }

                uint blk_csize = EndGetI32(&buf[patchblk_PatchSize]);
                uint blk_dsize = EndGetI32(&buf[patchblk_TargetSize]);
                uint blk_ssize = EndGetI32(&buf[patchblk_SourceSize]);
                uint blk_crc = EndGetI32(&buf[patchblk_CRC]);

                if (blk_dsize > block_max || blk_dsize > target_size ||
                    blk_ssize > block_max)
                {
                    ret = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                    goto outlbl;
                }


                window_size = (blk_ssize + 32767) & ~32767;
                window_size += blk_dsize;
                window_bits = 17;

                while (window_bits < 25 && (1U << window_bits) < window_size)
                    window_bits++;

                in_ofh.available = blk_csize;
                out_ofh.crc = 0xffffffff;

                lzx = lzxd_init(&oabd_sys, (void*)&in_ofh, (void*)&out_ofh, window_bits,
                                0, 4096, blk_dsize, 1);
                if (!lzx)
                {
                    ret = MSPACK_ERR.MSPACK_ERR_NOMEMORY;
                    goto outlbl;
                }
                ret = lzxd_set_reference_data(lzx, sys, basefh, blk_ssize);
                if (ret != MSPACK_ERR.MSPACK_ERR_OK)
                    goto outlbl;

                ret = lzxd_decompress(lzx, blk_dsize);
                if (ret != MSPACK_ERR.MSPACK_ERR_OK)
                    goto outlbl;

                lzxd_free(lzx);
                lzx = null;

                /* Consume any trailing padding bytes before the next block */
                ret = copy_fh(sys, infh, null, in_ofh.available, buf, this.buf_size);
                if (ret) goto outlbl;

                if (out_ofh.crc != blk_crc)
                {
                    ret = MSPACK_ERR.MSPACK_ERR_CHECKSUM;
                    goto outlbl;
                }

                target_size -= blk_dsize;
            }

        outlbl:
            if (lzx) lzxd_free(lzx);
            if (outfh) sys.close(outfh);
            if (basefh) sys.close(basefh);
            if (infh) sys.close(infh);
            sys.free(buf);

            return ret;
        }

        private static MSPACK_ERR copy_fh(mspack_system sys, mspack_file infh, mspack_file outfh, int bytes_to_copy, byte* buf, int buf_size)
        {
            while (bytes_to_copy > 0)
            {
                int run = buf_size;
                if (run > bytes_to_copy)
                {
                    run = bytes_to_copy;
                }
                if (sys.read(infh, buf, run) != run)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }
                if (outfh != null && sys.write(outfh, buf, run) != run)
                {
                    return MSPACK_ERR.MSPACK_ERR_WRITE;
                }
                bytes_to_copy -= run;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        /// <summary>
        /// Sets an OAB decompression engine parameter. Available only in OAB
        /// decompressor version 2 and above.
        ///
        /// - #MSOABD_PARAM_DECOMPBUF: How many bytes should be used as an input
        ///   buffer by decompressors? The minimum value is 16. The default value
        ///   is 4096.
        /// </summary>
        /// <param name="param">The parameter to set</param>
        /// <param name="value">The value to set the parameter to</param>
        /// <returns>
        /// MSPACK_ERR.MSPACK_ERR_OK if all is OK, or MSPACK_ERR.MSPACK_ERR_ARGS if there
        /// is a problem with either parameter or value.
        /// </returns>
        public MSPACK_ERR set_param(MSOABD_PARAM param, int value)
        {
            if (param == MSOABD_PARAM.MSOABD_PARAM_DECOMPBUF && value >= 16)
            {
                // Must be at least 16 bytes (patchblk_SIZEOF, oabblk_SIZEOF)
                this.buf_size = value;
                return MSPACK_ERR.MSPACK_ERR_OK;
            }

            return MSPACK_ERR.MSPACK_ERR_ARGS;
        }
    }
}