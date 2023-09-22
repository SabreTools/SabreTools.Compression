using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SabreTools.Models.Compression.MSZIP;
using static SabreTools.Compression.MSZIP.Constants;
using static SabreTools.Models.Compression.MSZIP.Constants;

namespace SabreTools.Compression.MSZIP
{
    /// <see href="https://www.rfc-editor.org/rfc/rfc1951"/>
    public class DeflateDecompressor
    {
        /// <summary>
        /// Internal bitstream to use for decompression
        /// </summary>
        private BitStream _bitStream;

        /// <summary>
        /// Create a new Decompressor from a Stream
        /// </summary>
        /// <param name="input">Stream to decompress</param>
        public DeflateDecompressor(Stream input)
        {
            // If we have an invalid stream
            if (input == null || !input.CanRead || !input.CanSeek)
                throw new ArgumentException(nameof(input));

            // Wrap the stream in a BitStream
            _bitStream = new BitStream(input);
        }

        /// <summary>
        /// Decompress a stream into a byte array
        /// </summary>
        /// <returns>Byte array containing the decompressed data on success, null on error</returns>
        public byte[] Process()
        {
            // Try to read the header
            var blockHeader = ReadBlockHeader();
            if (blockHeader.Signature != 0x4B43)
                return null;

            // Loop and read the internal blocks
            var bytes = new List<byte>();
            while (true)
            {
                // Try to read the deflate block header
                var deflateBlockHeader = ReadDeflateBlockHeader();
                switch (deflateBlockHeader.BTYPE)
                {
                    // If stored with no compression
                    case CompressionType.NoCompression:
                        byte[] bytes00 = ReadNoCompression();
                        bytes.AddRange(bytes00);
                        break;

                    // If compressed with fixed Huffman codes
                    case CompressionType.FixedHuffman:
                        byte[] bytes01 = ReadFixedHuffman();
                        bytes.AddRange(bytes01);
                        break;

                    // If compressed with dynamic Huffman codes
                    case CompressionType.DynamicHuffman:
                        byte[] bytes10 = ReadDynamicHuffman();
                        bytes.AddRange(bytes10);
                        break;

                    // Reserved is not allowed and is treated as an error
                    case CompressionType.Reserved:
                    default:
                        return null;
                }

                // If we're at the final block, exit out of the loop
                if (deflateBlockHeader.BFINAL)
                    break;
            }

            return bytes.ToArray();
        }

        #region Headers

        /// <summary>
        /// Read a BlockHeader from the input stream
        /// </summary>
        private BlockHeader ReadBlockHeader()
        {
            var header = new BlockHeader();
            header.Signature = _bitStream.ReadUInt16() ?? 0;
            return header;
        }

        /// <summary>
        /// Read a DeflateBlockHeader from the input stream
        /// </summary>
        private DeflateBlockHeader ReadDeflateBlockHeader()
        {
            var header = new DeflateBlockHeader();
            header.BFINAL = _bitStream.ReadBit() != 0x01;
            uint? btype = _bitStream.ReadBitsLSB(2) ?? 0b11;
            header.BTYPE = (CompressionType)btype;
            return header;
        }

        /// <summary>
        /// Read a NonCompressedBlockHeader from the input stream
        /// </summary>
        private NonCompressedBlockHeader ReadNonCompressedBlockHeader()
        {
            var header = new NonCompressedBlockHeader();
            header.LEN = _bitStream.ReadUInt16() ?? 0;
            header.NLEN = _bitStream.ReadUInt16() ?? 0;
            return header;
        }

        /// <summary>
        /// Read a FixedHuffmanCompressedBlockHeader from the input stream
        /// </summary>
        private (FixedHuffmanCompressedBlockHeader, uint, uint) ReadFixedHuffmanCompressedBlockHeader()
        {
            // Nothing needs to be read, all values are fixed
            return (new FixedHuffmanCompressedBlockHeader(), 288, 30);
        }

