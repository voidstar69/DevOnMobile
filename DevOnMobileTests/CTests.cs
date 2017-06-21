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
 public class InterpreterTests
 {
  [TestMethod()]
  public void CProgramTest()
  {
   var code =
@"print 'a'
print 'b'";

   var app = new Interpreter();
   app.CStyleExecute(code, System.Console.Out);
   //Assert.AreEqual(123, c.bar());
  }

  [TestMethod()]
  public void LispProgramTest()
  {
   var code =
@"(print 'a' (
  add 1 2))";

   var app = new Interpreter();
   app.LispStyleExecute(code, System.Console.Out);
   //Assert.AreEqual(123, c.bar());
  }
 }
}
