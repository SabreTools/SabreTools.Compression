namespace SabreTools.Compression.libmspack
{
    public unsafe class qtmd_model
    {
        public int shiftsleft { get; set; }

        public int entries { get; set; }

        public qtmd_modelsym[] syms { get; set; }

        /// <summary>
        /// Initialises a model to decode symbols from [start] to [start]+[len]-1
        /// </summary>
        public qtmd_model(qtmd_modelsym[] syms, int start, int len)
        {
            this.shiftsleft = 4;
            this.entries = len;
            this.syms = syms;

            for (int i = 0; i <= len; i++)
            {
                syms[i].sym = (ushort)(start + i); // Actual symbol
                syms[i].cumfreq = (ushort)(len - i); // Current frequency of that symbol
            }
        }
    }
}