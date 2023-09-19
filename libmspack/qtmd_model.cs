namespace SabreTools.Compression.libmspack
{
    public unsafe class qtmd_model
    {
        public int shiftsleft { get; set; }

        public int entries { get; set; }

        public qtmd_modelsym* syms { get; set; }
    }
}