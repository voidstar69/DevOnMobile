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
 public class CTests
 {
  [TestMethod()]
  public void barTest()
  {
   C c = new C();
   Assert.AreEqual(-1, c.bar());
  }
 }
}
