using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SabreTools.IO.Streams;
using SabreTools.Models.Compression.MSZIP;
using static SabreTools.Models.Compression.MSZIP.Constants;

namespace SabreTools.Compression.MSZIP
{
    /// <see href="https://www.rfc-editor.org/rfc/rfc1951"/>
    public class DeflateDecompressor
    {
        /// <summary>
        /// Internal bitstream to use for decompression
        /// </summary>
        private readonly ReadOnlyBitStream _bitStream;

        /// <summary>
        /// Create a new Decompressor from a byte array
        /// </summary>
        /// <param name="input">Byte array to decompress</param>
        public DeflateDecompressor(byte[]? input)
        {
            // If we have an invalid stream
            if (input == null || input.Length == 0)
                throw new ArgumentException(nameof(input));

            // Create a memory stream to wrap
            var ms = new MemoryStream(input);

            // Wrap the stream in a ReadOnlyBitStream
            _bitStream = new ReadOnlyBitStream(ms);
        }

        /// <summary>
        /// Create a new Decompressor from a Stream
        /// </summary>
        /// <param name="input">Stream to decompress</param>
        public DeflateDecompressor(Stream? input)
        {
            // If we have an invalid stream
            if (input == null || !input.CanRead || !input.CanSeek)
                throw new ArgumentException(nameof(input));

            // Wrap the stream in a ReadOnlyBitStream
            _bitStream = new ReadOnlyBitStream(input);
        }

