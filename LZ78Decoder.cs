using System.IO;

namespace DevOnMobile
{
    internal class LZ78Decoder
    {
        internal struct Entry
        {
            public ushort PrefixIndex;
            public byte Suffix;
        }

        private const ushort Sentinel = 0;

        // map entry index to PrefixIndex mixed with Suffix
        private readonly Entry[] dict;
        private readonly ushort maxDictSize;
        private readonly byte numIndexBits;

        ushort nextAvailableIndex = 1;

        public LZ78Decoder(byte codecBitSize, ushort maxDictSize)
        {
            numIndexBits = codecBitSize;
            this.maxDictSize = maxDictSize;
            dict = new Entry[maxDictSize + 1];
            //dict = new Dictionary<int, Entry>(maxDictSize + 1);
        }

        public Entry[] GetInternalState()
        {
            return dict;
        }

        public ushort DictionarySizeAfterDecoding { get; private set; }
      
        /// <summary>
        /// Decodes a single compression entry into a decompressed bit stream.
        /// </summary>
        /// <param name="indexBits"></param>
        /// <param name="byteVal"></param>
        /// <param name="outputStream"></param>
        /// <returns>True iff byteVal is null, i.e. end of input stream reached. Some bits may still be output.</returns>
        public bool DecodeEntry(ushort indexBits, byte? byteVal, Stream outputStream)
        {
            // read N-bit last matching index
            var lastMatchingIndex = indexBits;

            // TODO: throw new EndOfStreamException();   if end of stream reached?

            // output run of bytes from dictionary
            OutputBytesInReverseUsingRecursion(dict, lastMatchingIndex, outputStream);
            //OutputBytesInReverseUsingStack(dict, lastMatchingIndex, outputStream);

            // read data byte
            if (byteVal == null)
            {
                // end of input stream
                DictionarySizeAfterDecoding = (ushort)(nextAvailableIndex - 1);
                //Console.WriteLine("LZ78Decoder.DecodeEntry: dictionary size = {0}", nextAvailableIndex - 1);
                return true;
            }
            var dataByte = (byte) byteVal;

            if (nextAvailableIndex < maxDictSize)
            {
                // store new entry into dictionary to grow run of bytes
                if(nextAvailableIndex == lastMatchingIndex)
                {
                    throw new InvalidDataException("Inserting a loop into the Dictionary!");
                }

                var entry = new Entry {PrefixIndex = lastMatchingIndex, Suffix = dataByte};
                dict[nextAvailableIndex] = entry;
                nextAvailableIndex++;
            }

            // output next byte value
            outputStream.WriteByte(dataByte);
            return false;
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