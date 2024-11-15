using System;
using System.IO;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using SabreTools.IO.Streams;

namespace SabreTools.Compression.MSZIP
{
    public class HuffmanDecoder
    {
        /// <summary>
        /// Root Huffman node for the tree
        /// </summary>
        private HuffmanNode _root;

        /// <summary>
        /// Create a Huffman tree to decode with
        /// </summary>
        /// <param name="lengths">Array representing the number of bits for each value</param>
        /// <param name="numCodes">Number of Huffman codes encoded</param>
        public HuffmanDecoder(uint[]? lengths, uint numCodes)
        {
            // Ensure we have lengths
            if (lengths == null)
                throw new ArgumentNullException(nameof(lengths));

            // Set the root to null for now
            HuffmanNode? root = null;

            // Determine the value for max_bits
#if NET20
            uint max_bits = 0;
            foreach (uint u in lengths)
            {
                max_bits = u > max_bits ? u : max_bits;
            }
#else
            uint max_bits = lengths.Max();
#endif

            // Count the number of codes for each code length
            int[] bl_count = new int[max_bits + 1];
            for (int i = 0; i < numCodes; i++)
            {
                uint length = lengths[i];
                bl_count[length]++;
            }

            // Find the numerical value of the smalles code for each code length
            int[] next_code = new int[max_bits + 1];
            int code = 0;
            bl_count[0] = 0;
            for (int bits = 1; bits <= max_bits; bits++)
            {
                code = (code + bl_count[bits - 1]) << 1;
                next_code[bits] = code;
            }

            // Assign numerical values to all codes, using consecutive
            // values for all codes of the same length with the base
            // values determined at step 2. Codes that are never used
            // (which have a bit length of zero) must not be assigned a value.
            int[] tree = new int[numCodes];
            for (int i = 0; i < numCodes; i++)
            {
                uint len = lengths[i];
                if (len == 0)
                    continue;

                // Set the value in the tree
                tree[i] = next_code[len];
                next_code[len]++;
            }

            // Now insert the values into the structure
            for (int i = 0; i < numCodes; i++)
            {
                // If we have a 0-length code
                uint len = lengths[i];
                if (len == 0)
                    continue;

                // Insert the value starting at the root
                _root = Insert(_root, i, len, tree[i]);
            }

            // Assign the root value
            _root = root!;
        }

        /// <summary>
        /// Decode the next value from the stream as a Huffman-encoded value
        /// </summary>
        /// <param name="input">BitStream representing the input</param>
        /// <returns>Value of the node described by the input</returns>
        public int Decode(ReadOnlyBitStream input)
        {
            // Start at the root of the tree
            var node = _root;
            while (node?.Left != null)
            {
                // Read the next bit to determine direction
                byte? nextBit = input.ReadBit();
                if (nextBit == null)
                    throw new EndOfStreamException();

                // Left == 0, Right == 1
                if (nextBit == 0)
                    node = node.Left;
                else
                    node = node.Right;
            }

            // We traversed to the bottom of the branch
            return node?.Value ?? 0;
        }

        /// <summary>
        /// Insert a value based on an existing Huffman node
        /// </summary>
        /// <param name="node">Existing node to append to, or null if root</param>
        /// <param name="value">Value to append to the tree</param>
        /// <param name="length">Length of the current encoding</param>
        /// <param name="code">Encoding of the value to traverse</param>
        /// <returns>New instance of the node with value appended</returns>
        private static HuffmanNode Insert(HuffmanNode? node, int value, uint length, int code)
        {
            // If no node is provided, create a new one
            if (node == null)
                node = new HuffmanNode();

            // If we're at the correct location, insert the value
            if (length == 0)
            {
                node.Value = value;
                return node;
            }

            // Otherwise, get the next bit from the code
            byte nextBit = (byte)(code >> (int)(length - 1) & 1);

            // Left == 0, Right == 1
            if (nextBit == 0)
                node.Left = Insert(node.Left, value, length - 1, code);
            else
                node.Right = Insert(node.Right, value, length - 1, code);

            // Now return the node
            return node;
        }
    }
}