namespace SabreTools.Compression.libmspack
{
    public unsafe static class qtm
    {
        public const int QTM_FRAME_SIZE = 32768;

        #region Quantum static data tables

        /*
        * Quantum uses 'position slots' to represent match offsets.  For every
        * match, a small 'position slot' number and a small offset from that slot
        * are encoded instead of one large offset.
        *
        * position_base[] is an index to the position slot bases
        *
        * extra_bits[] states how many bits of offset-from-base data is needed.
        *
        * length_base[] and length_extra[] are equivalent in function, but are
        * used for encoding selector 6 (variable length match) match lengths,
        * instead of match offsets.
        *
        * They are generated with the following code:
        *   unsigned int i, offset;
        *   for (i = 0, offset = 0; i < 42; i++) {
        *     position_base[i] = offset;
        *     extra_bits[i] = ((i < 2) ? 0 : (i - 2)) >> 1;
        *     offset += 1 << extra_bits[i];
        *   }
        *   for (i = 0, offset = 0; i < 26; i++) {
        *     length_base[i] = offset;
        *     length_extra[i] = (i < 2 ? 0 : i - 2) >> 2;
        *     offset += 1 << length_extra[i];
        *   }
        *   length_base[26] = 254; length_extra[26] = 0;
        */

        private static readonly uint[] position_base = new uint[42]
        {
            0, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256, 384, 512, 768,
            1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576, 32768, 49152,
            65536, 98304, 131072, 196608, 262144, 393216, 524288, 786432, 1048576, 1572864
        };
        private static readonly byte[] extra_bits = new byte[42]
        {
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10,
            11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19
        };

        private static readonly byte[] length_base = new byte[27]
        {
            0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 14, 18, 22, 26,
            30, 38, 46, 54, 62, 78, 94, 110, 126, 158, 190, 222, 254
        };

        private static readonly byte[] length_extra = new byte[27]
        {
            0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
            3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
        };

        #endregion

        private static void qtmd_update_model(qtmd_model model)
        {
            qtmd_modelsym tmp;
            int i, j;

            if (--model.shiftsleft > 0)
            {
                for (i = model.entries - 1; i >= 0; i--)
                {
                    /* -1, not -2; the 0 entry saves this */
                    model.syms[i].cumfreq >>= 1;
                    if (model.syms[i].cumfreq <= model.syms[i + 1].cumfreq)
                    {
                        model.syms[i].cumfreq = (ushort)(model.syms[i + 1].cumfreq + 1);
                    }
                }
            }
            else
            {
                model.shiftsleft = 50;
                for (i = 0; i < model.entries; i++)
                {
                    /* no -1, want to include the 0 entry */
                    /* this converts cumfreqs into frequencies, then shifts right */
                    model.syms[i].cumfreq -= model.syms[i + 1].cumfreq;
                    model.syms[i].cumfreq++; /* avoid losing things entirely */
                    model.syms[i].cumfreq >>= 1;
                }

                /* now sort by frequencies, decreasing order -- this must be an
                 * inplace selection sort, or a sort with the same (in)stability
                 * characteristics */
                for (i = 0; i < model.entries - 1; i++)
                {
                    for (j = i + 1; j < model.entries; j++)
                    {
                        if (model.syms[i].cumfreq < model.syms[j].cumfreq)
                        {
                            tmp = model.syms[i];
                            model.syms[i] = model.syms[j];
                            model.syms[j] = tmp;
                        }
                    }
                }

                /* then convert frequencies back to cumfreq */
                for (i = model.entries - 1; i >= 0; i--)
                {
                    model.syms[i].cumfreq += model.syms[i + 1].cumfreq;
                }
            }
        }

