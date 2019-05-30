﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
    [TestClass]
    public class LZWCodec16BitTests
    {
        [TestMethod, Timeout(1000)]
        public void TestWithOneSymbol()
        {
            byte[] input = {1 , 1, 1, 1, 1, 1, 1, 1, 1, 1};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(16), input /*, new byte[]{2,0,1,1,1,1,2,1,1,2,0,8}*/);
        }

        [TestMethod, Timeout(2000)]
        public void TestWithTwoSymbols()
        {
            byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(16), input /*, new byte[] {1,0,5,1,1,0,1,1,5,1,0,0,0,0,8}*/);
        }

        [TestMethod, Timeout(1000)]
        public void TestWithFewSymbols()
        {
            byte[] input = {1, 2, 1, 2, 3, 1, 2};
            CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(16), input /*, new byte[]{2,0,2,1,1,3,1,1,8}*/);
        }

        [TestMethod, Timeout(60000)]
        public void TestWithLargeData()
        {
            byte[] randomBytes = CodecTestUtils.GenRandomBytes(128 * 1024, 0.2);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(16), randomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, randomBytes.Length, encodedBytes.Length);
        }

        [TestMethod, Timeout(60000)]
        public void TestFor64KBoundaryBug()
        {
            byte[] veryRandomBytes = CodecTestUtils.GenRandomBytes(256 * 1024, 1.0);
            byte[] encodedBytes = CodecTestUtils.CheckStreamCodecWithBinaryData(new LempelZivWelchCodec(16), veryRandomBytes, null, false);
            Console.WriteLine("LZ78-12bit: {0}% ({1}->{2} bytes)", (double) encodedBytes.Length / veryRandomBytes.Length * 100, veryRandomBytes.Length, encodedBytes.Length);
        }
   }
}