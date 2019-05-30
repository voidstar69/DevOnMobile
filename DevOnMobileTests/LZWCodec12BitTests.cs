using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace DevOnMobile.Tests
{
    [TestClass]
    public class LZWCodec12BitTests
    {
        [TestMethod, Timeout(1000)]
        public void TestWithOneSymbol()
        {
            byte[] input = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(12), input, new byte[]{2,16,16,16,1,2,17,32,0,8});
        }

        [TestMethod, Timeout(2000)]
        public void TestWithTwoSymbols()
        {
            byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(12), input, new byte[] {1,80,16,16,0,1,81,16,0,0,0,0,4});
        }

        [TestMethod, Timeout(1000)]
        public void TestWithFewSymbols()
        {
            byte[] input = {1, 2, 1, 2, 3, 1, 2};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(12), input, new byte[]{2,32,16,16,3,1,1,4});
        }

        [TestMethod, Timeout(60000)]
        public void TestWithLargeData()
        {
            byte[] randomBytes = CodecTestUtils.GenRandomBytes(128 * 1024, 0.2);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(12), randomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, randomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(60000)]
        public void TestFor4KBoundaryBug()
        {
            byte[] veryRandomBytes = CodecTestUtils.GenRandomBytes(16 * 1024, 1.0);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(12), veryRandomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / veryRandomBytes.Length * 100, veryRandomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(1000)]
        public void TestWithOneSymbolMocked()
        {
            const bool printData = true;
            const bool printStats = true;
            byte[] inputBytes = {100};
            IStreamCodec codec = new LempelZivWelchCodec(12);

            byte[] decodedBytes;
            long encodeMillis;
            long decodeMillis;

            var encodedDataStreamMock = MockRepository.GenerateStrictMock<Stream>();
            encodedDataStreamMock.Expect(x => x.WriteByte(Arg<byte>.Is.Anything));
            encodedDataStreamMock.Expect(x => x.Seek(0, SeekOrigin.Begin)).Return(0).Repeat.Twice();
            encodedDataStreamMock.Expect(x => x.Seek(-1, SeekOrigin.End)).Return(-1);
            encodedDataStreamMock.Expect(x => x.Position).Return(0).Repeat.AtLeastOnce();
            encodedDataStreamMock.Expect(x => x.ReadByte()).Return(101);
            encodedDataStreamMock.Expect(x => x.ReadByte()).Return(0);
            encodedDataStreamMock.Expect(x => x.ReadByte()).Return(4);
            encodedDataStreamMock.Expect(x => x.ReadByte()).Return(-1);

            using (var inputDataStream = new MemoryStream(inputBytes))
            using (var decodedDataStream = new MemoryStream())
            {
                Stopwatch stopWatch = Stopwatch.StartNew();
                codec.encode(inputDataStream, encodedDataStreamMock);
                encodeMillis = stopWatch.ElapsedMilliseconds;
                stopWatch.Restart();
                encodedDataStreamMock.Seek(0, SeekOrigin.Begin);
                codec.decode(encodedDataStreamMock, decodedDataStream);
                decodeMillis = stopWatch.ElapsedMilliseconds;
                decodedBytes = decodedDataStream.ToArray();
            }

            if (printStats)
            {
                Console.WriteLine("Encode time: {0}ms, Decode time: {1}ms, Total time {2}ms", encodeMillis, decodeMillis, encodeMillis + decodeMillis);
            }
            if (printData)
            {
                Console.WriteLine("{0} -> {1}", string.Join(",", inputBytes), string.Join(",", decodedBytes));
            }

            Assert.IsTrue(CodecTestUtils.AreArraysEqual(new List<byte>(new byte[] {0}), decodedBytes), "Unexpected decoded data");
            //Assert.IsTrue(CodecTestUtils.AreArraysEqual(inputBytes, decodedBytes), "Encode then decode must produce original data");
        }
   }
}