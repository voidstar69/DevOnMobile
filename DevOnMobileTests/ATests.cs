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
  private void checkCodec(Codec codec, string input, string expectedEncoded)
  {
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output, "encodeThenDecodeMustProduceOriginalData");
   Assert.AreNotEqual(input, encoded, "encodingMustChangeData");
   Assert.AreEqual(expectedEncoded, encoded);
  }

  [TestMethod]
  public void testMultipleCodecs()
  {
   const string input1 = "Hello World";
   const string output1 = "Hel2o Wo3rld";
   const string input2 = "";
   const string output2 = "";
   var codec1 = new RunLengthCodec();

   checkCodec(codec1, input1, output1);
   checkCodec(codec1, input2, output2);
  }

  [TestMethod]
  public void encodeThenDecodeMustProduceOriginalData()
  {
   const string input = "Hello World";
   var codec = new RunLengthCodec();
   var encoded = codec.encode(input);
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
