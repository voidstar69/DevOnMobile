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
  [TestMethod()]
  public void addTest()
  {
   A a = new A();
   C c = a.add(new B(), new B());
   Assert.AreEqual(-1, c.bar());
  }
 }
}
