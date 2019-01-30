using System.Collections.Generic;
using System.IO;

namespace DevOnMobile
{
    public class LempelZiv78Codec : IStreamCodec
    {
        private struct Entry
        {
            public ushort PrefixIndex;
            public byte Suffix;
        }

        public void encode(Stream inputStream, Stream outputStream)
        {
            // TODO: limit size of dictionary? Original LZ78 algorithm uses a fixed size array as a dictionary
            //const int maxDictSize = 10;
            var dict = new Dictionary<Entry, ushort>(4096);

            ushort lastMatchingIndex = 0;
            ushort nextAvailableIndex = 1;
            int byteOrFlag;
            while (-1 != (byteOrFlag = inputStream.ReadByte()))
            {
                var byteVal = (byte) byteOrFlag;

                var entry = new Entry {PrefixIndex = lastMatchingIndex, Suffix = byteVal};

                if (dict.ContainsKey(entry))
                {
                    // grow run of matching bytes
                    lastMatchingIndex = dict[entry];
                }
                else
                {
                    // reached end of run of matching bytes, so output dictionary index of this run, and next byte value
                    dict[entry] = nextAvailableIndex;
                    nextAvailableIndex++;

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
            // TODO: could be a fixed-size array, Entry[]
            var dict = new Dictionary<ushort, Entry>(4096);

            ushort lastMatchingIndex = 0;
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
                lastMatchingIndex = (ushort)((highByte << 8) + lowByte);

                // output run of bytes from dictionary
                var stack = new Stack<byte>(); // TODO: reverse bytes more efficiently?
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

                // read data byte
                int byteValOrFlag = inputStream.ReadByte();
                if (byteValOrFlag == -1)
                {
                    return;
                }
                var byteVal = (byte) byteValOrFlag;

                // store new entry into dictionary to grow run of bytes
                var entry = new Entry {PrefixIndex = lastMatchingIndex, Suffix = byteVal};
                dict[nextAvailableIndex] = entry;
                nextAvailableIndex++;

                // output next byte value
                outputStream.WriteByte(byteVal);
            }
        }
    }
}