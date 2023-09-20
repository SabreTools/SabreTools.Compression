namespace SabreTools.Compression.libmspack.None
{
    /// <summary>
    /// The "not compressed" method decompressor
    /// </summary>
    public unsafe class State
    {
        public mspack_system InternalSystem { get; private set; }

        public mspack_file Input { get; private set; }

        public mspack_file Output { get; private set; }

        public byte* Buffer { get; private set; }

        public int BufferSize { get; private set; }

        public State(mspack_system sys, mspack_file infh, mspack_file outfh, int bufsize)
        {
            this.InternalSystem = sys;
            this.Input = infh;
            this.Output = outfh;
            this.Buffer = (byte*)new FixedArray<byte>(bufsize).Pointer;
            this.BufferSize = bufsize;
        }

        ~State()
        {
            mspack_system sys = this.InternalSystem;
            sys.free(this.Buffer);
            //sys.free(this);
        }
    }
}