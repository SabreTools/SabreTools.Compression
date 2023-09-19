using System.IO;

namespace SabreTools.Compression.libmspack
{
    public class mspack_file_p : mspack_file
    {
        public Stream fh { get; set; }

        public string name { get; set; }
    }
}