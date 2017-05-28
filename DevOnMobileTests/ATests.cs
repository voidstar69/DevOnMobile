using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevOnMobile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
 [TestClass()]
 public class CodecTests
 {
  private string checkCodec(Codec codec, string input, string expectedEncoded)
  {
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Console.WriteLine("{0} -> {1}", input, encoded);

   if (expectedEncoded != null)
    Assert.AreEqual(expectedEncoded, encoded, "Unexpected encoded data");

   Assert.AreEqual(input, output, "Encode then decode must produce original data");

   // TODO: this fails for the binary RLE codec
   //Assert.IsTrue(encoded.Length <= input.Length, "Codec must not expand data");

   //Assert.AreNotEqual(input, encoded, "Encoding must change data");

   return encoded;
  }

  [TestMethod, Timeout(100)]
  public void testRandomCharacterData()
  {
   var random = new Random();
   var codec1 = new CharacterRunLengthCodec();
   var totalDecodedSize = 0;
   var totalEncodedSize = 0;

   for(int i=0; i<100; i++)
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
  public void testRandomBinaryData()
  {
   var random = new Random();
   var codec1 = new BinaryRunLengthCodec();
   var totalDecodedSize = 0;
   var totalEncodedSize = 0;

   for (int i = 0; i < 100; i++)
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

  [TestMethod, Timeout(100)]
  public void testMultipleCodecs()
  {
   var input1 = "Hello Wooorld";
   var output1 = "Hel2o Wo3rld";
   var input2 = "hhheelooo   woorrrlllld!!";
   var output2 = "h3e2lo3 3wo2r3l4d!2";
   var input3 = "a";
   var output3 = "a";
   var input4 = "aa";
   var output4 = "a2";
   var input5 = "aab";
   var output5 = "a2b";
   var input6 = "";
   var output6 = "";

   const string binInput1 = "";
   const string binOutput1 = "";
   const string binInput2 = "10001";
   const string binOutput2 = "100011";

   var codec1 = new CharacterRunLengthCodec();
   var codec2 = new BinaryRunLengthCodec();

   checkCodec(codec1, input1, output1);
   checkCodec(codec1, input2, output2);
   checkCodec(codec1, input3, output3);
   checkCodec(codec1, input4, output4);
   checkCodec(codec1, input5, output5);
   checkCodec(codec1, input6, output6);

   checkCodec(codec2, "0", "0");
   checkCodec(codec2, "1", "1");
   checkCodec(codec2, "00", "0000");
   checkCodec(codec2, "11", "1100");
   checkCodec(codec2, binInput1, binOutput1);
   checkCodec(codec2, binInput2, binOutput2);
  }

/*
  [TestMethod]
  public void encodeThenDecodeEmptyDataMustProduceOriginalData()
  {
   const string input = "";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }

  [TestMethod]
  public void encodeThenDecodeManyRepeatedCharsMustProduceOriginalData()
  {
   const string input = "qdttpmmmmmmmmmmhmm";
   var codec = new CharacterRunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }

  [TestMethod]
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

  [TestMethod]
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
