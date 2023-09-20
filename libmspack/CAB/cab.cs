using static SabreTools.Compression.libmspack.CAB.Constants;

namespace SabreTools.Compression.libmspack
{
    public unsafe static class cab
    {
        #region decomp

        /// <summary>
        /// cabd_free_decomp frees decompression state, according to which method
        /// was used.
        /// </summary>
        public static MSPACK_ERR cabd_init_decomp(CAB.Decompressor self, MSCAB_COMP ct)
        {
            mspack_file fh = self;

            self.d.comp_type = ct;

            switch ((MSCAB_COMP)((int)ct & cffoldCOMPTYPE_MASK))
            {
                case MSCAB_COMP.MSCAB_COMP_NONE:
                    self.d = new None.DecompressState(self.d);
                    self.d.state = new None.State(self.d.sys, fh, fh, self.buf_size);
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
        public static void cabd_free_decomp(CAB.Decompressor self)
        {
            if (self == null || self.d == null || self.d.state == null) return;

            switch ((MSCAB_COMP)((int)self.d.comp_type & cffoldCOMPTYPE_MASK))
            {
                case MSCAB_COMP.MSCAB_COMP_MSZIP: mszipd_free((mszipd_stream)self.d.state); break;
                case MSCAB_COMP.MSCAB_COMP_QUANTUM: qtmd_free((qtmd_stream)self.d.state); break;
                case MSCAB_COMP.MSCAB_COMP_LZX: lzxd_free((lzxd_stream)self.d.state); break;
            }

            //self.d.decompress = null;
            self.d.state = null;
        }

        #endregion
    }
}