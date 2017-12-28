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
   app.CStyleExecute(code, System.Console.Error);
   //Assert.AreEqual(123, c.bar());
  }

  [TestMethod()]
  public void testLispReverse()
  {
   var code = @"print (reverse 1 2 3)";

   var app = new Interpreter();
   app.LispStyleExecute(code, Console.Error);
//   Assert.AreEqual("3 2 1", app.Output);
  }

  [TestMethod()]
  public void testLispAdd()
  {
   var code = @"print (add 1 2 3)";

   var app = new Interpreter();

   //using(var writer=TextWriter())
   {
    app.LispStyleExecute(code,
//writer);
Console.Error);
   }

//   Assert.AreEqual("6", app.Output);
  }

  [TestMethod()]
  public void testLispOps()
  {
var app = new Interpreter();
var out = Console.Error.
app.LispStyleExecute("reverse 4 2 3 1", out);
app.LispStyleExecute("add 4 2 3", out);
app.LispStyleExecute("mul 4 2 3", out);
  }

// todo: parsing of nested parentheses is broken!
/*
  [TestMethod()]
  public void testLispAddWithParentheses()
  {
   var code = @"print (add (1) (2) (3))";

   var app = new Interpreter();

   app.LispStyleExecute(code, Console.Error);

//   Assert.AreEqual("6", app.Output);
  }

  [TestMethod()]
  public void testLispReverseAndAdd()
  {
   var code = @"print (reverse (add 1 2 3) (add 4 5))";

   var app = new Interpreter();

   app.LispStyleExecute(code, Console.Error);

//   Assert.AreEqual("9 6", app.Output);
  }
*/
 }
}
