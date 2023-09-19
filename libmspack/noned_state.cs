namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// The "not compressed" method decompressor
    /// </summary>
    public unsafe class noned_state
    {
        public mspack_system sys { get; set; }

        public mspack_file i { get; set; }

        public mspack_file o { get; set; }

        public byte* buf { get; set; }

        public int bufsize { get; set; }
    }
}