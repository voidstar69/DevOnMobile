﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
    [TestClass]
    public class Lz78Codec12BitTests
    {
        private const int NumRandomBytes = 1024 * 1024;
        private const double ByteChangeProbability = 0.2;
        private static readonly byte[] RandomBytes = CodecTestUtils.GenRandomBytes(NumRandomBytes, ByteChangeProbability);

        [TestMethod, Timeout(1000)]
        public void TestWithOneSymbol()
        {
            byte[] input = {1 , 1, 1, 1, 1, 1, 1, 1, 1, 1};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_12BitCodec(), input, new byte[]{0,0,1,1,0,1,2,0,1,3,0,1,0,0,8});
        }

        [TestMethod, Timeout(2000)]
        public void TestWithTwoSymbols()
        {
            byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_12BitCodec(), input, new byte[] {0,0,0,0,0,5,1,0,5,1,0,0,2,0,5,4,0,8});
        }

        [TestMethod, Timeout(1000)]
        public void TestWithFewSymbols()
        {
            byte[] input = {1, 2, 1, 2, 3, 1, 2};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_12BitCodec(), input, new byte[]{0,0,1,0,0,2,1,0,2,0,0,3,3,0,8});
        }

        [TestMethod, Timeout(1000)]
        public void TestWithLargeData()
        {
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_12BitCodec(), RandomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / RandomBytes.Length * 100, RandomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(1000)]
        public void TestFor64KBoundaryBug()
        {
            byte[] veryRandomBytes = CodecTestUtils.GenRandomBytes(256 * 1024, 1.0);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZiv78_12BitCodec(), veryRandomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / veryRandomBytes.Length * 100, veryRandomBytes.Length, encodedBytes.Length);
        }
   }
}