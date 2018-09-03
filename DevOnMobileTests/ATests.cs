using System;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace DevOnMobile.Tests
{
 public class CodecTests
 {
  [Fact] // Timeout(100)
  public void testGZip()
  {
   var input = genRandomBytes(1000);

   using (var outMemStream = new MemoryStream())
   {
    using (var inMemStream = new MemoryStream(input))
    using (var zipStream = new GZipStream(outMemStream, CompressionMode.Compress))
    {
     inMemStream.CopyTo(zipStream);
    }

    var output = outMemStream.GetBuffer();
    Console.WriteLine("GZip: {0}% ({1} bytes)", (double)output.Length / input.Length * 100, output.Length);
   }
  }

  [Fact] // Timeout(100)]
  public void testDeflate()
  {
   var input = genRandomBytes(1000);

   using (var outMemStream = new MemoryStream())
   {
    using (var inMemStream = new MemoryStream(input))
    using (var zipStream = new DeflateStream(outMemStream, CompressionMode.Compress))
    {

     // TODO
     checkStreamEncode(zipStream, outMemStream, input);

     inMemStream.CopyTo(zipStream);
    }

    var output = outMemStream.GetBuffer();
    Console.WriteLine("Deflate: {0}% ({1} bytes)", (double)output.Length / input.Length * 100, output.Length);
   }
  }

  private byte[] genRandomBytes(int len)
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

  private byte[] genRandomBytes(int len, double byteChangeProb)
  {
   var data = new byte[len];
   var random = new Random();
   byte currByte = 0;

   for(int j=0; j<len; j++)
   {
    if(random.NextDouble() < byteChangeProb)
     currByte = (byte)random.Next(256);

    data[j] = currByte;
   }

   return data;
  }

  private string genText(int len, double charChangeProb)
  {
   var text = string.Empty;
   var random = new Random();
   char ch = ' ';

   for(int j=0; j<len; j++)
   {
    if(random.NextDouble() < charChangeProb)
     ch=(char)('a'+random.Next(26));

    text += ch;
   }

   return text;
  }

  private byte[] checkStreamEncode(Stream encodeStream, MemoryStream outStream, byte[] input)
  {
   using (var inMemStream = new MemoryStream(input))
   using (encodeStream)
   {
    inMemStream.CopyTo(encodeStream);
   }

   byte[] output = outStream.GetBuffer();
   Console.WriteLine("Stream Encode: {0} bytes -> {1} bytes = {2}% compression",
    input.Length, output.Length, (double)output.Length / input.Length * 100);
   return output;
  }

  private string checkCodec(Codec codec, string input, string expectedEncoded)
  {
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Console.WriteLine("{0} -> {1}", input, encoded);

   if (expectedEncoded != null)
    Assert.Equal(expectedEncoded, encoded); //, "Unexpected encoded data");

   Assert.Equal(input, output); //, "Encode then decode must produce original data");

   // TODO: this fails for the binary RLE codec
   Assert.True(encoded.Length <= input.Length); //, "Codec must not expand data");

   Assert.NotEqual(input, encoded); //, "Encoding must change data");

   return encoded;
  }

  [Fact] // Timeout(150)]
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

  [Fact] // Timeout(100)]
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

  [Fact] // Timeout(100)]
  public void testHuffmanCodecWithRandomData()
  {
   var random = new Random();
   var codec1 = new HuffmanCodec();
   var totalDecodedSize = 0;
   var totalEncodedSize = 0;

   for (int i = 0; i < 20; i++)
   {
    var input = "";
    char ch = ' ';

    for (int j = 0; j < 20; j++)
    {
     if (random.NextDouble() < 0.5)
      ch = (char)('a' + random.Next(26));

     input += ch;
    }

    var encoded = checkCodec(codec1, input, null);
    totalDecodedSize += input.Length * 8;
    totalEncodedSize += encoded.Length;
   }

   Console.WriteLine();
   Console.WriteLine("*** Compression ratio: {0}% (encoded size vs original size, in bits) ***", (double)totalEncodedSize / totalDecodedSize * 100);
  }

  [Fact] // Timeout(400)]
  public void testMultipleCodecs()
  {
   var input3 = "a";
   var output3 = "a";
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

   var input7 = genText(1000, 1.0);
   Console.WriteLine("Compression ratio on {0} random letters (encoded size vs original size):", input7.Length);

   var encoded = codec1.encode(input7);
   Console.WriteLine("Char RLE: {0}%", (double)encoded.Length / input7.Length * 100);

   encoded = codec3.encode(input7);
   Console.WriteLine("Huffman: {0}%", (double)encoded.Length / 8.0 / input7.Length * 100);
  }

/*
  [Fact]
  public void encodeThenDecodeEmptyDataMustProduceOriginalData()
  {
   const string input = "";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.Equal(input, output);
  }

  [Fact]
  public void encodeThenDecodeManyRepeatedCharsMustProduceOriginalData()
  {
   const string input = "qdttpmmmmmmmmmmhmm";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.Equal(input, output);
  }

  [Fact]
  public void encodeThenDecodeMustProduceOriginalData()
  {
   //const string input = "Hello World";
   var input = "hhheelooo   woorrrlllld!!";
   var expectedEncoded = "h3e2lo3 3wo2r3l4d!2";

   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   Assert.Equal(expectedEncoded, encoded, "Unexpected encoded data");
   var output = codec.decode(encoded);
   Assert.Equal(input, output);
  }

  [Fact]
  public void verifyRunLengthEncodedData()
  {
   const string input = "Hello Wooorld";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   Assert.Equal("Hel2o Wo3rld", encoded);
  }
*/

  [Fact]
  public void codecMustNotExpandData()
  {
   const string input = "Hello World";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.True(encoded.Length <= input.Length);
  }
 }
}