        /// <summary>
        /// Decompress a stream into a <see cref="Block"/> 
        /// </summary>
        /// <returns>Block containing the decompressed data on success, null on error</returns>
        public Block? Process()
        {
            // Create a new block
            var block = new Block();

            // Try to read the header
            block.BlockHeader = ReadBlockHeader();
            if (block.BlockHeader.Signature != 0x4B43)
                return null;

            // Loop and read the deflate blocks
            var deflateBlocks = new List<DeflateBlock>();
            while (true)
            {
                // Try to read the deflate block
                var deflateBlock = ReadDeflateBlock();
                if (deflateBlock == null)
                    return null;

                // Add the deflate block to the set
                deflateBlocks.Add(deflateBlock);

                // If we're at the final block, exit out of the loop
                if (deflateBlock.Header!.BFINAL)
                    break;
            }

            // Assign the deflate blocks to the block and return
            block.CompressedBlocks = deflateBlocks.ToArray();
            return block;
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
        private (FixedCompressedDataHeader, uint, uint) RaadFixedCompressedDataHeader()
        {
            // Nothing needs to be read, all values are fixed
            return (new FixedCompressedDataHeader(), 288, 30);
        }

        /// <summary>
        /// Read a DynamicHuffmanCompressedBlockHeader from the input stream
        /// </summary>
        private (DynamicCompressedDataHeader, uint, uint) ReadDynamicCompressedDataHeader()
        {
            var header = new DynamicCompressedDataHeader();

            // Setup the counts first
            uint numLiteral = 257 + _bitStream.ReadBitsLSB(5) ?? 0;
            uint numDistance = 1 + _bitStream.ReadBitsLSB(5) ?? 0;
            uint numLength = 4 + _bitStream.ReadBitsLSB(4) ?? 0;

            // Convert the alphabet based on lengths
            uint[] lengthLengths = new uint[19];
            for (int i = 0; i < numLength; i++)
            {
                lengthLengths[BitLengthOrder[i]] = (byte)(_bitStream.ReadBitsLSB(3) ?? 0);
            }
            for (int i = (int)numLength; i < 19; i++)
            {
                lengthLengths[BitLengthOrder[i]] = 0;
            }

            // Make the lengths tree
            var lengthTree = new HuffmanDecoder(lengthLengths, 19);

            // Setup the literal and distance lengths
            header.LiteralLengths = new uint[288];
            header.DistanceCodes = new uint[32];

            // Read the literal and distance codes
            int repeatCode = 1;
            uint leftover = ReadHuffmanLengths(lengthTree, header.LiteralLengths, numLiteral, 0, ref repeatCode);
            _ = ReadHuffmanLengths(lengthTree, header.DistanceCodes, numDistance, leftover, ref repeatCode);

            return (header, numLiteral, numDistance);
        }

        #endregion

        #region Data

        /// <summary>
        /// Read an RFC1951 block
        /// </summary>
        private DeflateBlock? ReadDeflateBlock()
        {
            var deflateBlock = new DeflateBlock();

            // Try to read the deflate block header
            deflateBlock.Header = ReadDeflateBlockHeader();
            switch (deflateBlock.Header.BTYPE)
            {
                // If stored with no compression
                case CompressionType.NoCompression:
                    (var header00, var bytes00) = ReadNoCompression();
                    if (header00 == null || bytes00 == null)
                        return null;

                    deflateBlock.DataHeader = header00;
                    deflateBlock.Data = bytes00;
                    break;

                // If compressed with fixed Huffman codes
                case CompressionType.FixedHuffman:
                    (var header01, var bytes01) = ReadFixedHuffman();
                    if (header01 == null || bytes01 == null)
                        return null;

                    deflateBlock.DataHeader = header01;
                    deflateBlock.Data = bytes01;
                    break;

                // If compressed with dynamic Huffman codes
                case CompressionType.DynamicHuffman:
                    (var header10, var bytes10) = ReadDynamicHuffman();
                    if (header10 == null || bytes10 == null)
                        return null;

                    deflateBlock.DataHeader = header10;
                    deflateBlock.Data = bytes10;
                    break;

                // Reserved is not allowed and is treated as an error
                case CompressionType.Reserved:
                default:
                    return null;
            }

            return deflateBlock;
        }

        /// <summary>
        /// Read an RFC1951 block with no compression
        /// </summary>
        private (NonCompressedBlockHeader?, byte[]?) ReadNoCompression()
        {
            // Skip any remaining bits in current partially processed byte
            _bitStream.Discard();

            // Read LEN and NLEN
            var header = ReadNonCompressedBlockHeader();
            if (header.LEN == 0 && header.NLEN == 0)
                return (null, null);

            // Copy LEN bytes of data to output
            return (header, _bitStream.ReadBytes(header.LEN));
        }

        /// <summary>
        /// Read an RFC1951 block with fixed Huffman compression
        /// </summary>
        private (FixedCompressedDataHeader, byte[]?) ReadFixedHuffman()
        {
            var bytes = new List<byte>();

            // Get the fixed huffman header
            (var header, uint numLiteral, uint numDistance) = RaadFixedCompressedDataHeader();

            // Make the literal and distance trees
            var literalTree = new HuffmanDecoder(header.LiteralLengths, numLiteral);
            var distanceTree = new HuffmanDecoder(header.DistanceCodes, numDistance);

            // Now loop and decode
            return (header, ReadHuffmanBlock(literalTree, distanceTree));
        }

        /// <summary>
        /// Read an RFC1951 block with dynamic Huffman compression
        /// </summary>
        private (DynamicCompressedDataHeader?, byte[]?) ReadDynamicHuffman()
        {
            // Get the dynamic huffman header
            (var header, uint numLiteral, uint numDistance) = ReadDynamicCompressedDataHeader();

            // Make the literal and distance trees
            var literalTree = new HuffmanDecoder(header.LiteralLengths, numLiteral);
            var distanceTree = new HuffmanDecoder(header.DistanceCodes, numDistance);

            // Now loop and decode
            return (header, ReadHuffmanBlock(literalTree, distanceTree));
        }

        /// <summary>
        /// Read an RFC1951 block with Huffman compression
        /// </summary>
        private byte[]? ReadHuffmanBlock(HuffmanDecoder literalTree, HuffmanDecoder distanceTree)
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