using System;
using System.IO;
using static SabreTools.Compression.Blast.Constants;

namespace SabreTools.Compression.Blast
{
    /// <summary>
    /// Input and output state
    /// </summary>
    public class State
    {
        #region Input State

        /// <summary>
        /// Opaque information passed to InputFunction()
        /// </summary>
        public Stream Source { get; set; }
        
        /// <summary>
        /// Next input location
        /// </summary>
        public byte[] Input { get; set; }

        /// <summary>
        /// Pointer to the next input location
        /// </summary>
        public int InputPtr { get; set; }

        /// <summary>
        /// Available input at in
        /// </summary>
        public uint Left { get; set; }

        /// <summary>
        /// Bit buffer
        /// </summary>
        public int BitBuf { get; set; }

        /// <summary>
        /// Number of bits in bit buffer
        /// </summary>
        public int BitCnt { get; set; }

        #endregion

        #region Output State

        /// <summary>
        /// Opaque information passed to OutputFunction()
        /// </summary>
        public Stream Dest { get; set; }

        /// <summary>
        /// Index of next write location in out[]
        /// </summary>
        public uint Next { get; set; }

        /// <summary>
        /// True to check distances (for first 4K)
        /// </summary>
        public bool First { get; set; }

        /// <summary>
        /// Output buffer and sliding window
        /// </summary>
        public readonly byte[] Output = new byte[MAXWIN];

        /// <summary>
        /// Pointer to the next output location
        /// </summary>
        public int OutputPtr { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public State(Stream source, Stream dest)
        {
            Source = source;
            Input = [];
            InputPtr = 0;
            Left = 0;
            BitBuf = 0;
            BitCnt = 0;

            Dest = dest;
            Next = 0;
            First = true;
        }

        /// <summary>
        /// Return need bits from the input stream.  This always leaves less than
        /// eight bits in the buffer.  bits() works properly for need == 0.
        /// </summary>
        /// <param name="need">Number of bits to read</param>
        /// <remarks>
        /// Bits are stored in bytes from the least significant bit to the most
        /// significant bit.  Therefore bits are dropped from the bottom of the bit
        /// buffer, using shift right, and new bytes are appended to the top of the
        /// bit buffer, using shift left.
        /// </remarks>
        public int Bits(int need)
        {
            // Load at least need bits into val
            int val = BitBuf;
            while (BitCnt < need)
            {
                if (Left == 0)
                {
                    Left = ProcessInput();
                    if (Left == 0)
                        throw new IndexOutOfRangeException();
                }

                // Load eight bits
                val |= Input[InputPtr++] << BitCnt;
                Left--;
                BitCnt += 8;
            }

            // Drop need bits and update buffer, always zero to seven bits left
            BitBuf = val >> need;
            BitCnt -= need;

            // Return need bits, zeroing the bits above that
            return val & ((1 << need) - 1);
        }

        /// <summary>
        /// Process input for the current state
        /// </summary>
        /// <returns>Amount of data in Input</returns>
        public uint ProcessInput()
        {
            int read = Source.Read(Input, 0, 4096);
            return (uint)read;
        }

        /// <summary>
        /// Process output for the current state
        /// </summary>
        /// <returns>True if the output could be added, false otherwise</returns>
        public bool ProcessOutput()
        {
            try
            {
                byte[] next = new byte[Next];
                Array.Copy(Output, next, next.Length);
                Dest.Write(next);
                Dest.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}