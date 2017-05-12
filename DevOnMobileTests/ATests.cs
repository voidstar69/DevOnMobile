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
 public class ATests
 {
[TestMethod]
  public void testRandomData()
  {
   var random = new Random();
   var codec1 = new RunLengthCodec();

   for(int i=0; i<100; i++)
   {
    var input = "";
    for(int j=0; j<20; j++)
    {
     input = input + (char)('a' + random.Next(26));
    }

    Console.Write(input + " -> ");
    var encoded = checkCodec(codec1, input, null);
    Console.WriteLine(encoded);
   }
  }

  private string checkCodec(Codec codec, string input, string expectedEncoded)
  {
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);

   if(expectedEncoded != null)
    Assert.AreEqual(expectedEncoded, encoded, "Unexpected encoded data");

   Assert.AreEqual(input, output, "encodeThenDecodeMustProduceOriginalData");
//   Assert.AreNotEqual(input, encoded, "encodingMustChangeData");

   return encoded;
  }

  [TestMethod]
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
   
   var codec1 = new RunLengthCodec();

   checkCodec(codec1, input1, output1);
   checkCodec(codec1, input2, output2);
   checkCodec(codec1, input3, output3);
   checkCodec(codec1, input4, output4);
   checkCodec(codec1, input5, output5);
  }

  [TestMethod]
  public void encodeThenDecodeEmptyDataMustProduceOriginalData()
  {
   const string input = "";
   var codec = new RunLengthCodec();
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

   var codec = new RunLengthCodec();
   var encoded = codec.encode(input);
   Assert.AreEqual(expectedEncoded, encoded, "Unexpected encoded data");
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }

  [TestMethod]
  public void encodingMustChangeData()
  {
   const string input = "Hello World";
   var codec = new RunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreNotEqual(input, encoded);
  }

  [TestMethod]
  public void verifyRunLengthEncodedData()
  {
   const string input = "Hello Wooorld";
   var codec = new RunLengthCodec();
   var encoded = codec.encode(input);
   Assert.AreEqual("Hel2o Wo3rld", encoded);
  }

  //[TestMethod]
  public void codecMustShrinkData()
  {
   const string input = "Hello World";
   var codec = new RunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.IsTrue(encoded.Length < input.Length);
  }
 }
}