        /// <summary>
        /// Read a DynamicHuffmanCompressedBlockHeader from the input stream
        /// </summary>
        private (DynamicHuffmanCompressedBlockHeader, uint, uint) ReadDynamicHuffmanCompressedBlockHeader()
        {
            var header = new DynamicHuffmanCompressedBlockHeader();

            // Setup the counts first
            uint numLiteral = 257 + _bitStream.ReadBitsLSB(5) ?? 0;
            uint numDistance = 1 + _bitStream.ReadBitsLSB(5) ?? 0;
            uint numLength = 4 + _bitStream.ReadBitsLSB(4) ?? 0;

            // Convert the alphabet based on lengths
            uint[] lengthLengths = new uint[19];
            for (int i = 0; i < numLength; i++)
            {
                lengthLengths[BitLengthOrder[i]] = (byte)_bitStream.ReadBitsLSB(3);
            }
            for (int i = (int)numLength; i < 19; i++)
            {
                lengthLengths[BitLengthOrder[i]] = 0;
            }

            // Make the lengths tree
            HuffmanDecoder lengthTree = new HuffmanDecoder(lengthLengths, 19);

            // Setup the literal and distance lengths
            header.LiteralLengths = new int[288];
            header.DistanceCodes = new int[32];

            // Read the literal and distance codes
            int repeatCode = 1;
            uint leftover = ReadHuffmanLengths(lengthTree, header.LiteralLengths, numLiteral, 0, ref repeatCode);
            _ = ReadHuffmanLengths(lengthTree, header.DistanceCodes, numDistance, leftover, ref repeatCode);

            return (header, numLiteral, numDistance);
        }

        #endregion

        #region Data

        /// <summary>
        /// Read an RFC1951 block with no compression
        /// </summary>
        private byte[] ReadNoCompression()
        {
            // Skip any remaining bits in current partially processed byte
            _bitStream.Discard();

            // Read LEN and NLEN
            var nonCompressedBlockHeader = ReadNonCompressedBlockHeader();
            if (nonCompressedBlockHeader.LEN == 0 && nonCompressedBlockHeader.NLEN == 0)
                return null;

            // Copy LEN bytes of data to output
            return _bitStream.ReadBytes(nonCompressedBlockHeader.LEN);
        }

        /// <summary>
        /// Read an RFC1951 block with fixed Huffman compression
        /// </summary>
        private byte[] ReadFixedHuffman()
        {
            var bytes = new List<byte>();

            // Get the fixed huffman header
            (var header, uint numLiteral, uint numDistance) = ReadFixedHuffmanCompressedBlockHeader();

            // Make the literal and distance trees
            HuffmanDecoder literalTree = new HuffmanDecoder(header.LiteralLengths, numLiteral);
            HuffmanDecoder distanceTree = new HuffmanDecoder(header.DistanceCodes, numDistance);

            // Now loop and decode
            return ReadHuffmanBlock(literalTree, distanceTree);
        }

        /// <summary>
        /// Read an RFC1951 block with dynamic Huffman compression
        /// </summary>
        private byte[] ReadDynamicHuffman()
        {
            // Get the dynamic huffman header
            (var header, uint numLiteral, uint numDistance) = ReadDynamicHuffmanCompressedBlockHeader();

            // Make the literal and distance trees
            HuffmanDecoder literalTree = new HuffmanDecoder(header.LiteralLengths, numLiteral);
            HuffmanDecoder distanceTree = new HuffmanDecoder(header.DistanceCodes, numDistance);

            // Now loop and decode
            return ReadHuffmanBlock(literalTree, distanceTree);
        }

