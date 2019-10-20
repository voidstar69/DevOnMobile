using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
    [TestClass]
    public class Lz78Codec12BitTests
    {
        [TestMethod, Timeout(1000)]
        public void TestWithOneSymbol()
        {
            byte[] input = {1 , 1, 1, 1, 1, 1, 1, 1, 1, 1};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(12), input, new byte[]{0,16,16,0,1,2,16,48,0,1,0,0});
        }

        [TestMethod, Timeout(2000)]
        public void TestWithTwoSymbols()
        {
            byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(12), input, new byte[] {0,0,0,0,5,1,80,16,0,0,2,80,64,0});
        }

        [TestMethod, Timeout(1000)]
        public void TestWithFewSymbols()
        {
            byte[] input = {1, 2, 1, 2, 3, 1, 2};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(12), input, new byte[]{0,16,0,0,2,1,32,0,0,3,3,0});
        }

        [TestMethod, Timeout(60000)]
        public void TestWithLargeData()
        {
            byte[] randomBytes = CodecTestUtils.GenRandomBytes(128 * 1024, 0.2);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(12), randomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, randomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(60000)]
        public void TestFor4KBoundaryBug()
        {
            byte[] veryRandomBytes = CodecTestUtils.GenRandomBytes(16 * 1024, 1.0);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_NBitCodec(12), veryRandomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / veryRandomBytes.Length * 100, veryRandomBytes.Length, encodedBytes.Length);
        }
   }
}