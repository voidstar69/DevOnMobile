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
/*
  //[TestMethod()]
  public void addTest()
  {
   A a = new A();
   C c = a.add(new B(), new B());
   Assert.AreEqual(123, c.bar());
  }
*/

  [TestMethod]
  public void testCodec()
  {
   const string input = "Hello World";
   var codec = new A();
   var encoded = codec.encode(input);
   var output = codec.decode(encoded);
   Assert.AreEqual(input, output);
  }
 }
}
