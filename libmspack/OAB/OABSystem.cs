namespace SabreTools.Compression.libmspack.OAB
{
    public unsafe class OABSystem : mspack_default_system
    {
        /// <inheritdoc/>
        public override unsafe int read(mspack_file base_file, void* buf, int size)
        {
            oabd_file file = (oabd_file)base_file;
            int bytes_read;

            if (size > file.available)
                size = file.available;

            bytes_read = file.orig_sys.read(file.orig_file, buf, size);
            if (bytes_read < 0)
                return bytes_read;

            file.available -= bytes_read;
            return bytes_read;
        }

        /// <inheritdoc/>
        public override unsafe int write(mspack_file base_file, void* buf, int size)
        {
            oabd_file file = (oabd_file)base_file;
            int bytes_written = file.orig_sys.write(file.orig_file, buf, size);

            if (bytes_written > 0)
                file.crc = mspack.crc32(file.crc, buf, bytes_written);

            return bytes_written;
        }
    }
}