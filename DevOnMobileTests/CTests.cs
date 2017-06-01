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
 public class CTests
 {
  [TestMethod()]
  public void barTest()
  {
   var app = new Interpreter();
   app.Execute("10 print 'a'", System.Console);
   //Assert.AreEqual(123, c.bar());
  }
 }
}
