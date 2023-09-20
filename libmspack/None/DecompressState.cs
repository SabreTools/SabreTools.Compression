namespace SabreTools.Compression.libmspack.None
{
    public unsafe class DecompressState : mscabd_decompress_state
    {
        public DecompressState()
        {
            this.comp_type = MSCAB_COMP.MSCAB_COMP_NONE;
            this.state = null;
        }

        public DecompressState(mscabd_decompress_state oldstate)
        {
            this.comp_type = MSCAB_COMP.MSCAB_COMP_NONE;
            this.state = null;
            if (oldstate != null)
            {
                this.folder = oldstate.folder;
                this.data = oldstate.data;
                this.offset = oldstate.offset;
                this.block = oldstate.block;
                this.outlen = oldstate.outlen;
                this.sys = oldstate.sys;
                this.incab = oldstate.incab;
                this.infh = oldstate.infh;
                this.outfh = oldstate.outfh;
                this.i_ptr = oldstate.i_ptr;
                this.i_end = oldstate.i_end;
                this.input = oldstate.input;
            }
        }

        /// <inheritdoc/>
        public override unsafe MSPACK_ERR decompress(object data, long bytes)
        {
            State s = data as State;
            while (bytes > 0)
            {
                int run = (bytes > s.BufferSize) ? s.BufferSize : (int)bytes;
                if (s.InternalSystem.read(s.Input, s.Buffer, run) != run) return MSPACK_ERR.MSPACK_ERR_READ;
                if (s.InternalSystem.write(s.Output, s.Buffer, run) != run) return MSPACK_ERR.MSPACK_ERR_WRITE;
                bytes -= run;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }
    }
}