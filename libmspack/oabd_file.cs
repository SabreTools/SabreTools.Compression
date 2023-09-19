namespace SabreTools.Compression.libmspack
{
    public class oabd_file : mspack_file
    {
        public mspack_system orig_sys { get; set; }

        public mspack_file orig_file { get; set; }

        public uint crc { get; set; }

        public int available { get; set; }
    }
}