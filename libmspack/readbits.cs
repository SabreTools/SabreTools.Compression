namespace SabreTools.Compression.libmspack
{
    public unsafe abstract class readbits
    {
        /// <summary>
        /// I/O routines
        /// </summary>
        public mspack_system sys { get; set; }

        /// <summary>
        /// Input file handle
        /// </summary>
        public mspack_file input { get; set; }

        /// <summary>
        /// Output file handle
        /// </summary>
        public mspack_file output { get; set; }

        /// <summary>
        /// Decompression offset within window
        /// </summary>
        public uint window_posn { get; set; }

        #region I/O buffering

        public byte* inbuf { get; set; }

        public byte* i_ptr { get; set; }

        public byte* i_end { get; set; }

        public byte* o_ptr { get; set; }
        
        public byte* o_end { get; set; }

        public int input_end { get; set; }

        public uint bit_buffer { get; set; }

        public uint bits_left { get; set; }

        public uint inbuf_size { get; set; }

        #endregion

        public MSPACK_ERR error { get; set; }

        /// <see href="https://github.com/kyz/libmspack/blob/master/libmspack/mspack/readbits.h"/> 
        #region readbits.h

        private const int BITBUF_WIDTH = 64;

        private static readonly ushort[] lsb_bit_mask = new ushort[17]
        {
            0x0000, 0x0001, 0x0003, 0x0007, 0x000f, 0x001f, 0x003f, 0x007f, 0x00ff,
            0x01ff, 0x03ff, 0x07ff, 0x0fff, 0x1fff, 0x3fff, 0x7fff, 0xffff
        };

        public void INIT_BITS()
        {
            this.i_ptr = inbuf;
            this.i_end = inbuf;
            this.bit_buffer = 0;
            this.bits_left = 0;
            this.input_end = 0;
        }

        public void STORE_BITS(byte* i_ptr, byte* i_end, uint bit_buffer, uint bits_left)
        {
            this.i_ptr = i_ptr;
            this.i_end = i_end;
            this.bit_buffer = bit_buffer;
            this.bits_left = bits_left;
        }

        public void RESTORE_BITS(out byte* i_ptr, out byte* i_end, out uint bit_buffer, out uint bits_left)
        {
            i_ptr = this.i_ptr;
            i_end = this.i_end;
            bit_buffer = this.bit_buffer;
            bits_left = this.bits_left;
        }

        public void ENSURE_BITS(byte nbits, ref uint bits_left)
        {
            while (bits_left < nbits)
            {
                this.READ_BYTES();
            }
        }

        #region MSB

        public void READ_BITS_MSB(out int val, byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            this.ENSURE_BITS(nbits, ref bits_left);
            val = PEEK_BITS_MSB(nbits, bit_buffer);
            REMOVE_BITS_MSB(nbits, ref bit_buffer, ref bits_left);
        }

        public void READ_MANY_BITS_MSB(out int val, byte bits, ref uint bit_buffer, ref uint bits_left)
        {
            byte needed = bits;
            byte bitrun;
            val = 0;
            while (needed > 0)
            {
                if (bits_left < (int)(BITBUF_WIDTH - 16))
                    this.READ_BYTES();

                bitrun = (bits_left < needed) ? (byte)bits_left : needed;
                val = (val << bitrun) | PEEK_BITS_MSB(bitrun, bit_buffer);
                REMOVE_BITS_MSB(bitrun, ref bit_buffer, ref bits_left);
                needed -= bitrun;
            }
        }

        public int PEEK_BITS_MSB(byte nbits, uint bit_buffer)
        {
            return (int)(bit_buffer >> (BITBUF_WIDTH - nbits));
        }

        public void REMOVE_BITS_MSB(byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            bit_buffer <<= nbits;
            bits_left -= nbits;
        }

        public void INJECT_BITS_MSB(uint bitdata, byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            bit_buffer |= (uint)(int)(bitdata << (int)(BITBUF_WIDTH - nbits - bits_left));
            bits_left += nbits;
        }

        #endregion

        #region LSB

        public void READ_BITS_LSB(out int val, byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            this.ENSURE_BITS(nbits, ref bits_left);
            val = PEEK_BITS_LSB(nbits, bit_buffer);
            REMOVE_BITS_LSB(nbits, ref bit_buffer, ref bits_left);
        }

        public void READ_MANY_BITS_LSB(out int val, byte bits, ref uint bit_buffer, ref uint bits_left)
        {
            byte needed = bits;
            byte bitrun;
            val = 0;
            while (needed > 0)
            {
                if (bits_left < (int)(BITBUF_WIDTH - 16))
                    this.READ_BYTES();

                bitrun = (bits_left < needed) ? (byte)bits_left : needed;
                val = (val << bitrun) | PEEK_BITS_LSB(bitrun, bit_buffer);
                REMOVE_BITS_LSB(bitrun, ref bit_buffer, ref bits_left);
                needed -= bitrun;
            }
        }

        public int PEEK_BITS_LSB(byte nbits, uint bit_buffer)
        {
            return (int)(bit_buffer & ((uint)(1 << nbits) - 1));
        }

        public void REMOVE_BITS_LSB(byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            bit_buffer >>= nbits;
            bits_left -= nbits;
        }

        public void INJECT_BITS_LSB(uint bitdata, byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            bit_buffer |= bitdata << (int)bits_left;
            bits_left += nbits;
        }

        #endregion

        #region LSB_T

        public int PEEK_BITS_LSB_T(byte nbits, uint bit_buffer)
        {
            return (int)(bit_buffer & lsb_bit_mask[nbits]);
        }

        public void READ_BITS_LSB_T(out int val, byte nbits, ref uint bit_buffer, ref uint bits_left)
        {
            this.ENSURE_BITS(nbits, ref bits_left);
            val = PEEK_BITS_LSB_T(nbits, bit_buffer);
            REMOVE_BITS_LSB(nbits, ref bit_buffer, ref bits_left);
        }

        #endregion

        public abstract void READ_BYTES();

        public MSPACK_ERR READ_IF_NEEDED(ref byte* i_ptr, ref byte* i_end)
        {
            if (i_ptr >= i_end)
            {
                if (read_input() != MSPACK_ERR.MSPACK_ERR_OK)
                    return this.error;

                i_ptr = this.i_ptr;
                i_end = this.i_end;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        private MSPACK_ERR read_input()
        {
            int read = this.sys.read(this.input, this.inbuf, (int)this.inbuf_size);
            if (read < 0) return this.error = MSPACK_ERR.MSPACK_ERR_READ;

            /* we might overrun the input stream by asking for bits we don't use,
             * so fake 2 more bytes at the end of input */
            if (read == 0)
            {
                if (this.input_end != 0)
                {
                    System.Console.Error.WriteLine("Out of input bytes");
                    return this.error = MSPACK_ERR.MSPACK_ERR_READ;
                }
                else
                {
                    read = 2;
                    this.inbuf[0] = this.inbuf[1] = 0;
                    this.input_end = 1;
                }
            }

            // Update i_ptr and i_end
            this.i_ptr = this.inbuf;
            this.i_end = this.inbuf + read;
            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        #endregion
    }
}