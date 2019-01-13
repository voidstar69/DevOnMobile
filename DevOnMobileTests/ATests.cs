﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
 [TestClass]
 public class CodecTests
 {
     private const int NumRandomBytes = 100 * 1024;
     private const double ByteChangeProbability = 0.2;
     private static readonly byte[] randomBytes = genRandomBytes(NumRandomBytes, ByteChangeProbability);

     [TestMethod, Timeout(100)]
     public void TestDeflate()
     {
         using (var outMemStream = new MemoryStream())
         {
             // TODO: experiment with passing CompressionLevel to DeflateStream ctor
             using (var inMemStream = new MemoryStream(randomBytes))
             using (var zipStream = new DeflateStream(outMemStream, CompressionMode.Compress))
             {
                 // TODO
                 //checkStreamEncode(zipStream, outMemStream, randomBytes);

                 inMemStream.CopyTo(zipStream);
             }

             byte[] output = outMemStream.GetBuffer();
             Console.WriteLine("Deflate: {0}% ({1} bytes)", (double) output.Length / randomBytes.Length * 100, output.Length);
         }
     }

     [TestMethod, Timeout(100)]
     public void TestGZip()
     {
         using (var outMemStream = new MemoryStream())
         {
             // TODO: experiment with passing CompressionLevel to GZipStream ctor
             using (var inMemStream = new MemoryStream(randomBytes))
             using (var zipStream = new GZipStream(outMemStream, CompressionMode.Compress))
             {
                 inMemStream.CopyTo(zipStream);
             }

             byte[] output = outMemStream.GetBuffer();
             Console.WriteLine("GZip: {0}% ({1} bytes)", (double) output.Length / randomBytes.Length * 100, output.Length);
         }
     }
    
     // TODO: Why does my Huffman codec have much worse compression than Deflate and GZip? Huffman vs LZW?
     [TestMethod, Timeout(120000)]
     public void TestHuffman()
     {
         var codec = new HuffmanCodec();
         byte[] encodedBytes = CheckStreamCodecWithBinaryData(codec, randomBytes, null, false);
         Console.WriteLine("Huffman: {0}% ({1} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, encodedBytes.Length);
     }

     [TestMethod, Timeout(100)]
     public void TestHuffmanCodecWithTwoSymbols()
     {
         byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
         var codec = new HuffmanCodec();
         CheckStreamCodecWithBinaryData(codec, input);
     }

     [TestMethod, Timeout(100)]
     public void TestHuffmanCodecWithOneSymbol()
     {
         byte[] input = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1};

         var codec = new HuffmanCodec();
         CheckStreamCodecWithBinaryData(codec, input);
     }

     private static byte[] genRandomBytes(int len)
     {
         var data = new byte[len];
         var random = new Random();
         random.NextBytes(data);
         return data;

/*
   byte currByte = 0;

   for(int j=0; j<len; j++)
   {
    if(random.NextDouble() < byteChangeProb)
     currByte=random.NextByte();

    data[j] = currByte;
   }

   return data;
*/
     }

     private static byte[] genRandomBytes(int len, double byteChangeProb)
     {
         var data = new byte[len];
         var random = new Random();
         byte currByte = 0;

         for (int j = 0; j < len; j++)
         {
             if (random.NextDouble() < byteChangeProb)
                 currByte = (byte) random.Next(256);

             data[j] = currByte;
         }

         return data;
     }

     private static string genText(int len, double charChangeProb)
     {
         var text = string.Empty;
         var random = new Random();
         char ch = ' ';

         for (int j = 0; j < len; j++)
         {
             if (random.NextDouble() < charChangeProb)
                 ch = (char) ('a' + random.Next(26));

             text += ch;
         }

         return text;
     }

     private byte[] checkStreamEncode(Stream encodeStream, MemoryStream outStream, byte[] input)
  {
   using (var inMemStream = new MemoryStream(input))
   //using (encodeStream)
   {
    inMemStream.CopyTo(outStream);
   }

   byte[] output = outStream.GetBuffer();
   Console.WriteLine("Stream Encode: {0} bytes -> {1} bytes = {2}% compression",
    input.Length, output.Length, (double)output.Length / input.Length * 100);
   return output;
  }

  private string checkCodec(ITextCodec codec, string input, string expectedEncoded)
  {
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Console.WriteLine("{0} -> {1}", input, encoded);

   if (expectedEncoded != null)
    Assert.AreEqual(expectedEncoded, encoded); //, "Unexpected encoded data");

   Assert.AreEqual(input, output); //, "Encode then decode must produce original data");

   // TODO: this fails for the binary RLE and Huffman-as-Chars codecs because they store bits as characters
   //Assert.True(encoded.Length <= input.Length); //, "Codec must not expand data");

   return encoded;
  }

     private static byte[] CheckStreamCodecWithText(IStreamCodec codec, string inputText,
         IReadOnlyList<byte> expectedEncoded)
     {
         var encoding = new ASCIIEncoding();
         byte[] inputBytes = encoding.GetBytes(inputText);

         byte[] encodedBytes = CheckStreamCodecWithBinaryData(codec, inputBytes, expectedEncoded);
         Console.WriteLine("{0} -> ({1})", inputText, string.Join(",", encodedBytes));

         //string decodedText = encoding.GetString(decodedBytes);
         //Assert.AreEqual(inputText, decodedText, "Encode then decode must produce original data");

         return encodedBytes;
     }

     private static byte[] CheckStreamCodecWithBinaryData(IStreamCodec codec, byte[] inputBytes,
         IReadOnlyList<byte> expectedEncoded = null, bool printData = true)
     {
         byte[] encodedBytes;
         byte[] decodedBytes;
         using (var inputDataStream = new MemoryStream(inputBytes))
         using (var encodedDataStream = new MemoryStream())
         using (var decodedDataStream = new MemoryStream())
         {
             codec.encode(inputDataStream, encodedDataStream);
             encodedDataStream.Seek(0, SeekOrigin.Begin);
             codec.decode(encodedDataStream, decodedDataStream);
             encodedBytes = encodedDataStream.ToArray();
             decodedBytes = decodedDataStream.ToArray();
         }

         if (printData)
         {
             Console.WriteLine("{0} -> ({1})", string.Join(",", inputBytes), string.Join(",", encodedBytes));
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

     private static bool AreArraysEqual<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
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

     [TestMethod, Timeout(150)]
  public void testCharacterRunLengthCodecWithRandomData()
  {
   var random = new Random();
   var codec1 = new CharacterRunLengthCodec();
   var totalDecodedSize = 0;
   var totalEncodedSize = 0;

   for(int i=0; i<50; i++)
   {
    var input = "";
    char ch = ' ';

    for(int j=0; j<20; j++)
    {
     if(random.NextDouble()<0.5)
      ch=(char)('a'+random.Next(26));

     input += ch;
    }

    var encoded = checkCodec(codec1, input, null);
    totalDecodedSize += input.Length;
    totalEncodedSize += encoded.Length;
   }

   Console.WriteLine();
   Console.WriteLine("*** Compression ratio: {0}% (encoded size vs original size) ***", (double)totalEncodedSize / totalDecodedSize * 100);
  }

  [TestMethod, Timeout(100)]
  public void testBinaryRunLengthCodecWithRandomData()
  {
   var random = new Random();
   var codec1 = new BinaryRunLengthCodec();
   var totalDecodedSize = 0;
   var totalEncodedSize = 0;

   for (int i = 0; i < 50; i++)
   {
    var input = "";
    char ch = '0';

    for (int j = 0; j < 20; j++)
    {
     if (random.NextDouble() < 0.5)
      ch = (random.Next(2) == 0 ? '0' : '1');

     input += ch;
    }

    var encoded = checkCodec(codec1, input, null);
    totalDecodedSize += input.Length;
    totalEncodedSize += encoded.Length;
   }

   Console.WriteLine();
   Console.WriteLine("*** Compression ratio: {0}% (encoded size vs original size) ***", (double)totalEncodedSize / totalDecodedSize * 100);
  }

  [TestMethod, Timeout(500)]
  public void testHuffmanCodecWithRandomData()
  {
   var random = new Random();
   var codec1 = new HuffmanCodec();
      var totalDecodedSize = 0;
      var totalEncodedSizeChar = 0;
      var totalEncodedSizeBinary = 0;

   for (int i = 0; i < 15; i++)
   {
    var input = "";
    char ch = ' ';

    for (int j = 0; j < 25; j++)
    {
     if (random.NextDouble() < 0.5)
      ch = (char)('a' + random.Next(26));

     input += ch;
    }

    string encodedText = checkCodec(codec1, input, null);
    totalDecodedSize += input.Length * 8;
    totalEncodedSizeChar += encodedText.Length;

       byte[] encodedBytes = CheckStreamCodecWithText(codec1, input, null);
       totalEncodedSizeBinary += encodedBytes.Length * 8;
   }

      Console.WriteLine("Encoded size vs original size, in bits:");
      Console.WriteLine("*** Char   Compression ratio: {0}%", (double)totalEncodedSizeChar / totalDecodedSize * 100);
      Console.WriteLine("*** Binary Compression ratio: {0}%", (double)totalEncodedSizeBinary / totalDecodedSize * 100);
  }
    
     [TestMethod]
     public void testBitStreamWritingAndReading()
     {
         byte[] data;
         using (var memoryStream = new MemoryStream())
         {
             using (var bitStream = new OutputBitStream(memoryStream))
             {
                 bitStream.WriteBit(1);
                 bitStream.WriteBit(0);
                 bitStream.WriteBit(1);
                 bitStream.WriteBit(1);
             }
             data = memoryStream.ToArray();
         }

         using (var memoryStream = new MemoryStream(data))
         {
             var bitStream = new InputBitStream(memoryStream);
             Assert.AreEqual(1, bitStream.ReadBit());
             Assert.AreEqual(0, bitStream.ReadBit());
             Assert.AreEqual(1, bitStream.ReadBit());
             Assert.AreEqual(1, bitStream.ReadBit());
         }
     }

     [TestMethod]
     public void testBitStreamReadingToEndOfStream()
     {
         byte[] data;
         using (var memoryStream = new MemoryStream())
         {
             using (var bitStream = new OutputBitStream(memoryStream))
             {
                 bitStream.WriteBit(1);
             }
             data = memoryStream.ToArray();
         }

         using (var memoryStream = new MemoryStream(data))
         {
             var bitStream = new InputBitStream(memoryStream);
             Assert.AreEqual(1, bitStream.ReadBit());
             Assert.AreEqual(null, bitStream.ReadBit());
         }
     }

  [TestMethod]
  public void testBitStreamLengthAndReading()
  {
   var stream = new BitsAsTextStream("1test");
   //Assert.AreEqual(5, stream.Length);
   Assert.AreEqual(1, stream.ReadBit());
  }
  
  [TestMethod, ExpectedException(typeof(FormatException))]
  public void testBitStreamReadingNonNumericCharacter()
  {
   var stream = new BitsAsTextStream("a");
   Assert.AreEqual(1, stream.ReadBit());
  }
  
  [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
  public void testBitStreamReadingNonBinaryNumber()
  {
   var stream = new BitsAsTextStream("2");
   Assert.AreEqual(1, stream.ReadBit());
  }

  [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
  public void testBitStreamWritingNonBinaryNumber()
  {
   var stream = new BitsAsTextStream("");
   stream.WriteBit(2);
  }

  [TestMethod, Timeout(5000)]
  public void testMultipleCodecs()
  {
   var input3 = "aaaaaaaaabbbbbbbb";
   var output3 = "a9b8";
   var input4 = "aa";
   var output4 = "a2";
   var input5 = "aab";
   var output5 = "a2b";
   var input6 = "";
   var output6 = "";

   var codec1 = new CharacterRunLengthCodec();
   var codec2 = new BinaryRunLengthCodec();
   var codec3 = new HuffmanCodec();

   checkCodec(codec1, "Hello Wooorld", "Hel2o Wo3rld");
   checkCodec(codec1, "hhheelooo   woorrrlllld!!", "h3e2lo3 3wo2r3l4d!2");
   checkCodec(codec1, input3, output3);
   checkCodec(codec1, input4, output4);
   checkCodec(codec1, input5, output5);
   checkCodec(codec1, input6, output6);

   checkCodec(codec2, "0", "0");
   checkCodec(codec2, "1", "1");
   checkCodec(codec2, "00", "0000");
   checkCodec(codec2, "11", "1100");
   checkCodec(codec2, "", "");
   checkCodec(codec2, "10001", "100011");

   checkCodec(codec3, "Hello Wooorld", "0001101001101010011101001101100111110110001000001001000100100111101010100100110110100001011011001110101010001011111");
   checkCodec(codec3, "hhheelooo   woorrrlllld!!", "001111101100010010011011110111010000010000100010110101001110001100001001101001101001101101001001001101110111100000001101101101010000101101101111111111111010011001100");
   checkCodec(codec3, input6, output6);
   CheckStreamCodecWithText(codec3, "Hello Wooorld", new byte[] {88, 86, 46, 155, 111, 4, 137, 228, 85, 178, 133, 54, 87, 209, 7, 3});
   
   var input7 = genText(1000, 1.0);
   Console.WriteLine("Compression ratio on {0} random letters (encoded size vs original size):", input7.Length);

   var encoded = codec1.encode(input7);
   Console.WriteLine("Char RLE: {0}%", (double)encoded.Length / input7.Length * 100);

   encoded = codec3.encode(input7);
   Console.WriteLine("Huffman to BitChars: {0}%", (double)encoded.Length / 8.0 / input7.Length * 100);

      byte[] encodedBytes = CheckStreamCodecWithText(codec3, input7, null);
      Console.WriteLine("Huffman to Bits: {0}%", (double)encodedBytes.Length / input7.Length * 100);
  }

/*
  [Fact]
  public void encodeThenDecodeEmptyDataMustProduceOriginalData()
  {
   const string input = "";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }

  [Fact]
  public void encodeThenDecodeManyRepeatedCharsMustProduceOriginalData()
  {
   const string input = "qdttpmmmmmmmmmmhmm";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }

  [Fact]
  public void encodeThenDecodeMustProduceOriginalData()
  {
   //const string input = "Hello World";
   var input = "hhheelooo   woorrrlllld!!";
   var expectedEncoded = "h3e2lo3 3wo2r3l4d!2";

   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   Assert.AreEqual(expectedEncoded, encoded, "Unexpected encoded data");
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }

  [Fact]
  public void verifyRunLengthEncodedData()
  {
   const string input = "Hello Wooorld";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   Assert.AreEqual("Hel2o Wo3rld", encoded);
  }
*/

  [TestMethod]
  public void codecMustNotExpandData()
  {
   const string input = "Hello World";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.IsTrue(encoded.Length <= input.Length);
  }
 }
}
