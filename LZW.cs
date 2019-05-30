using System;
using System.Collections.Generic;
using System.IO;

namespace DevOnMobile
{
    public class LempelZivWelchCodec : IStreamCodec
    {
        private const ushort Sentinel = 0;
        private readonly byte numIndexBits;
        private readonly ushort maxDictSize;

        private struct Entry
        {
            public ushort PrefixIndex;
            public byte Suffix;
        }

        public LempelZivWelchCodec(int codecBitSize)
        {
            if (codecBitSize < 1 || codecBitSize > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(codecBitSize), codecBitSize, "Only 1-16 bits supported");
            }

            numIndexBits = (byte)codecBitSize;
            if (codecBitSize == 16)
            {
                maxDictSize = 65535;
            }
            else
            {
                maxDictSize = (ushort) (1 << codecBitSize);
            }
        }

        public void encode(Stream inputStream, Stream outputStream)
        {
            // map PrefixIndex mixed with Suffix, to entry index
            var dict = new Dictionary<uint, ushort>(maxDictSize);

            ushort nextAvailableIndex = 1;
            for (ushort byteVal = 0; byteVal <= 255; byteVal++, nextAvailableIndex++)
            {
                uint entry = ((uint)Sentinel << 8) + (byte)byteVal;
                dict[entry] = nextAvailableIndex;
            }

            ushort lastMatchingIndex = Sentinel;
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
                        //lastMatchingIndex = (ushort) (byteVal + 1);
                    }
                }

                // write N-bit last matching index
                outBitStream.WriteBits(lastMatchingIndex, numIndexBits);
            }

            if (dict.Count != nextAvailableIndex - 1)
            {
                throw new InvalidDataException("Dictionary is corrupt!");
            }
            Console.WriteLine("LempelZivWelchCodec.encode: dictionary size = {0} ({1})", dict.Count, nextAvailableIndex - 1);
        }

        public void decode(Stream inputStream, Stream outputStream)
        {
            // map entry index to PrefixIndex mixed with Suffix
            var dict = new Entry[maxDictSize + 1];
            //var dict = new Dictionary<int, Entry>(maxDictSize + 1);

            ushort nextAvailableIndex = 1;
            for (ushort byteVal = 0; byteVal <= 255; byteVal++, nextAvailableIndex++)
            {
                var entry = new Entry {PrefixIndex = Sentinel, Suffix = (byte)byteVal};
                dict[nextAvailableIndex] = entry;
            }

            var inBitStream = new InputBitStream(inputStream);
            //var nextIndex = (ushort)inBitStream.ReadBits(numIndexBits).Value;
            while (true)
            {
                // read N-bit last matching index
                uint? indexBits = inBitStream.ReadBits(numIndexBits);
                if (indexBits == null)
                {
                    // end of input stream
                    Console.WriteLine("LempelZivWelchCodec.decode: dictionary size = {0}", nextAvailableIndex - 1);
                    return;
                }
                var lastMatchingIndex = (ushort)indexBits;
                //ushort lastMatchingIndex = nextIndex;

                // output run of bytes from dictionary
                OutputBytesInReverseUsingRecursion(dict, lastMatchingIndex, outputStream);
                //OutputBytesInReverseUsingStack(dict, lastMatchingIndex, outputStream);

                // read data byte
                byte? byteValOrFlag = inBitStream.ReadByte();
                if (byteValOrFlag == null)
                {
                    // end of input stream
                    Console.WriteLine("LempelZiv78_NBitCodec.decode: dictionary size = {0}", nextAvailableIndex - 1);
                    return;
                }
                var byteVal = (byte)byteValOrFlag;

                // read next N-bit last matching index
                //indexBits = inBitStream.ReadBits(numIndexBits);
                //if (indexBits == null)
                //{
                //    // end of input stream
                //    Console.WriteLine("LempelZivWelchCodec.decode: dictionary size = {0}", nextAvailableIndex - 1);
                //    return;
                //}
                //nextIndex = (ushort) indexBits;
                //byteVal = dict[nextIndex].Suffix;

                if (nextAvailableIndex < maxDictSize)
                {
                    if(nextAvailableIndex == lastMatchingIndex)
                    {
                        throw new InvalidDataException("Inserting a loop into the Dictionary!");
                    }

                    // store new entry into dictionary to grow run of bytes
                    var entry = new Entry {PrefixIndex = lastMatchingIndex, Suffix = byteVal};
                    dict[nextAvailableIndex] = entry;
                    nextAvailableIndex++;
                }

                // output next byte value
                outputStream.WriteByte(byteVal);
            }
        }

        private static void OutputBytesInReverseUsingRecursion(/*IList<Entry>*/ /*IDictionary<int, Entry>*/ Entry[] dict, ushort index, Stream outputStream)
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

/*
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
*/
    }
}