        /// <summary>
        /// Read an RFC1951 block with Huffman compression
        /// </summary>
        private byte[] ReadHuffmanBlock(HuffmanDecoder literalTree, HuffmanDecoder distanceTree)
        {
            // Now loop and decode
            var bytes = new List<byte>();
            while (true)
            {
                // Decode the next literal value
                int sym = literalTree.Decode(_bitStream);

                // If we have an immediate symbol
                if (sym < 256)
                {
                    bytes.Add((byte)sym);
                }

                // If we have the ending symbol
                else if (sym == 256)
                {
                    break;
                }

                // If we have a length/distance pair
                else
                {
                    sym -= 257;
                    uint? length = CopyLengths[sym] + _bitStream.ReadBitsLSB(LiteralExtraBits[sym]);
                    if (length == null)
                        return null;

                    int distanceCode = distanceTree.Decode(_bitStream);

                    uint? distance = CopyOffsets[distanceCode] + _bitStream.ReadBitsLSB(DistanceExtraBits[distanceCode]);
                    if (distance == null)
                        return null;

                    byte[] arr = bytes.Skip(bytes.Count - (int)distance).Take((int)length).ToArray();
                    bytes.AddRange(arr);
                }
            }

            // Return the decoded array
            return bytes.ToArray();
        }

        /// <summary>
        /// Read the huffman lengths
        /// </summary>
        private uint ReadHuffmanLengths(HuffmanDecoder lengthTree, int[] lengths, uint numCodes, uint repeat, ref int repeatCode)
        {
            int i = 0;

            // First fill in any repeat codes
            while (repeat > 0)
            {
                lengths[i++] = (byte)repeatCode;
                repeat--;
            }

            // Then process the rest of the table
            while (i < numCodes)
            {
                // Get the next length encoding from the stream
                int lengthEncoding = lengthTree.Decode(_bitStream);

                // Values less than 16 are encoded directly
                if (lengthEncoding < 16)
                {
                    lengths[i++] = (byte)lengthEncoding;
                    repeatCode = lengthEncoding;
                }

                // Otherwise, the repeat count is based on the next values
                else
                {
                    // Determine the repeat count and code from the encoding
                    if (lengthEncoding == 16)
                    {
                        repeat = 3 + _bitStream.ReadBitsLSB(2) ?? 0;
                    }
                    else if (lengthEncoding == 17)
                    {
                        repeat = 3 + _bitStream.ReadBitsLSB(3) ?? 0;
                        repeatCode = 0;
                    }
                    else if (lengthEncoding == 18)
                    {
                        repeat = 11 + _bitStream.ReadBitsLSB(7) ?? 0;
                        repeatCode = 0;
                    }

                    // Read in the expected lengths
                    while (i < numCodes && repeat > 0)
                    {
                        lengths[i++] = (byte)repeatCode;
                        repeat--;
                    }
                }
            }

            // Return any repeat value we have left over
            return repeat;
        }

        /// <summary>
        /// Read the huffman lengths
        /// </summary>
        private uint ReadHuffmanLengths(HuffmanDecoder lengthTree, uint[] lengths, uint numCodes, uint repeat, ref int repeatCode)
        {
            int i = 0;

            // First fill in any repeat codes
            while (repeat > 0)
            {
                lengths[i++] = (byte)repeatCode;
                repeat--;
            }

            // Then process the rest of the table
            while (i < numCodes)
            {
                // Get the next length encoding from the stream
                int lengthEncoding = lengthTree.Decode(_bitStream);

                // Values less than 16 are encoded directly
                if (lengthEncoding < 16)
                {
                    lengths[i++] = (byte)lengthEncoding;
                    repeatCode = lengthEncoding;
                }

                // Otherwise, the repeat count is based on the next values
                else
                {
                    // Determine the repeat count and code from the encoding
                    if (lengthEncoding == 16)
                    {
                        repeat = 3 + _bitStream.ReadBitsLSB(2) ?? 0;
                    }
                    else if (lengthEncoding == 17)
                    {
                        repeat = 3 + _bitStream.ReadBitsLSB(3) ?? 0;
                        repeatCode = 0;
                    }
                    else if (lengthEncoding == 18)
                    {
                        repeat = 11 + _bitStream.ReadBitsLSB(7) ?? 0;
                        repeatCode = 0;
                    }

                    // Read in the expected lengths
                    while (i < numCodes && repeat > 0)
                    {
                        lengths[i++] = (byte)repeatCode;
                        repeat--;
                    }
                }
            }

            // Return any repeat value we have left over
            return repeat;
        }

        #endregion
    }
}