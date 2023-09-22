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
#if NET48
        public HuffmanNode Left { get; set; }
#else
        public HuffmanNode? Left { get; set; }
#endif

        /// <summary>
        /// Right child of the current node
        /// </summary>
#if NET48
        public HuffmanNode Right { get; set; }
#else
        public HuffmanNode? Right { get; set; }
#endif

        /// <summary>
        /// Value of the current node
        /// </summary>
        public int Value { get; set; }
    }
}