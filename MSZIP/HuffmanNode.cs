namespace SabreTools.Compression.MSZIP
{
    /// <summary>
    /// Represents a single node in a Huffman tree
    /// </summary>
    public class HuffmanNode
    {
        /// <summary>
        /// Left child of the current node
        /// </summary>
        public HuffmanNode Left { get; set; }

        /// <summary>
        /// Right child of the current node
        /// </summary>
        public HuffmanNode Right { get; set; }

        /// <summary>
        /// Value of the current node
        /// </summary>
        public int Value { get; set; }
    }
}