using System;
using System.IO;
using System.Text;
using SabreTools.IO.Extensions;

namespace SabreTools.Compression.SZDD
{
    /// <see href="https://www.cabextract.org.uk/libmspack/doc/szdd_kwaj_format.html"/>
    public class Decompressor
    {
        /// <summary>
        /// Window to deflate data into
        /// </summary>
        private readonly byte[] _window = new byte[4096];

        /// <summary>
        /// Source stream for the decompressor
        /// </summary>
        private readonly BufferedStream _source;

        /// <summary>
        /// Offset within the window
        /// </summary>
        private int _offset;

        #region Constructors

        /// <summary>
        /// Create a SZDD decompressor
        /// </summary>
        private Decompressor(Stream source, int offset)
        {
            // Validate the inputs
            if (source.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(source));
            if (!source.CanRead)
                throw new InvalidOperationException(nameof(source));
            if (offset < 0 || offset > 4096)
                throw new ArgumentOutOfRangeException(nameof(offset));

            // Initialize the window with space characters
            _window = Array.ConvertAll(_window, b => (byte)0x20);
            _source = new BufferedStream(source);
            _offset = 4096 - offset;
        }

        /// <summary>
        /// Create a QBasic 4.5 installer SZDD decompressor
        /// </summary>
        public static Decompressor CreateQBasic(byte[] source)
            => CreateQBasic(new MemoryStream(source));

        /// <summary>
        /// Create a QBasic 4.5 installer SZDD decompressor
        /// </summary>
        /// TODO: Replace validation when Models is updated
        public static Decompressor CreateQBasic(Stream source)
        {
            // Create the decompressor
            var decompressor = new Decompressor(source, 18);

            // Validate the header
            byte[] magic = source.ReadBytes(8);
            if (Encoding.ASCII.GetString(magic) != Encoding.ASCII.GetString([0x53, 0x5A, 0x20, 0x88, 0xF0, 0x27, 0x33, 0xD1]))
                throw new InvalidDataException(nameof(source));

            // Skip the rest of the header
            _ = source.ReadUInt32(); // RealLength
            return decompressor;
        }

        /// <summary>
        /// Create a standard SZDD decompressor
        /// </summary>
        public static Decompressor CreateSZDD(byte[] source)
            => CreateSZDD(new MemoryStream(source));

        /// <summary>
        /// Create a standard SZDD decompressor
        /// </summary>
        /// TODO: Replace validation when Models is updated
        public static Decompressor CreateSZDD(Stream source)
        {
            // Create the decompressor
            var decompressor = new Decompressor(source, 16);

            // Validate the header
            byte[] magic = source.ReadBytes(8);
            if (Encoding.ASCII.GetString(magic) != Encoding.ASCII.GetString([0x53, 0x5A, 0x44, 0x44, 0x88, 0xF0, 0x27, 0x33]))
                throw new InvalidDataException(nameof(source));
            byte compressionType = source.ReadByteValue();
            if (compressionType != 0x41)
                throw new InvalidDataException(nameof(source));

            // Skip the rest of the header
            _ = source.ReadByteValue(); // LastChar
            _ = source.ReadUInt32(); // RealLength
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

            // Loop and decompress
            while (true)
            {
                // Get the control byte
                byte? control = _source.ReadNextByte();
                if (control == null)
                    break;

                for (int cbit = 0x01; (cbit & 0xFF) != 0; cbit <<= 1)
                {
                    // Literal value
                    if ((control & cbit) != 0)
                    {
                        // Read the literal byte
                        byte? literal = _source.ReadNextByte();
                        if (literal == null)
                            break;

                        // Store the data in the window and write
                        _window[_offset] = literal.Value;
                        dest.WriteByte(_window[_offset]);

                        // Set the next offset value
                        _offset++;
                        _offset &= 4095;
                        continue;
                    }

                    // Read the match position
                    int? matchpos = _source.ReadNextByte();
                    if (matchpos == null)
                        break;

                    // Read the match length
                    int? matchlen = _source.ReadNextByte();
                    if (matchlen == null)
                        break;

                    // Adjust the position and length
                    matchpos |= (matchlen & 0xF0) << 4;
                    matchlen = (matchlen & 0x0F) + 3;

                    // Loop over the match length
                    while (matchlen-- > 0)
                    {
                        // Copy the window value and write
                        _window[_offset] = _window[matchpos.Value];
                        dest.WriteByte(_window[_offset]);

                        // Set the next offset value
                        _offset++; matchpos++;
                        _offset &= 4095; matchpos &= 4095;
                    }
                }
            }

            // Flush and return
            dest.Flush();
            return true;
        }

        /// <summary>
        /// Buffered stream that reads in blocks
        /// </summary>
        private class BufferedStream
        {
            /// <summary>
            /// Source stream for populating the buffer
            /// </summary>
            private readonly Stream _source;

            /// <summary>
            /// Internal buffer to read
            /// </summary>
            private readonly byte[] _buffer = new byte[2048];

            /// <summary>
            /// Current pointer into the buffer
            /// </summary>
            private int _bufferPtr = 0;

            /// <summary>
            /// Represents the number of available bytes
            /// </summary>
            private int _available = -1;

            /// <summary>
            /// Create a new buffered stream
            /// </summary>
            public BufferedStream(Stream source)
            {
                _source = source;
            }

            /// <summary>
            /// Read the next byte from the buffer, if possible
            /// </summary>
            public byte? ReadNextByte()
            {
                // Ensure the buffer first
                if (!EnsureBuffer())
                    return null;

                // Return the next available value
                return _buffer[_bufferPtr++];
            }

            /// <summary>
            /// Ensure the buffer has data to read
            /// </summary>
            private bool EnsureBuffer()
            {
                // Force an update if in the initial state
                if (_available == -1)
                {
                    _available = _source.Read(_buffer, 0, _buffer.Length);
                    _bufferPtr = 0;
                    return _available != 0;
                }

                // If the pointer is out of range
                if (_bufferPtr >= _available)
                {
                    _available = _source.Read(_buffer, 0, _buffer.Length);
                    _bufferPtr = 0;
                    return _available != 0;
                }

                // Otherwise, assume data is available
                return true;
            }
        }
    }
}