        /// <summary>
        /// Allocates Quantum decompression state for decoding the given stream.
        ///
        /// - returns null if window_bits is outwith the range 10 to 21 (inclusive).
        ///
        /// - uses system.alloc() to allocate memory
        ///
        /// - returns null if not enough memory
        ///
        /// - window_bits is the size of the Quantum window, from 1Kb (10) to 2Mb (21).
        ///
        /// - input_buffer_size is the number of bytes to use to store bitstream data.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="window_bits"></param>
        /// <param name="input_buffer_size"></param>
        /// <returns></returns>
        public static qtmd_stream qtmd_init(mspack_system system, mspack_file input, mspack_file output, int window_bits, int input_buffer_size)
        {
            uint window_size = (uint)(1 << window_bits);
            int i;

            if (system == null) return null;

            // Quantum supports window sizes of 2^10 (1Kb) through 2^21 (2Mb)
            if (window_bits < 10 || window_bits > 21) return null;

            // Round up input buffer size to multiple of two
            input_buffer_size = (input_buffer_size + 1) & -2;
            if (input_buffer_size < 2) return null;

            // Allocate decompression state
            qtmd_stream qtm = new qtmd_stream();

            // Allocate decompression window and input buffer
            qtm.window = (byte*)system.alloc((int)window_size);
            qtm.inbuf = (byte*)system.alloc((int)input_buffer_size);
            if (qtm.window == null || qtm.inbuf == null)
            {
                system.free(qtm.window);
                system.free(qtm.inbuf);
                //system.free(qtm);
                return null;
            }

            // Initialise decompression state
            qtm.sys = system;
            qtm.input = input;
            qtm.output = output;
            qtm.inbuf_size = (uint)input_buffer_size;
            qtm.window_size = window_size;
            qtm.window_posn = 0;
            qtm.frame_todo = QTM_FRAME_SIZE;
            qtm.header_read = 0;
            qtm.error = MSPACK_ERR.MSPACK_ERR_OK;

            qtm.i_ptr = qtm.i_end = &qtm.inbuf[0];
            qtm.o_ptr = qtm.o_end = &qtm.window[0];
            qtm.input_end = 0;
            qtm.bits_left = 0;
            qtm.bit_buffer = 0;

            // Initialise arithmetic coding models
            // - model 4    depends on window size, ranges from 20 to 24
            // - model 5    depends on window size, ranges from 20 to 36
            // - model 6pos depends on window size, ranges from 20 to 42
            i = window_bits * 2;
            qtm.model0 = new qtmd_model(qtm.m0sym, 0, 64);
            qtm.model1 = new qtmd_model(qtm.m1sym, 64, 64);
            qtm.model2 = new qtmd_model(qtm.m2sym, 128, 64);
            qtm.model3 = new qtmd_model(qtm.m3sym, 192, 64);
            qtm.model4 = new qtmd_model(qtm.m4sym, 0, (i > 24) ? 24 : i);
            qtm.model5 = new qtmd_model(qtm.m5sym, 0, (i > 36) ? 36 : i);
            qtm.model6 = new qtmd_model(qtm.m6sym, 0, i);
            qtm.model6len = new qtmd_model(qtm.m6lsym, 0, 27);
            qtm.model7 = new qtmd_model(qtm.m7sym, 0, 7);

            // All ok
            return qtm;
        }

        /// <summary>
        /// Decompresses, or decompresses more of, a Quantum stream.
        ///
        /// - out_bytes of data will be decompressed and the function will return
        ///   with an MSPACK_ERR_OK return code.
        ///
        /// - decompressing will stop as soon as out_bytes is reached. if the true
        ///   amount of bytes decoded spills over that amount, they will be kept for
        ///   a later invocation of qtmd_decompress().
        ///
        /// - the output bytes will be passed to the system.write() function given in
        ///   qtmd_init(), using the output file handle given in qtmd_init(). More
        ///   than one call may be made to system.write()
        ///
        /// - Quantum will read input bytes as necessary using the system.read()
        ///   function given in qtmd_init(), using the input file handle given in
        ///   qtmd_init(). This will continue until system.read() returns 0 bytes,
        ///   or an error.
        /// </summary>
        /// <param name="qtm"></param>
        /// <param name="out_bytes"></param>
        /// <returns></returns>
        public static MSPACK_ERR qtmd_decompress(qtmd_stream qtm, long out_bytes) => MSPACK_ERR.MSPACK_ERR_OK;

        /// <summary>
        /// Frees all state associated with a Quantum data stream
        ///
        /// - calls system.free() using the system pointer given in qtmd_init()
        /// </summary>
        /// <param name="qtm"></param>
        public static void qtmd_free(qtmd_stream qtm)
        {
            mspack_system sys;
            if (qtm != null)
            {
                sys = qtm.sys;
                //sys.free(qtm.window);
                //sys.free(qtm.inbuf);
                //sys.free(qtm);
            }
        }
    }
}