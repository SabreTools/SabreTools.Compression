using static SabreTools.Compression.libmspack.CAB.Constants;

namespace SabreTools.Compression.libmspack.CAB
{
    public unsafe class CABSystem : mspack_default_system
    {
        /// <summary>
        /// cabd_sys_read is the internal reader function which the decompressors
        /// use. will read data blocks (and merge split blocks) from the cabinet
        /// and serve the read bytes to the decompressors
        /// </summary>
        public override int read(mspack_file file, void* buffer, int bytes)
        {
            Decompressor self = (Decompressor)file;
            byte* buf = (byte*)buffer;
            mspack_system sys = self.system;
            int avail, todo, outlen = 0, ignore_cksum, ignore_blocksize;

            ignore_cksum = self.salvage != 0 || (self.fix_mszip != 0 && ((MSCAB_COMP)((int)self.d.comp_type & cffoldCOMPTYPE_MASK) == MSCAB_COMP.MSCAB_COMP_MSZIP)) == true ? 1 : 0;
            ignore_blocksize = self.salvage;

            todo = bytes;
            while (todo > 0)
            {
                avail = (int)(self.d.i_end - self.d.i_ptr);

                // If out of input data, read a new block
                if (avail != 0)
                {
                    // Copy as many input bytes available as possible
                    if (avail > todo) avail = todo;
                    sys.copy(self.d.i_ptr, buf, avail);
                    self.d.i_ptr += avail;
                    buf += avail;
                    todo -= avail;
                }
                else
                {
                    // Out of data, read a new block

                    // Check if we're out of input blocks, advance block counter
                    if (self.d.block++ >= self.d.folder.num_blocks)
                    {
                        if (self.salvage == 0)
                        {
                            self.read_error = MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                        }
                        else
                        {
                            System.Console.Error.WriteLine("Ran out of CAB input blocks prematurely");
                        }
                        break;
                    }

                    // Read a block
                    self.read_error = ReadBlock(sys, self.d, ref outlen, ignore_cksum, ignore_blocksize);
                    if (self.read_error != MSPACK_ERR.MSPACK_ERR_OK) return -1;
                    self.d.outlen += outlen;

                    // Special Quantum hack -- trailer byte to allow the decompressor
                    // to realign itself. CAB Quantum blocks, unlike LZX blocks, can have
                    // anything from 0 to 4 trailing null bytes.
                    if ((MSCAB_COMP)((int)self.d.comp_type & cffoldCOMPTYPE_MASK) == MSCAB_COMP.MSCAB_COMP_QUANTUM)
                    {
                        *self.d.i_end++ = 0xFF;
                    }

                    // Is this the last block?
                    if (self.d.block >= self.d.folder.num_blocks)
                    {
                        if ((MSCAB_COMP)((int)self.d.comp_type & cffoldCOMPTYPE_MASK) == MSCAB_COMP.MSCAB_COMP_LZX)
                        {
                            // Special LZX hack -- on the last block, inform LZX of the
                            // size of the output data stream.
                            lzxd_set_output_length((lzxd_stream)self.d.state, self.d.outlen);
                        }
                    }
                } /* if (avail) */
            } /* while (todo > 0) */
            return bytes - todo;
        }

