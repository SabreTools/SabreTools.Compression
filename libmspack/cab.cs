using System;

namespace SabreTools.Compression.libmspack
{
    public unsafe static class cab
    {
        /* structure offsets */
        public const byte cfhead_Signature = 0x00;
        public const byte cfhead_CabinetSize = 0x08;
        public const byte cfhead_FileOffset = 0x10;
        public const byte cfhead_MinorVersion = 0x18;
        public const byte cfhead_MajorVersion = 0x19;
        public const byte cfhead_NumFolders = 0x1A;
        public const byte cfhead_NumFiles = 0x1C;
        public const byte cfhead_Flags = 0x1E;
        public const byte cfhead_SetID = 0x20;
        public const byte cfhead_CabinetIndex = 0x22;
        public const byte cfhead_SIZEOF = 0x24;
        public const byte cfheadext_HeaderReserved = 0x00;
        public const byte cfheadext_FolderReserved = 0x02;
        public const byte cfheadext_DataReserved = 0x03;
        public const byte cfheadext_SIZEOF = 0x04;
        public const byte cffold_DataOffset = 0x00;
        public const byte cffold_NumBlocks = 0x04;
        public const byte cffold_CompType = 0x06;
        public const byte cffold_SIZEOF = 0x08;
        public const byte cffile_UncompressedSize = 0x00;
        public const byte cffile_FolderOffset = 0x04;
        public const byte cffile_FolderIndex = 0x08;
        public const byte cffile_Date = 0x0A;
        public const byte cffile_Time = 0x0C;
        public const byte cffile_Attribs = 0x0E;
        public const byte cffile_SIZEOF = 0x10;
        public const byte cfdata_CheckSum = 0x00;
        public const byte cfdata_CompressedSize = 0x04;
        public const byte cfdata_UncompressedSize = 0x06;
        public const byte cfdata_SIZEOF = 0x08;

        /* flags */
        public const ushort cffoldCOMPTYPE_MASK = 0x000f;
        public const ushort cffileCONTINUED_FROM_PREV = 0xFFFD;
        public const ushort cffileCONTINUED_TO_NEXT = 0xFFFE;
        public const ushort cffileCONTINUED_PREV_AND_NEXT = 0xFFFF;

        /* CAB data blocks are <= 32768 bytes in uncompressed form. Uncompressed
         * blocks have zero growth. MSZIP guarantees that it won't grow above
         * uncompressed size by more than 12 bytes. LZX guarantees it won't grow
         * more than 6144 bytes. Quantum has no documentation, but the largest
         * block seen in the wild is 337 bytes above uncompressed size.
         */
        public const int CAB_BLOCKMAX = 32768;
        public const int CAB_INPUTMAX = CAB_BLOCKMAX + 6144;

        /* input buffer needs to be CAB_INPUTMAX + 1 byte to allow for max-sized block
         * plus 1 trailer byte added by cabd_sys_read_block() for Quantum alignment.
         *
         * When MSCABD_PARAM_SALVAGE is set, block size is not checked so can be
         * up to 65535 bytes, so max input buffer size needed is 65535 + 1
         */
        public const int CAB_INPUTMAX_SALVAGE = 65535;
        public const int CAB_INPUTBUF = CAB_INPUTMAX_SALVAGE + 1;

        /* There are no more than 65535 data blocks per folder, so a folder cannot
         * be more than 32768*65535 bytes in length. As files cannot span more than
         * one folder, this is also their max offset, length and offset+length limit.
         */
        public const int CAB_FOLDERMAX = 65535;
        public const int CAB_LENGTHMAX = CAB_BLOCKMAX * CAB_FOLDERMAX;

        #region decomp

        /// <summary>
        /// cabd_free_decomp frees decompression state, according to which method
        /// was used.
        /// </summary>
        public static MSPACK_ERR cabd_init_decomp(mscab_decompressor self, MSCAB_COMP ct)
        {
            mspack_file fh = self;

            self.d.comp_type = ct;

            switch ((MSCAB_COMP)((int)ct & cffoldCOMPTYPE_MASK))
            {
                case MSCAB_COMP.MSCAB_COMP_NONE:
                    self.d = new mscabd_noned_decompress_state();
                    self.d.state = noned_init(self.d.sys, fh, fh, self.buf_size);
                    break;
                case MSCAB_COMP.MSCAB_COMP_MSZIP:
                    self.d = new mscabd_mszipd_decompress_state();
                    self.d.state = mszipd_init(self.d.sys, fh, fh, self.buf_size, self.fix_mszip);
                    break;
                case MSCAB_COMP.MSCAB_COMP_QUANTUM:
                    self.d = new mscabd_qtmd_decompress_state();
                    self.d.state = qtmd_init(self.d.sys, fh, fh, ((int)ct >> 8) & 0x1f, self.buf_size);
                    break;
                case MSCAB_COMP.MSCAB_COMP_LZX:
                    self.d = new mscabd_lzxd_decompress_state();
                    self.d.state = lzxd_init(self.d.sys, fh, fh, ((int)ct >> 8) & 0x1f, 0, self.buf_size, 0, 0);
                    break;
                default:
                    return self.error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
            }
            return self.error = (self.d.state != null) ? MSPACK_ERR.MSPACK_ERR_OK : MSPACK_ERR.MSPACK_ERR_NOMEMORY;
        }

        /// <summary>
        /// cabd_init_decomp initialises decompression state, according to which
        /// decompression method was used. relies on self.d.folder being the same
        /// as when initialised.
        /// </summary>
        public static void cabd_free_decomp(mscab_decompressor self)
        {
            if (self == null || self.d == null || self.d.state == null) return;

            switch ((MSCAB_COMP)((int)self.d.comp_type & cffoldCOMPTYPE_MASK))
            {
                case MSCAB_COMP.MSCAB_COMP_NONE: noned_free((noned_state)self.d.state); break;
                case MSCAB_COMP.MSCAB_COMP_MSZIP: mszipd_free((mszipd_stream)self.d.state); break;
                case MSCAB_COMP.MSCAB_COMP_QUANTUM: qtmd_free((qtmd_stream)self.d.state); break;
                case MSCAB_COMP.MSCAB_COMP_LZX: lzxd_free((lzxd_stream)self.d.state); break;
            }

            //self.d.decompress = null;
            self.d.state = null;
        }

        #endregion

        #region noned_state

        public static noned_state noned_init(mspack_system sys, mspack_file @in, mspack_file @out, int bufsize)
        {
            noned_state state = new noned_state();

            state.sys = sys;
            state.i = @in;
            state.o = @out;
            state.buf = system.CreateArray<byte>(bufsize);
            state.bufsize = bufsize;
            return state;
        }

        public static void noned_free(noned_state state)
        {
            mspack_system sys;
            if (state != null)
            {
                sys = state.sys;
                sys.free(state.buf);
                //sys.free(state);
            }
        }

        #endregion
    }
}