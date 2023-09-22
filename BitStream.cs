using System;
using System.IO;
using SabreTools.IO;

namespace SabreTools.Compression
{
    /// <summary>
    /// Wrapper to allow reading bits from a source stream
    /// </summary>
    public class BitStream
    {
        /// <summary>
        /// Original stream source
        /// </summary>
        private Stream _source;

        /// <summary>
        /// Last read byte value from the stream
        /// </summary>
        private byte? _lastRead;

        /// <summary>
        /// Index in the byte of the current bit
        /// </summary>
        private int _bitIndex;

        /// <summary>
        /// Create a new BitStream from a source Stream
        /// </summary>
        public BitStream(Stream source)
        {
            if (!source.CanRead || !source.CanSeek)
                throw new ArgumentException(nameof(source));

            _source = source;
            _lastRead = null;
            _bitIndex = 0;
        }

        /// <summary>
        /// Discard the current cached byte
        /// </summary>
        public void Discard()
        {
            _lastRead = null;
            _bitIndex = 0;
        }

        /// <summary>
        /// Read a single bit, if possible
        /// </summary>
        /// <returns>The next bit encoded in a byte, null on error or end of stream</returns>
        public byte? ReadBit()
        {
            // If we reached the end of the stream
            if (_source.Position >= _source.Length)
                return null;

            // If we don't have a value cached
            if (_lastRead == null)
            {
                // Read the next byte, if possible
                _lastRead = ReadSourceByte();
                if (_lastRead == null)
                    return null;

                // Reset the bit index
                _bitIndex = 0;
            }

            // Get the value by bit-shifting
            int value = (_lastRead.Value >> _bitIndex++) & 0x01;

            // Reset the byte if we're at the end
            if (_bitIndex >= 8)
                Discard();

            return (byte)value;
        }

        /// <summary>
        /// Read a full byte, if possible
        /// </summary>
        /// <returns>The next byte, null on error or end of stream</returns>
        public byte? ReadByte()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a full UInt16, if possible
        /// </summary>
        /// <returns>The next UInt16, null on error or end of stream</returns>
        public byte? ReadUInt16()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a full UInt32, if possible
        /// </summary>
        /// <returns>The next UInt32, null on error or end of stream</returns>
        public byte? ReadUInt32()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a full UInt64, if possible
        /// </summary>
        /// <returns>The next UInt64, null on error or end of stream</returns>
        public byte? ReadUInt64()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a single byte from the underlying stream, if possible
        /// </summary>
        /// <returns>The next full byte from the stream, null on error or end of stream</returns>
        private byte? ReadSourceByte()
        {
            try
            {
                return _source.ReadByteValue();
            }
            catch
            {
                return null;
            }
        }
    }
}