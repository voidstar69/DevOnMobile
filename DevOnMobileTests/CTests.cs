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
 public class LispInterpreterTests
 {
/*
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
*/

  [TestMethod()]
  public void testLispReverse()
  {
   var code = @"print (reverse 1 2 3)";

   var app = new LispInterpreter();
   app.Exec(code, Console.Error);
//   Assert.AreEqual("3 2 1", app.Output);
  }

  [TestMethod()]
  public void testLispAdd()
  {
   var code = @"print (add 1 2 3)";

   var app = new LispInterpreter();

   //using(var writer=TextWriter())
   {
    app.Exec(code,
//writer);
Console.Error);
   }

//   Assert.AreEqual("6", app.Output);
  }

  [TestMethod()]
  public void testLispOps()
  {
var app = new LispInterpreter();
var output = Console.Error;
app.Exec("reverse 4 2 3 1", output);
app.Exec("add 4 2 3", output);
app.Exec("mul 4 2 3", output);
  }

// todo: parsing of nested parentheses is broken!
/*
  [TestMethod()]
  public void testLispAddWithParentheses()
  {
   var code = @"print (add (1) (2) (3))";

   var app = new LispInterpreter();

   app.LispStyleExecute(code, Console.Error);

//   Assert.AreEqual("6", app.Output);
  }

  [TestMethod()]
  public void testLispReverseAndAdd()
  {
   var code = @"print (reverse (add 1 2 3) (add 4 5))";

   var app = new LispInterpreter();

   app.LispStyleExecute(code, Console.Error);

//   Assert.AreEqual("9 6", app.Output);
  }
*/
 }
}
