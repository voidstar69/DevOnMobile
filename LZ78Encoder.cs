using System;
using System.Collections.Generic;
using System.IO;

namespace DevOnMobile
{
    internal class LZ78Encoder
    {
        private const ushort Sentinel = 0;

        // map PrefixIndex mixed with Suffix, to entry index
        private readonly IDictionary<uint, ushort> dict;
        private readonly ushort maxDictSize;
        private readonly byte numIndexBits;

        private ushort lastMatchingIndex = Sentinel;
        private ushort nextAvailableIndex = 1;

        public LZ78Encoder(byte codecBitSize, ushort maxDictSize)
        {
            numIndexBits = codecBitSize;
            this.maxDictSize = maxDictSize;
            dict = new Dictionary<uint, ushort>(maxDictSize);
        }

        public IDictionary<uint, ushort> GetInternalState()
        {
            return dict;
        }

        // TODO: wrap this in a method that consumes input bytes until an entry is output/returned?
        public void EncodeByte(byte byteVal, IOutputBitStream outBitStream)
        {
            //var entry = new Entry {PrefixIndex = lastMatchingIndex, Suffix = byteVal};
            uint entry = ((uint)lastMatchingIndex << 8) + byteVal;
                    
            ushort dictIndex;
            if (dict.TryGetValue(entry, out dictIndex))
            {
                // grow run of matching bytes
                lastMatchingIndex = dictIndex;
            }
            else
            {
                // reached end of run of matching bytes, so output dictionary index of this run, and next byte value
                if (nextAvailableIndex < maxDictSize)
                {
                    dict[entry] = nextAvailableIndex;
                    nextAvailableIndex++;
                }

                // write N-bit last matching index
                outBitStream.WriteBits(lastMatchingIndex, numIndexBits);

                // write data byte
                outBitStream.WriteByte(byteVal);

                lastMatchingIndex = Sentinel;
            }
        }

        public void Flush(IOutputBitStream outBitStream)
        {
            // write N-bit last matching index
            outBitStream.WriteBits(lastMatchingIndex, numIndexBits);

            if (dict.Count != nextAvailableIndex - 1)
            {
                throw new InvalidDataException("Dictionary is corrupt!");
            }
            Console.WriteLine("LempelZiv78_NBitCodec.encode: dictionary size = {0} ({1})", dict.Count, nextAvailableIndex - 1);
        }
    }
}