        /// <summary>
        /// cabd_sys_write is the internal writer function which the decompressors
        /// use. it either writes data to disk (self.d.outfh) with the real
        /// sys.write() function, or does nothing with the data when
        /// self.d.outfh == null. advances self.d.offset
        /// </summary>
        public override int write(mspack_file file, void* buffer, int bytes)
        {
            Decompressor self = (Decompressor)file;
            self.d.offset += (uint)bytes;
            if (self.d.outfh != null)
            {
                return self.system.write(self.d.outfh, buffer, bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Reads a whole data block from a cab file. the block may span more than
        /// one cab file, if it does then the fragments will be reassembled
        /// </summary>
        private static MSPACK_ERR ReadBlock(mspack_system sys, mscabd_decompress_state d, ref int @out, int ignore_cksum, int ignore_blocksize)
        {
            byte[] hdr = new byte[cfdata_SIZEOF];
            uint cksum;
            int len, full_len;

            // Reset the input block pointer and end of block pointer
            d.i_ptr = d.i_end = &d.input[0];

            do
            {
                // Read the block header
                if (sys.read(d.infh, &hdr[0], cfdata_SIZEOF) != cfdata_SIZEOF)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }

                // Skip any reserved block headers
                if (d.data.cab.block_resv != 0 &&
                    sys.seek(d.infh, d.data.cab.block_resv, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR) != 0)
                {
                    return MSPACK_ERR.MSPACK_ERR_SEEK;
                }

                // Blocks must not be over CAB_INPUTMAX in size
                len = System.BitConverter.ToUInt16(hdr, cfdata_CompressedSize);
                full_len = (int)(d.i_end - d.i_ptr + len); // Include cab-spanning blocks */
                if (full_len > CAB_INPUTMAX)
                {
                    System.Console.Error.WriteLine($"Block size {full_len} > CAB_INPUTMAX");
                    // In salvage mode, blocks can be 65535 bytes but no more than that
                    if (ignore_blocksize == 0 || full_len > CAB_INPUTMAX_SALVAGE)
                    {
                        return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                    }
                }

                // Blocks must not expand to more than CAB_BLOCKMAX
                if (System.BitConverter.ToUInt16(hdr, cfdata_UncompressedSize) > CAB_BLOCKMAX)
                {
                    System.Console.Error.WriteLine("block size > CAB_BLOCKMAX");
                    if (ignore_blocksize == 0) return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                }

                // Read the block data
                if (sys.read(d.infh, d.i_end, len) != len)
                {
                    return MSPACK_ERR.MSPACK_ERR_READ;
                }

                // Perform checksum test on the block (if one is stored)
                if ((cksum = System.BitConverter.ToUInt32(hdr, cfdata_CheckSum)) != 0)
                {
                    uint sum2 = Checksum(d.i_end, (uint)len, 0);
                    if (Checksum(&hdr[4], 4, sum2) != cksum)
                    {
                        if (ignore_cksum == 0) return MSPACK_ERR.MSPACK_ERR_CHECKSUM;
                        sys.message(d.infh, "WARNING; bad block checksum found");
                    }
                }

                // Advance end of block pointer to include newly read data
                d.i_end += len;

                // Uncompressed size == 0 means this block was part of a split block
                // and it continues as the first block of the next cabinet in the set.
                // otherwise, this is the last part of the block, and no more block
                // reading needs to be done.

                // EXIT POINT OF LOOP -- uncompressed size != 0
                if ((@out = System.BitConverter.ToInt16(hdr, cfdata_UncompressedSize)) != 0)
                {
                    return MSPACK_ERR.MSPACK_ERR_OK;
                }

                // Otherwise, advance to next cabinet

                // Close current file handle
                sys.close(d.infh);
                d.infh = null;

                // Aadvance to next member in the cabinet set
                if ((d.data = d.data.next) == null)
                {
                    sys.message(d.infh, "WARNING; ran out of cabinets in set. Are any missing?");
                    return MSPACK_ERR.MSPACK_ERR_DATAFORMAT;
                }

                // Open next cab file
                d.incab = d.data.cab;
                if ((d.infh = sys.open(d.incab.filename, MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ)) == null)
                {
                    return MSPACK_ERR.MSPACK_ERR_OPEN;
                }

                // Seek to start of data blocks
                if (sys.seek(d.infh, d.data.offset, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
                {
                    return MSPACK_ERR.MSPACK_ERR_SEEK;
                }
            } while (true);

            // Not reached
            return MSPACK_ERR.MSPACK_ERR_OK;
        }

        private static uint Checksum(byte* data, uint bytes, uint cksum)
        {
            uint len, ul = 0;

            for (len = bytes >> 2; len-- > 0; data += 4)
            {
                cksum ^= System.BitConverter.ToInt32(data, 0);
            }

            switch (bytes & 3)
            {
                case 3: ul |= (uint)(*data++ << 16); goto case 2;
                case 2: ul |= (uint)(*data++ << 8); goto case 1;
                case 1: ul |= *data; break;
            }
            cksum ^= ul;

            return cksum;
        }
    }
}