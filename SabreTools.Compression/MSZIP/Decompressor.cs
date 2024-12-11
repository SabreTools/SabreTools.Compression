using System;
using System.IO;
using SabreTools.IO.Extensions;

namespace SabreTools.Compression.MSZIP
{
    /// <see href="https://msopenspecs.azureedge.net/files/MS-MCI/%5bMS-MCI%5d.pdf"/>
    public class Decompressor
    {
        /// <summary>
        /// Source stream for the decompressor
        /// </summary>
        private readonly Stream _source;

        #region Constructors

        /// <summary>
        /// Create a MS-ZIP decompressor
        /// </summary>
        private Decompressor(Stream source)
        {
            // Validate the inputs
            if (source.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(source));
            if (!source.CanRead)
                throw new InvalidOperationException(nameof(source));

            _source = source;
        }

        /// <summary>
        /// Create a MS-ZIP decompressor
        /// </summary>
        public static Decompressor Create(byte[] source)
            => Create(new MemoryStream(source));

        /// <summary>
        /// Create a MS-ZIP decompressor
        /// </summary>
        public static Decompressor Create(Stream source)
        {
            // Create the decompressor
            var decompressor = new Decompressor(source);

            // Validate the header
            var header = new Models.Compression.MSZIP.BlockHeader();
            header.Signature = source.ReadUInt16();
            if (header.Signature != 0x4B43)
                throw new InvalidDataException(nameof(source));

            // Return
            return decompressor;
        }

        #endregion

        /// <summary>
        /// Decompress source data to an output stream
        /// </summary>
        public bool CopyTo(Stream dest)
        {
            // Ignore unwritable streams
            if (!dest.CanWrite)
                return false;

            byte[]? history = null;
            while (true)
            {
                byte[] buffer = new byte[32 * 1024];
                var blockStream = new Deflate.DeflateStream(_source, Deflate.CompressionMode.Decompress);
                if (history != null)
                    blockStream.SetDictionary(history);

                int read = blockStream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break;

                // Write to output
                dest.Write(buffer, 0, read);

                // Save the history for rollover
                history = new byte[read];
                Array.Copy(buffer, history, read);

                // Handle end of stream
                if (_source.Position >= _source.Length)
                    break;

                // Validate the header
                var header = new Models.Compression.MSZIP.BlockHeader();
                header.Signature = _source.ReadUInt16();
                if (header.Signature != 0x4B43)
                    break;
            }

            // Flush and return
            dest.Flush();
            return true;
        }
    }
}