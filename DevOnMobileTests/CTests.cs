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
  public void simpleProgramTest()
  {
   var code =
@"print 'a'
print 'b'";

   var app = new Interpreter();
   app.Execute(code, System.Console.Out);
   //Assert.AreEqual(123, c.bar());
  }
 }
}
