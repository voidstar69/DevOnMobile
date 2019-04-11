using System.Collections.Generic;
using System.IO;

namespace DevOnMobile
{
public class LempelZiv78_12BitCodec : IStreamCodec
    {
        private const int NumIndexBits = 12;
        private const int MaxDictSize = 1 << NumIndexBits;

        private struct Entry
        {
            public ushort PrefixIndex;
            public byte Suffix;
        }

        public void encode(Stream inputStream, Stream outputStream)
        {
            // map PrefixIndex mixed with Suffix, to entry index
            var dict = new Dictionary<uint, ushort>(MaxDictSize);

            ushort lastMatchingIndex = 0;
            ushort nextAvailableIndex = 1;
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
                    if (nextAvailableIndex <= MaxDictSize)
                    {
                        dict[entry] = nextAvailableIndex;
                        nextAvailableIndex++;
                    }

                    // write two-byte last matching index
                    var highByte = (byte) (lastMatchingIndex >> 8);
                    var lowByte = (byte) (lastMatchingIndex & 0xff);
                    outputStream.WriteByte(highByte);
                    outputStream.WriteByte(lowByte);

                    // write data byte
                    outputStream.WriteByte(byteVal);

                    lastMatchingIndex = 0;
                }
            }

            // write two-byte last matching index
            var hiByte = (byte) (lastMatchingIndex >> 8);
            var loByte = (byte) (lastMatchingIndex & 0xff);
            outputStream.WriteByte(hiByte);
            outputStream.WriteByte(loByte);
        }

        public void decode(Stream inputStream, Stream outputStream)
        {
            // map entry index to PrefixIndex mixed with Suffix
            var dict = new Entry[MaxDictSize + 1];

            ushort nextAvailableIndex = 1;
            while (true)
            {
                // read two-byte last matching index
                int highByte = inputStream.ReadByte();
                if (highByte == -1)
                {
                    throw new EndOfStreamException();
                }

                int lowByte = inputStream.ReadByte();
                if (lowByte == -1)
                {
                    throw new EndOfStreamException();
                }
                var lastMatchingIndex = (ushort)((highByte << 8) + lowByte);

                // output run of bytes from dictionary
                OutputBytesInReverseUsingRecursion(dict, lastMatchingIndex, outputStream);
                //OutputBytesInReverseUsingStack(dict, lastMatchingIndex, outputStream);

                // read data byte
                int byteValOrFlag = inputStream.ReadByte();
                if (byteValOrFlag == -1)
                {
                    return;
                }
                var byteVal = (byte) byteValOrFlag;

                if (nextAvailableIndex <= MaxDictSize)
                {
                    // store new entry into dictionary to grow run of bytes
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
            if (index == 0)
                return;

            OutputBytesInReverseUsingRecursion(dict, dict[index].PrefixIndex, outputStream);
            outputStream.WriteByte(dict[index].Suffix);
        }

/*
        private static Stack<byte> stack = new Stack<byte>(100);

        private static void OutputBytesInReverseUsingStack(Entry[] dict, ushort lastMatchingIndex, Stream outputStream)
        {
            // output run of bytes from dictionary
            ushort index = lastMatchingIndex;
            while (index != 0)
            {
                stack.Push(dict[index].Suffix);
                index = dict[index].PrefixIndex;
            }

            foreach (byte runByte in stack)
            {
                outputStream.WriteByte(runByte);
            }
}

            stack.Clear();
        }
*/
    }