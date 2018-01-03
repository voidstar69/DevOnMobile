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
  public void Add()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("add 1 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("6\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void Mul()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("mul 2 3 4", writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("24\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void AddThenPrint()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("print (add 1 2 3)", writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("6 \r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void Reverse()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("reverse 1 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("3 2 1\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void Operations()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("reverse 4 2 3 1", writer);
    app.Exec("add 4 2 3", writer);
    app.Exec("mul 4 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.AreEqual("1 3 2 4\r\n9\r\n24\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void ReverseAndAdd()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("reverse (add 1 2 3) (add 4 5)", writer);
    Assert.AreEqual("9 6\r\n", writer.ToString());
   }
  }

  [TestMethod()]
  public void Range()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("(range 1 9)", writer);
    app.Exec("(range 1 9 3)", writer);
    Assert.AreEqual("1 2 3 4 5 6 7 8 9\r\n1 4 7\r\n", writer.ToString());
   }
  }

  // todo: parsing of nested parentheses is broken!
  [TestMethod()]
  public void AddPrintWithParentheses()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("print (add (1) (2) (3))", writer);
    Assert.AreEqual("6\r\n", writer.ToString());
   }
  }
 }
}
