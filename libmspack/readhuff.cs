namespace SabreTools.Compression.libmspack
{
    public unsafe static class readhuff
    {
        public const int HUFF_MAXBITS = 16;

        #region MSB

        /// <summary>
        /// This function was originally coded by David Tritscher.
        /// It builds a fast huffman decoding table from
        /// a canonical huffman code lengths table.
        /// </summary>
        /// <param name="nsyms">Total number of symbols in this huffman tree.</param>
        /// <param name="nbits">
        /// Any symbols with a code length of nbits or less can be decoded
        /// in one lookup of the table.
        /// </param>
        /// <param name="length">A table to get code lengths from [0 to nsyms-1]</param>
        /// <param name="table">
        /// The table to fill up with decoded symbols and pointers.
        /// Should be ((1<<nbits) + (nsyms*2)) in length.
        /// </param>
        /// <returns>Returns 0 for OK or 1 for error</returns>
        public static int make_decode_table_MSB(uint nsyms, uint nbits, byte* length, ushort* table)
        {
            ushort sym, next_symbol;
            uint leaf, fill;
            byte bit_num;
            uint pos = 0; // The current position in the decode table
            uint table_mask = (uint)(1 << (int)nbits);
            uint bit_mask = table_mask >> 1; // Don't do 0 length codes

            // Fill entries for codes short enough for a direct mapping
            for (bit_num = 1; bit_num <= nbits; bit_num++)
            {
                for (sym = 0; sym < nsyms; sym++)
                {
                    if (length[sym] != bit_num) continue;
                    leaf = pos;

                    if ((pos += bit_mask) > table_mask) return 1; // Table overrun

                    // Fill all possible lookups of this symbol with the symbol itself
                    for (fill = bit_mask; fill-- > 0;) table[leaf++] = sym;
                }
                bit_mask >>= 1;
            }

            // Exit with success if table is now complete
            if (pos == table_mask) return 0;

            // Mark all remaining table entries as unused
            for (sym = (ushort)pos; sym < table_mask; sym++)
            {
                table[sym] = 0xFFFF;
            }

            // next_symbol = base of allocation for long codes
            next_symbol = (ushort)(((table_mask >> 1) < nsyms) ? nsyms : (table_mask >> 1));

            // Give ourselves room for codes to grow by up to 16 more bits.
            // codes now start at bit nbits+16 and end at (nbits+16-codelength)
            pos <<= 16;
            table_mask <<= 16;
            bit_mask = 1 << 15;

            for (bit_num = (byte)(nbits + 1); bit_num <= HUFF_MAXBITS; bit_num++)
            {
                for (sym = 0; sym < nsyms; sym++)
                {
                    if (length[sym] != bit_num) continue;
                    if (pos >= table_mask) return 1; // Table overflow

                    leaf = pos >> 16;
                    for (fill = 0; fill < (bit_num - nbits); fill++)
                    {
                        // If this path hasn't been taken yet, 'allocate' two entries
                        if (table[leaf] == 0xFFFF)
                        {
                            table[(next_symbol << 1)] = 0xFFFF;
                            table[(next_symbol << 1) + 1] = 0xFFFF;
                            table[leaf] = next_symbol++;
                        }

                        // Follow the path and select either left or right for next bit
                        leaf = (uint)(table[leaf] << 1);
                        if (((pos >> (int)(15 - fill)) & 1) != 0) leaf++;
                    }
                    table[leaf] = sym;
                    pos += bit_mask;
                }
                bit_mask >>= 1;
            }

            // Full table?
            return (pos == table_mask) ? 0 : 1;
        }

        #endregion

        #region LSB

        /// <summary>
        /// This function was originally coded by David Tritscher.
        /// It builds a fast huffman decoding table from
        /// a canonical huffman code lengths table.
        /// </summary>
        /// <param name="nsyms">Total number of symbols in this huffman tree.</param>
        /// <param name="nbits">
        /// Any symbols with a code length of nbits or less can be decoded
        /// in one lookup of the table.
        /// </param>
        /// <param name="length">A table to get code lengths from [0 to nsyms-1]</param>
        /// <param name="table">
        /// The table to fill up with decoded symbols and pointers.
        /// Should be ((1<<nbits) + (nsyms*2)) in length.
        /// </param>
        /// <returns>Returns 0 for OK or 1 for error</returns>
        public static int make_decode_table_LSB(uint nsyms, uint nbits, byte* length, ushort* table)
        {
            ushort sym, next_symbol;
            uint leaf, fill;
            uint reverse;
            byte bit_num;
            uint pos = 0; // The current position in the decode table
            uint table_mask = (uint)(1 << (int)nbits);
            uint bit_mask = table_mask >> 1; // Don't do 0 length codes

            // Fill entries for codes short enough for a direct mapping
            for (bit_num = 1; bit_num <= nbits; bit_num++)
            {
                for (sym = 0; sym < nsyms; sym++)
                {
                    if (length[sym] != bit_num) continue;

                    // Reverse the significant bits
                    fill = length[sym]; reverse = pos >> (int)(nbits - fill); leaf = 0;
                    do { leaf <<= 1; leaf |= reverse & 1; reverse >>= 1; } while (--fill > 0);

                    if ((pos += bit_mask) > table_mask) return 1; // Table overrun

                    // Fill all possible lookups of this symbol with the symbol itself
                    fill = bit_mask; next_symbol = (ushort)(1 << bit_num);
                    do { table[leaf] = sym; leaf += next_symbol; } while (--fill > 0);
                }
                bit_mask >>= 1;
            }

            // Exit with success if table is now complete
            if (pos == table_mask) return 0;

            // Mark all remaining table entries as unused
            for (sym = (ushort)pos; sym < table_mask; sym++)
            {
                reverse = sym; leaf = 0; fill = nbits;
                do { leaf <<= 1; leaf |= reverse & 1; reverse >>= 1; } while (--fill > 0);
                table[leaf] = 0xFFFF;
            }

            // next_symbol = base of allocation for long codes
            next_symbol = (ushort)(((table_mask >> 1) < nsyms) ? nsyms : (table_mask >> 1));

            // Give ourselves room for codes to grow by up to 16 more bits.
            // codes now start at bit nbits+16 and end at (nbits+16-codelength)
            pos <<= 16;
            table_mask <<= 16;
            bit_mask = 1 << 15;

            for (bit_num = (byte)(nbits + 1); bit_num <= HUFF_MAXBITS; bit_num++)
            {
                for (sym = 0; sym < nsyms; sym++)
                {
                    if (length[sym] != bit_num) continue;
                    if (pos >= table_mask) return 1; // Table overflow

                    // leaf = the first nbits of the code, reversed
                    reverse = pos >> 16; leaf = 0; fill = nbits;
                    do { leaf <<= 1; leaf |= reverse & 1; reverse >>= 1; } while (--fill > 0);
                    for (fill = 0; fill < (bit_num - nbits); fill++)
                    {
                        // If this path hasn't been taken yet, 'allocate' two entries
                        if (table[leaf] == 0xFFFF)
                        {
                            table[(next_symbol << 1)] = 0xFFFF;
                            table[(next_symbol << 1) + 1] = 0xFFFF;
                            table[leaf] = next_symbol++;
                        }

                        // Follow the path and select either left or right for next bit
                        leaf = (uint)(table[leaf] << 1);
                        if (((pos >> (int)(15 - fill)) & 1) != 0) leaf++;
                    }
                    table[leaf] = sym;
                    pos += bit_mask;
                }
                bit_mask >>= 1;
            }

            // Full table?
            return (pos == table_mask) ? 0 : 1;
        }

        #endregion
    }
}