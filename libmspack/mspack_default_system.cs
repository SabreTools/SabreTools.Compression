using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SabreTools.Compression.libmspack
{
    public class mspack_default_system : mspack_system
    {
        /// <inheritdoc/>
        public override mspack_file open(in string filename, MSPACK_SYS_OPEN mode)
        {
            FileMode fmode;
            FileAccess faccess;

            switch (mode)
            {
                case MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_READ: fmode = FileMode.Open; faccess = FileAccess.Read; break;
                case MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_WRITE: fmode = FileMode.OpenOrCreate; faccess = FileAccess.Write; break;
                case MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_UPDATE: fmode = FileMode.Open; faccess = FileAccess.ReadWrite; break;
                case MSPACK_SYS_OPEN.MSPACK_SYS_OPEN_APPEND: fmode = FileMode.Append; faccess = FileAccess.ReadWrite; break;
                default: return null;
            }

            try
            {
                var fh = new mspack_file_p
                {
                    name = filename,
                    fh = File.Open(filename, fmode, faccess),
                };
                return fh;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override void close(mspack_file file)
        {
            if (file is mspack_file_p self)
                self.fh?.Dispose();
        }

        /// <inheritdoc/>
        public override unsafe int read(mspack_file file, void* buffer, int bytes)
        {
            try
            {
                if (file is mspack_file_p self && buffer != null && bytes >= 0)
                {
                    var ums = new UnmanagedMemoryStream((byte*)buffer, bytes);
                    self.fh.CopyTo(ums, bytes);
                    return bytes;
                }
            }
            catch { }

            return -1;
        }

        /// <inheritdoc/>
        public override unsafe int write(mspack_file file, void* buffer, int bytes)
        {
            try
            {
                if (file is mspack_file_p self && buffer != null && bytes >= 0)
                {
                    var ums = new UnmanagedMemoryStream((byte*)buffer, bytes);
                    ums.CopyTo(self.fh, bytes);
                    return bytes;
                }
            }
            catch { }

            return -1;
        }

        /// <inheritdoc/>
        public override int seek(mspack_file file, long offset, MSPACK_SYS_SEEK mode)
        {
            try
            {
                if (file is mspack_file_p self)
                {
                    SeekOrigin origin;
                    switch (mode)
                    {
                        case MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START: origin = SeekOrigin.Begin; break;
                        case MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_CUR: origin = SeekOrigin.Current; break;
                        case MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_END: origin = SeekOrigin.End; break;
                        default: return -1;
                    }

                    self.fh.Seek(offset, origin);
                    return 0;
                }
            }
            catch { }

            return -1;
        }

        /// <inheritdoc/>
        public override long tell(mspack_file file)
        {
            var self = file as mspack_file_p;
            return self != null ? self.fh.Position : 0;
        }

        /// <inheritdoc/>
        public override void message(mspack_file file, in string format, params string[] args)
        {
            if (file != null) Console.Write((file as mspack_file_p)?.name);
            Console.Write(format, args);
            Console.Write("\n");
        }

        /// <inheritdoc/>
        public override unsafe void* alloc(int bytes)
        {
            var arr = new byte[bytes];
            return (byte*)arr[0];
        }

        /// <inheritdoc/>
        public override unsafe void free(void* ptr)
        {
            Marshal.FreeCoTaskMem((IntPtr)ptr);
        }

        /// <inheritdoc/>
        public override unsafe void copy(void* src, void* dest, int bytes)
        {
            byte[] temp = new byte[bytes];
            Marshal.Copy((IntPtr)src, temp, 0, bytes);
            Marshal.Copy(temp, 0, (IntPtr)dest, bytes);
        }
    }
}