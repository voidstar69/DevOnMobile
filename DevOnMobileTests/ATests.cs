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
  public void encodeThenDecodeMustProduceOriginalData()
  {
   const string input = "Hello World";
   var codec = new RunLengthCodec();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
   Assert.AreEqual("Hel2o World", encoded);
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
