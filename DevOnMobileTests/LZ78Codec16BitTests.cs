using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
    [TestClass]
    public class Lz78Codec16BitTests
    {
        [TestMethod, Timeout(1000)]
        public void TestWithOneSymbol()
        {
            byte[] input = {1 , 1, 1, 1, 1, 1, 1, 1, 1, 1};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(16), input, new byte[]{0,0,1,1,0,1,2,0,1,3,0,1,0,0});
        }

        [TestMethod, Timeout(2000)]
        public void TestWithTwoSymbols()
        {
            byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(16), input, new byte[] {0,0,0,0,0,5,1,0,5,1,0,0,2,0,5,4,0});
        }

        [TestMethod, Timeout(1000)]
        public void TestWithFewSymbols()
        {
            byte[] input = {1, 2, 1, 2, 3, 1, 2};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(16), input, new byte[]{0,0,1,0,0,2,1,0,2,0,0,3,3,0});
        }

        [TestMethod, Timeout(60000)]
        public void TestWithLargeData()
        {
            byte[] randomBytes = CodecTestUtils.GenRandomBytes(128 * 1024, 0.2);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(16), randomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, randomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(60000)]
        public void TestFor64KBoundaryBug()
        {
            byte[] veryRandomBytes = CodecTestUtils.GenRandomBytes(256 * 1024, 1.0);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(16), veryRandomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / veryRandomBytes.Length * 100, veryRandomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(1000)]
        public void TestWithFewSymbols_VerifyIncrementally()
        {
            byte[] input = {1, 2, 1, 2, 3, 1, 2};
            IReadOnlyList<byte> expectedEncoded = new byte[]{0,0,1,0,0,2,1,0,2,0,0,3,3,0};
            byte[] encodedBytes;

            ushort maxDictSize;
            byte numIndexBits = 16;
            if (numIndexBits == 16)
            {
                maxDictSize = 65535;
            }
            else
            {
                maxDictSize = (ushort) (1 << numIndexBits);
            }

            using (var inputDataStream = new MemoryStream(input))
            using (var encodedStream = new MemoryStream())
            using (var decodedStream = new MemoryStream())
            {
                using (var encoderOutBitStream = new OutputBitStream(encodedStream, false))
                {
                    var encoder = new LZ78Encoder(numIndexBits, maxDictSize);
                    var decoder = new LZ78Decoder(numIndexBits, maxDictSize);
                    var decoderInBitStream = new InputBitStream(encodedStream, false);
                    Queue<byte> inputBytesToCompare = new Queue<byte>();
                    int byteOrFlag;
                    while (-1 != (byteOrFlag = inputDataStream.ReadByte()))
                    {
                        var byteVal = (byte) byteOrFlag;
                        inputBytesToCompare.Enqueue(byteVal);

                        Console.Write("Input byte: {0} ", byteVal);

                        long encodedStreamPosBeforeEncodingByte = encodedStream.Position;
                        if (encoder.EncodeByte(byteVal, encoderOutBitStream))
                        {
                            // Some encoded bits were written to the encoded bit stream.
                            // Jump back before these bits to allow the decoder to decode these bits.
                            encodedStream.Position = encodedStreamPosBeforeEncodingByte;

                            if (DecodeEntryAndVerify(decoderInBitStream, numIndexBits, decodedStream, decoder,
                                inputBytesToCompare))
                            {
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No Decoded bytes yet");
                        }
                    }

                    long encodedStreamPosBeforeFlush = encodedStream.Position;
                    encoder.Flush(encoderOutBitStream);

                    // Some encoded bits were written to the encoded bit stream.
                    // Jump back before these bits to allow the decoder to decode these bits.
                    encodedStream.Position = encodedStreamPosBeforeFlush;

                    bool endOfStreamReached = DecodeEntryAndVerify(decoderInBitStream, numIndexBits, decodedStream,
                        decoder, inputBytesToCompare);
                    Assert.IsTrue(endOfStreamReached);
                }
                encodedBytes = encodedStream.ToArray();
            }

            if (expectedEncoded != null)
            {
                Assert.IsTrue(CodecTestUtils.AreArraysEqual(expectedEncoded, encodedBytes), "Unexpected encoded data");
            }
        }

        /// <summary>
        /// Decodes the next compression entry, then verifies the N bytes against the first N bytes in the queue.
        /// </summary>
        /// <param name="decoderInBitStream"></param>
        /// <param name="numIndexBits"></param>
        /// <param name="decodedStream"></param>
        /// <param name="decoder"></param>
        /// <param name="inputBytesToCompare"></param>
        /// <returns>True iff end of input stream reached</returns>
        private static bool DecodeEntryAndVerify(InputBitStream decoderInBitStream, byte numIndexBits, MemoryStream decodedStream,
            LZ78Decoder decoder, Queue<byte> inputBytesToCompare)
        {
            // read N-bit index
            var indexBits = (ushort) decoderInBitStream.ReadBits(numIndexBits);

            // read data byte
            byte? byteValOrFlag = decoderInBitStream.ReadByte();

            long decodedStreamPosBeforeDecodingByte = decodedStream.Position;
            bool endOfStreamReached = decoder.DecodeEntry(indexBits, byteValOrFlag, decodedStream);
            long decodedStreamPosAfterDecodingByte = decodedStream.Position;

            decodedStream.Position = decodedStreamPosBeforeDecodingByte;

            Console.Write("Decoded bytes: ");
            while (decodedStream.Position < decodedStreamPosAfterDecodingByte)
            {
                byte decodedByteVal = (byte) decodedStream.ReadByte();
                byte inputByte = inputBytesToCompare.Dequeue();

                Console.Write(decodedByteVal);
                Console.Write(' ');
                Assert.AreEqual(inputByte, decodedByteVal);
            }

            Console.WriteLine();

            // TODO: compare internal state of encoder and decoder, i.e. dictionary vs array of Entry, to look for diffs/errors
            return endOfStreamReached;
        }
    }
}