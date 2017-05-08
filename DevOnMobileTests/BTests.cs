using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevOnMobile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DevOnMobile.Tests
{
 //[TestClass()]
 public class BTests
 {
  [TestMethod()]
  public void fooTest()
  {
   B b = new B();
   b.foo();
  }
 }
}
