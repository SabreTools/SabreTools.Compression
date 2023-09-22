using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Models.Compression.MSZIP;

namespace SabreTools.Compression.MSZIP
{
    public class Decompressor
    {
        /// <summary>
        /// Decompress a stream into a byte array
        /// </summary>
        /// <param name="input">Stream to decompress</param>
        /// <returns>Byte array containing the decompressed data on success, null on error</returns>
        /// <see href="https://www.rfc-editor.org/rfc/rfc1951"/>
        public static byte[] Decompress(Stream input)
        {
            // If we have an invalid stream
            if (input == null || !input.CanRead || !input.CanSeek)
                return null;

            // Wrap the stream in a BitStream
            var bitStream = new BitStream(input);

            // Try to read the header
            var blockHeader = ReadBlockHeader(bitStream);
            if (blockHeader.Signature != 0x4B43)
                return null;

            // Loop and read the internal blocks
            var bytes = new List<byte>();
            while (true)
            {
                // Try to read the deflate block header
                var deflateBlockHeader = ReadDeflateBlockHeader(bitStream);
                if (deflateBlockHeader.BTYPE == CompressionType.Reserved)
                    break;

                // If stored with no compression
                if (deflateBlockHeader.BTYPE == CompressionType.NoCompression)
                {
                    // Skip any remaining bits in current partially processed byte
                    bitStream.Discard();

                    // Read LEN and NLEN
                    var nonCompressedBlockHeader = ReadNonCompressedBlockHeader(bitStream);
                    if (nonCompressedBlockHeader.LEN == 0 && nonCompressedBlockHeader.NLEN == 0)
                        break;

                    // Copy LEN bytes of data to output
                    byte[] uncompressed = bitStream.ReadBytes(nonCompressedBlockHeader.LEN);
                    bytes.AddRange(uncompressed);
                }

                // Otherwise
                else
                {
                    // If compressed with dynamic Huffman codes
                    if (deflateBlockHeader.BTYPE == CompressionType.DynamicHuffman)
                    {
                        // Read representation of code trees
                    }

                    // Loop (until end of block code recognized)
                    while (true)
                    {
                        /*
                            decode literal/length value from input stream
                            if value < 256
                                copy value (literal byte) to output stream
                            otherwise
                                if value = end of block (256)
                                break from loop
                                otherwise (value = 257..285)
                                decode distance from input stream

                                move backwards distance bytes in the output
                                stream, and copy length bytes from this
                                position to the output stream.
                        */
                    }
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a BlockHeader from the input stream
        /// </summary>
        private static BlockHeader ReadBlockHeader(BitStream input)
        {
            var header = new BlockHeader();
            header.Signature = input.ReadUInt16() ?? 0;
            return header;
        }

        /// <summary>
        /// Read a DeflateBlockHeader from the input stream
        /// </summary>
        private static DeflateBlockHeader ReadDeflateBlockHeader(BitStream input)
        {
            var header = new DeflateBlockHeader();
            header.BFINAL = input.ReadBitLSB() != 0x01;
            byte btype = input.ReadBitLSB() ?? 0x01;
            btype <<= 1;
            btype |= input.ReadBitLSB() ?? 0x01;
            header.BTYPE = (CompressionType)btype;
            return header;
        }

        /// <summary>
        /// Read a NonCompressedBlockHeader from the input stream
        /// </summary>
        private static NonCompressedBlockHeader ReadNonCompressedBlockHeader(BitStream input)
        {
            var header = new NonCompressedBlockHeader();
            header.LEN = input.ReadUInt16() ?? 0;
            header.NLEN = input.ReadUInt16() ?? 0;
            return header;
        }
    }
}