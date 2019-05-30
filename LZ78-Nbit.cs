using System;
using System.IO;

namespace DevOnMobile
{
    public class LempelZiv78_NBitCodec : IStreamCodec
    {
        private const ushort Sentinel = 0;
        private readonly byte numIndexBits;
        private readonly ushort maxDictSize;

        private struct Entry
        {
            public ushort PrefixIndex;
            public byte Suffix;
        }

        public LempelZiv78_NBitCodec(int codecBitSize)
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
            var encoder = new LZ78Encoder(numIndexBits, maxDictSize);
            using(var outBitStream = new OutputBitStream(outputStream))
            {
                int byteOrFlag;
                while (-1 != (byteOrFlag = inputStream.ReadByte()))
                {
                    var byteVal = (byte) byteOrFlag;
                    encoder.EncodeByte(byteVal, outBitStream);
                }
                encoder.Flush(outBitStream);
            }
        }

        public void decode(Stream inputStream, Stream outputStream)
        {
            var decoder = new LZ78Decoder(numIndexBits, maxDictSize);
            var inBitStream = new InputBitStream(inputStream);
            while (true)
            {
                // read N-bit index
                var indexBits = (ushort)inBitStream.ReadBits(numIndexBits);

                // read data byte
                byte? byteValOrFlag = inBitStream.ReadByte();

                if (!decoder.DecodeEntry(indexBits, byteValOrFlag, outputStream))
                {
                    break;
                }
            }
        }
    }
}