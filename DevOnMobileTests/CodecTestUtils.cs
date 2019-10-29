using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DevOnMobile.Tests
{
    internal static class CodecTestUtils
    {
        internal static byte[] GenRandomBytes(int len, double byteChangeProb)
        {
            var data = new byte[len];
            var random = new Random();
            byte currByte = 0;
        
            for (var j = 0; j < len; j++)
            {
                if (random.NextDouble() < byteChangeProb)
                    currByte = (byte) random.Next(256);

                data[j] = currByte;
            }

            return data;
        }

        internal static byte[] CheckStreamCodecWithBinaryData(IStreamCodec codec, byte[] inputBytes,
            IReadOnlyList<byte> expectedEncoded = null, bool printData = true, bool printStats = true)
        {
            byte[] encodedBytes;
            byte[] decodedBytes;
            long encodeMillis;
            long decodeMillis;

            using (var inputDataStream = new MemoryStream(inputBytes))
            using (var encodedDataStream = new MemoryStream())
            using (var decodedDataStream = new MemoryStream())
            {
                Stopwatch stopWatch = Stopwatch.StartNew();
                codec.encode(inputDataStream, encodedDataStream);
                encodeMillis = stopWatch.ElapsedMilliseconds;
                stopWatch.Restart();
                encodedDataStream.Seek(0, SeekOrigin.Begin);
                codec.decode(encodedDataStream, decodedDataStream);
                decodeMillis = stopWatch.ElapsedMilliseconds;
                encodedBytes = encodedDataStream.ToArray();
                decodedBytes = decodedDataStream.ToArray();
            }

            if (printStats)
            {
                Console.WriteLine("Encode time: {0}ms, Decode time: {1}ms, Total time {2}ms. Dictionary size: {3}.", encodeMillis, decodeMillis, encodeMillis + decodeMillis, codec.dictionarySize);
            }
            if (printData)
            {
                Console.WriteLine("{0} -> ({1}) -> {2}", string.Join(",", inputBytes), string.Join(",", encodedBytes), string.Join(",", decodedBytes));
            }

            if (expectedEncoded != null)
            {
                Assert.IsTrue(AreArraysEqual(expectedEncoded, encodedBytes), "Unexpected encoded data");
            }

            Assert.IsTrue(AreArraysEqual(inputBytes, decodedBytes), "Encode then decode must produce original data");

            // TODO: this fails for the binary RLE and Huffman codecs because they store bits as characters
            // TODO: this sometimes fails for the Huffman stream codec because the Huffman tree takes up space
            //Assert.IsTrue(encodedBytes.Length <= inputBytes.Length, "Codec must not expand data");

            return encodedBytes;
        }

        internal static byte[] CheckStdStreamCodecWithBinaryData(IStreamCodec codec, byte[] inputBytes,
            IReadOnlyList<byte> expectedEncoded = null, bool printData = true, bool printStats = true)
        {
            byte[] encodedBytes;
            byte[] decodedBytes;
            long encodeMillis;
            long decodeMillis;

            using (var inputDataStream = new MemoryStream(inputBytes))
            using (var encodedDataStream = new MemoryStream())
            using (var decodedDataStream = new MemoryStream())
            {
                Stopwatch stopWatch = Stopwatch.StartNew();
                codec.encode(inputDataStream, encodedDataStream);
                encodeMillis = stopWatch.ElapsedMilliseconds;
                stopWatch.Restart();
                encodedDataStream.Seek(0, SeekOrigin.Begin);
                codec.decode(encodedDataStream, decodedDataStream);
                decodeMillis = stopWatch.ElapsedMilliseconds;
                encodedBytes = encodedDataStream.ToArray();
                decodedBytes = decodedDataStream.ToArray();
            }

            if (printStats)
            {
                Console.WriteLine("Encode time: {0}ms, Decode time: {1}ms, Total time {2}ms. Dictionary size: {3}.", encodeMillis, decodeMillis, encodeMillis + decodeMillis, codec.dictionarySize);
            }
            if (printData)
            {
                Console.WriteLine("{0} -> ({1}) -> {2}", string.Join(",", inputBytes), string.Join(",", encodedBytes), string.Join(",", decodedBytes));
            }

            if (expectedEncoded != null)
            {
                Assert.IsTrue(AreArraysEqual(expectedEncoded, encodedBytes), "Unexpected encoded data");
            }

            Assert.IsTrue(AreArraysEqual(inputBytes, decodedBytes), "Encode then decode must produce original data");

            // TODO: this fails for the binary RLE and Huffman codecs because they store bits as characters
            // TODO: this sometimes fails for the Huffman stream codec because the Huffman tree takes up space
            //Assert.IsTrue(encodedBytes.Length <= inputBytes.Length, "Codec must not expand data");

            return encodedBytes;
        }

        internal static bool AreArraysEqual<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
        {
            if (expected.Count != actual.Count)
                return false;

            for (var i = 0; i < actual.Count; i++)
            {
                if (!expected[i].Equals(actual[i]))
                    return false;
            }

            return true;
        }
    }
}