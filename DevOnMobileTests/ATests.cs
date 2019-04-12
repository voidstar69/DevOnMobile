using System;
using System.Collections.Generic;
using System.Diagnostics;
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
     private static readonly byte[] randomBytes = GenRandomBytes(NumRandomBytes, ByteChangeProbability);

     // TODO: experiment with passing CompressionLevel to DeflateStream ctor
     [TestMethod, Timeout(1000)]
     public void LargeData_Deflate()
     {
         using (var outMemStream = new MemoryStream())
         using (var deflateStream = new DeflateStream(outMemStream, CompressionMode.Compress))
         {
             byte[] output = CheckStreamEncode(randomBytes, deflateStream, outMemStream);
             Console.WriteLine("Deflate: {0}% ({1} bytes)", (double) output.Length / randomBytes.Length * 100, output.Length);
         }
     }

     // TODO: experiment with passing CompressionLevel to GZipStream ctor
     [TestMethod, Timeout(1000)]
     public void LargeData_GZip()
     {
         using (var outMemStream = new MemoryStream())
         using (var zipStream = new GZipStream(outMemStream, CompressionMode.Compress))
         {
             byte[] output = CheckStreamEncode(randomBytes, zipStream, outMemStream);
             Console.WriteLine("GZip: {0}% ({1} bytes)", (double) output.Length / randomBytes.Length * 100, output.Length);
         }
     }

     // TODO: Why does my Huffman codec have much worse compression than Deflate and GZip? Huffman vs LZW? All symbols have equal probability? Need to use real-world data.
     [TestMethod, Timeout(60000)]
     public void LargeData_Huffman()
     {
         byte[] encodedBytes = CheckStreamCodecWithBinaryData(new HuffmanCodec(), randomBytes, null, false);
         Console.WriteLine("Huffman: {0}% ({1} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, encodedBytes.Length);
     }

     [TestMethod, Timeout(10000)]
     public void LargeData_LempelZiv78()
     {
         byte[] encodedBytes = CheckStreamCodecWithBinaryData(new LempelZiv78Codec(), randomBytes, null, false);
         Console.WriteLine("LZ78: {0}% ({1} bytes)", (double) encodedBytes.Length / randomBytes.Length * 100, encodedBytes.Length);
     }

     [TestMethod, Timeout(100)]
     public void TestHuffmanCodecWithOneSymbol()
     {
         byte[] input = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
         CheckStreamCodecWithBinaryData(new HuffmanCodec(), input);
     }

     [TestMethod, Timeout(200)]
     public void TestHuffmanCodecWithTwoSymbols()
     {
         byte[] input = {0, 5, 0, 5, 0, 0, 5, 5, 0, 0};
         CheckStreamCodecWithBinaryData(new HuffmanCodec(), input);
     }

     [TestMethod, Timeout(100)]
     public void TestLempelZiv78WithFewSymbols()
     {
         byte[] input = {1, 2, 1, 2, 3, 1, 2};
         CheckStreamCodecWithBinaryData(new LempelZiv78Codec(), input, new byte[]{0,0,1,0,0,2,0,1,2,0,0,3,0,3});
     }

     private static byte[] GenRandomBytes(int len, double byteChangeProb)
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

     private static string GenText(int len, double charChangeProb)
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
    
     private static byte[] CheckStreamEncode(byte[] input, Stream encodeStream, MemoryStream outStream)
     {
         using (var inMemStream = new MemoryStream(input))
         {
             inMemStream.CopyTo(encodeStream);
         }
         return outStream.GetBuffer();
     }

     private static string CheckCodec(ITextCodec codec, string input, string expectedEncoded)
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
             Console.WriteLine("Encode time: {0}ms, Decode time: {1}ms, Total time {2}ms", encodeMillis, decodeMillis, encodeMillis + decodeMillis);
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

    var encoded = CheckCodec(codec1, input, null);
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

    var encoded = CheckCodec(codec1, input, null);
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
                     ch = (char) ('a' + random.Next(26));

                 input += ch;
             }

             string encodedText = CheckCodec(codec1, input, null);
             totalDecodedSize += input.Length * 8;
             totalEncodedSizeChar += encodedText.Length;

             byte[] encodedBytes = CheckStreamCodecWithText(codec1, input, null);
             totalEncodedSizeBinary += encodedBytes.Length * 8;
         }

         Console.WriteLine("Encoded size vs original size, in bits:");
         Console.WriteLine("*** Char   Compression ratio: {0}%", (double) totalEncodedSizeChar / totalDecodedSize * 100);
         Console.WriteLine("*** Binary Compression ratio: {0}%", (double) totalEncodedSizeBinary / totalDecodedSize * 100);
     }

     [TestMethod, Timeout(500)]
     public void testLempelZiv78CodecWithRandomData()
     {
         var random = new Random();
         var codec = new LempelZiv78Codec();
         var totalDecodedSize = 0;
         var totalEncodedSize = 0;

         for (int i = 0; i < 15; i++)
         {
             var input = "";
             char ch = ' ';

             for (int j = 0; j < 25; j++)
             {
                 if (random.NextDouble() < 0.5)
                     ch = (char) ('a' + random.Next(26));

                 input += ch;
             }

             byte[] encodedBytes = CheckStreamCodecWithText(codec, input, null);
             totalDecodedSize += input.Length * 8;
             totalEncodedSize += encodedBytes.Length * 8;
         }

         Console.WriteLine("Encoded size vs original size, in bits:");
         Console.WriteLine("*** Compression ratio: {0}%", (double) totalEncodedSize / totalDecodedSize * 100);
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
  public void TestMultipleCodecs()
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

   CheckCodec(codec1, "Hello Wooorld", "Hel2o Wo3rld");
   CheckCodec(codec1, "hhheelooo   woorrrlllld!!", "h3e2lo3 3wo2r3l4d!2");
   CheckCodec(codec1, input3, output3);
   CheckCodec(codec1, input4, output4);
   CheckCodec(codec1, input5, output5);
   CheckCodec(codec1, input6, output6);

   CheckCodec(codec2, "0", "0");
   CheckCodec(codec2, "1", "1");
   CheckCodec(codec2, "00", "0000");
   CheckCodec(codec2, "11", "1100");
   CheckCodec(codec2, "", "");
   CheckCodec(codec2, "10001", "100011");

   CheckCodec(codec3, "Hello Wooorld", "0001101001101010011101001101100111110110001000001001000100100111101010100100110110100001011011001110101010001011111");
   CheckCodec(codec3, "hhheelooo   woorrrlllld!!", "001111101100010010011011110111010000010000100010110101001110001100001001101001101001101101001001001101110111100000001101101101010000101101101111111111111010011001100");
   CheckCodec(codec3, input6, output6);
   CheckStreamCodecWithText(codec3, "Hello Wooorld", new byte[] {88, 86, 46, 155, 111, 4, 137, 228, 85, 178, 133, 54, 87, 209, 7, 3});
   
   string input7 = GenText(1000, 1.0);
   Console.WriteLine("Compression ratio on {0} random letters (encoded size vs original size):", input7.Length);

   string encoded = codec1.encode(input7);
   Console.WriteLine("Char RLE: {0}%", (double)encoded.Length / input7.Length * 100);

   encoded = codec3.encode(input7);
   Console.WriteLine("Huffman to BitChars: {0}%", encoded.Length / 8.0 / input7.Length * 100);

      byte[] encodedBytes = CheckStreamCodecWithText(codec3, input7, null);
      Console.WriteLine("Huffman to Bits: {0}%", (double)encodedBytes.Length / input7.Length * 100);
  }

     [TestMethod]
     public void RLE_EncodeThenDecodeEmptyDataMustProduceOriginalData()
     {
         const string input = "";
         var codec = new CharacterRunLengthCodec();
         string encoded = codec.encode(input);
         string output = codec.decode(encoded);
         Assert.AreEqual(input, output);
     }

     [TestMethod]
     public void RLE_EncodeThenDecodeManyRepeatedCharsMustProduceOriginalData()
     {
         const string input = "qdttpmmmmmmmmmmhmm";
         var codec = new CharacterRunLengthCodec();
         string encoded = codec.encode(input);
         string output = codec.decode(encoded);
         Assert.AreEqual(input, output);
     }

     [TestMethod]
     public void RLE_EncodeThenDecodeMustProduceOriginalData()
     {
         //const string input = "Hello World";
         const string input = "hhheelooo   woorrrlllld!!";
         const string expectedEncoded = "h3e2lo3 3wo2r3l4d!2";

         var codec = new CharacterRunLengthCodec();
         string encoded = codec.encode(input);
         Assert.AreEqual(expectedEncoded, encoded, "Unexpected encoded data");
         string output = codec.decode(encoded);
         Assert.AreEqual(input, output);
     }

     [TestMethod]
     public void RLE_VerifyRunLengthEncodedData()
     {
         const string input = "Hello Wooorld";
         var codec = new CharacterRunLengthCodec();
         string encoded = codec.encode(input);
         Assert.AreEqual("Hel2o Wo3rld", encoded);
     }

     [TestMethod]
     public void RLE_CodecMustNotExpandData()
     {
         const string input = "Hello World";
         var codec = new CharacterRunLengthCodec();
         string encoded = codec.encode(input);
         Assert.IsTrue(encoded.Length <= input.Length);
     }
 }
}