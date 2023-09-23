using System;
using SabreTools.Models.Compression.Quantum;

namespace SabreTools.Compression.Quantum
{
    /// <see href="www.russotto.net/quantumcomp.html"/> 
    public class Decompressor
    {
        /// <summary>
        /// Internal bitstream to use for decompression
        /// </summary>
        private BitStream _bitStream;

        // TODO: Figure out what these values are
        private uint CS_H;
        private uint CS_L;
        private uint CS_C;

        /// <summary>
        /// Get the next symbol from a model
        /// </summary>
        public int GetSymbol(Model model)
        {
            int i;
            int sym;

            int freq = GetFrequency(model.Symbols[0].CumulativeFrequency);
            for (i = 1; i < model.Entries; i++)
            {
                if (model.Symbols[i].CumulativeFrequency <= freq)
                    break;
            }

            sym = model.Symbols[i - 1].Symbol;

            GetCode(model.Symbols[i - 1].CumulativeFrequency,
                    model.Symbols[i].CumulativeFrequency,
                    model.Symbols[0].CumulativeFrequency);

            UpdateModel(model, i);

            return sym;
        }

        /// <summary>
        /// Get the next code based on the frequencies
        /// </summary>
        private int GetCode(int prevFrequency, int currentFrequency, int totalFrequency)
        {
            int uf;

            uint range = (CS_H - CS_L) + 1;
            CS_H = CS_L + (uint)((prevFrequency * range) / totalFrequency) - 1;
            CS_L = CS_L + (uint)((currentFrequency * range) / totalFrequency);

            while (true)
            {
                if ((CS_L & 0x8000) != (CS_H & 0x8000))
                {
                    if ((CS_L & 0x4000) != 0 && (CS_H & 0x4000) == 0)
                    {
                        // Underflow case
                        CS_C ^= 0x4000;
                        CS_L &= 0x3FFF;
                        CS_H |= 0x4000;
                    }
                    else
                    {
                        break;
                    }
                }

                CS_L <<= 1;
                CS_H = (CS_H << 1) | 1;
                CS_C = (CS_C << 1) | _bitStream.ReadBit() ?? 0;
            }

            // TODO: Incomplete implementation?
            return 0;
        }

        /// <summary>
        /// Update the model after an encode or decode step
        /// </summary>
        private void UpdateModel(Model model, int lastUpdated)
        {
            // Update cumulative frequencies
            for (int i = 0; i < lastUpdated; i++)
            {
                var sym = model.Symbols[i];
                sym.CumulativeFrequency += 8;
                // model.TotalFrequency += 8; // TODO: Added in new model
            }

            // Decrement reordering time, if needed
            // if (model.TotalFrequency > 3800) // TODO: Added in new model
            //     model.TimeToReorder--;

            // If we haven't hit the reordering time
            if (model.TimeToReorder > 0)
            {
                // Update the cumulative frequencies
                for (int i = model.Entries - 1; i >= 0; i--)
                {
                    // Divide with truncation by 2
                    var sym = model.Symbols[i];
                    sym.CumulativeFrequency = (ushort)Math.Truncate((double)sym.CumulativeFrequency / 2);

                    // If we are lower the next frequency
                    if (i != 0 && sym.CumulativeFrequency <= model.Symbols[i - 1].CumulativeFrequency)
                    {
                        sym.CumulativeFrequency = (ushort)(model.Symbols[i - 1].CumulativeFrequency + 1);
                    }
                }
            }

            // If we hit the reordering time
            else
            {
                // Calculate frequencies
                ushort[] frequencies = new ushort[model.Entries];
                for (int i = 0; i < model.Entries; i++)
                {
                    var sym = model.Symbols[i];
                    frequencies[i] = GetFrequency(sym.CumulativeFrequency);
                    frequencies[i] = (ushort)Math.Round((double)frequencies[i] / 2);
                }

                // TODO: Finish implementation based on this statement from the docs:
                // The table is then sorted by frequency (highest first) using
                // an in-place selection sort (not stable!) and the cumulative
                // frequencies recomputed.
                // TODO: Determine if selection sort is needed

                model.TimeToReorder = 50;
            }
        }

        /// <summary>
        /// Get the frequency of a symbol based on its total frequency
        /// </summary>
        private ushort GetFrequency(ushort totalFrequency)
        {
            ulong range = ((CS_H - CS_L) & 0xFFFF) + 1;
            ulong frequency = ((CS_C - CS_L + 1) * totalFrequency - 1) / range;
            return (ushort)(frequency & 0xFFFF);
        }
    }
}