using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
  public void testLispAdd()
  {
   var code = @"add 1 2 3";
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec(code, writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("6\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void testLispAddThenPrint()
  {
   var code = @"print (add 1 2 3)";
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec(code, writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("6 \r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void testLispReverse()
  {
   var code = @"reverse 1 2 3";
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec(code, writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("3 2 1\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void testLispOps()
  {
   var app = new LispInterpreter();
   var output = Console.Error;
   using (var writer = new StringWriter())
   {
    app.Exec("reverse 4 2 3 1", writer);
    app.Exec("add 4 2 3", writer);
    app.Exec("mul 4 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("1 3 2 4\r\n9\r\n24\r\n", writer.ToString());
   }
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
