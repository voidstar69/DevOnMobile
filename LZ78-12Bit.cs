using System;
using System.Collections.Generic;
using System.IO;

namespace DevOnMobile
{
    public class LempelZiv78_12BitCodec : IStreamCodec
    {
        private const int NumIndexBits = 16;
        private const ushort MaxDictSize = (1 << NumIndexBits) - 1;
        private const ushort Sentinel = 0;

        private struct Entry
        {
            public ushort PrefixIndex;
            public byte Suffix;
        }

        public void encode(Stream inputStream, Stream outputStream)
        {
            // map PrefixIndex mixed with Suffix, to entry index
            var dict = new Dictionary<uint, ushort>(MaxDictSize);

            ushort lastMatchingIndex = Sentinel;
            ushort nextAvailableIndex = 1;
            using(var outBitStream = new OutputBitStream(outputStream))
            {
                int byteOrFlag;
                while (-1 != (byteOrFlag = inputStream.ReadByte()))
                {
                    var byteVal = (byte) byteOrFlag;

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
                        if (nextAvailableIndex < MaxDictSize)
                        {
                            dict[entry] = nextAvailableIndex;
                            nextAvailableIndex++;
                        }

                        // write N-bit last matching index
                        outBitStream.WriteBits(lastMatchingIndex, NumIndexBits);

                        // write data byte
                        outBitStream.WriteByte(byteVal);

                        lastMatchingIndex = Sentinel;
                    }
                }

                // write N-bit last matching index
                outBitStream.WriteBits(lastMatchingIndex, NumIndexBits);
            }

            Console.WriteLine("LempelZiv78_12BitCodec.encode: dictionary size = {0} ({1})", dict.Count, nextAvailableIndex - 1);
        }

        public void decode(Stream inputStream, Stream outputStream)
        {
            // map entry index to PrefixIndex mixed with Suffix
            var dict = new Entry[MaxDictSize + 1];

            ushort nextAvailableIndex = 1;
            var inBitStream = new InputBitStream(inputStream);
            while (true)
            {
                // read N-bit last matching index
                var lastMatchingIndex = (ushort)inBitStream.ReadBits(NumIndexBits);

                // TODO: throw new EndOfStreamException();   if end of stream reached?

                // output run of bytes from dictionary
                OutputBytesInReverseUsingRecursion(dict, lastMatchingIndex, outputStream);
                //OutputBytesInReverseUsingStack(dict, lastMatchingIndex, outputStream);

                // read data byte
                int byteValOrFlag = inputStream.ReadByte();
                if (byteValOrFlag == -1)
                {
                    Console.WriteLine("LempelZiv78_12BitCodec.decode: dictionary size = {0}", nextAvailableIndex - 1);
                    return;
                }
                var byteVal = (byte) byteValOrFlag;

                if (nextAvailableIndex < MaxDictSize)
                {
                    // store new entry into dictionary to grow run of bytes
                    if(nextAvailableIndex == lastMatchingIndex)
                    {
                        throw new InvalidDataException("Inserting a loop into the Dictionary!");
                    }

                    var entry = new Entry {PrefixIndex = lastMatchingIndex, Suffix = byteVal};
                    dict[nextAvailableIndex] = entry;
                    nextAvailableIndex++;
                }

                // output next byte value
                outputStream.WriteByte(byteVal);
            }
        }

        private static void OutputBytesInReverseUsingRecursion(Entry[] dict, ushort index, Stream outputStream)
        {
            // output run of bytes from dictionary
            if (index == Sentinel)
                return;

            if(index == dict[index].PrefixIndex)
            {
                throw new InvalidDataException("Dictionary contains a loop!");
            }

            OutputBytesInReverseUsingRecursion(dict, dict[index].PrefixIndex, outputStream);
            outputStream.WriteByte(dict[index].Suffix);
        }

        private static readonly Stack<byte> Stack = new Stack<byte>(100);

        private static void OutputBytesInReverseUsingStack(Entry[] dict, ushort lastMatchingIndex, Stream outputStream)
        {
            // output run of bytes from dictionary
            ushort index = lastMatchingIndex;
            while (index != Sentinel)
            {
                if(index == dict[index].PrefixIndex)
                {
                    throw new InvalidDataException("Dictionary contains a loop!");
                }

                Stack.Push(dict[index].Suffix);
                index = dict[index].PrefixIndex;
            }

            foreach (byte runByte in Stack)
            {
                outputStream.WriteByte(runByte);
            }

            Stack.Clear();
        }
    }
}