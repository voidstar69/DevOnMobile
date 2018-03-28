using System;
using System.IO;
using Xunit;

namespace DevOnMobile.Tests
{
 public class LispInterpreterTests
 {
  /*
    [Fact]
    public void CProgramTest()
    {
     var code =
  @"print 'a'
  print 'b'";

     var app = new Interpreter();
     app.CStyleExecute(code, System.Console.Error);
     //Assert.Equal(123, c.bar());
    }
  */

  [Fact]
  public void Add()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("add 1 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.Equal("6\r\n", writer.ToString());
   }
  }

  [Fact]
  public void Mul()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("mul 2 3 4", writer);
    Console.Error.Write(writer.ToString());
    Assert.Equal("24\r\n", writer.ToString());
   }
  }

  [Fact]
  public void AddThenPrint()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("print (add 1 2 3)", writer);
    Console.Error.Write(writer.ToString());
    Assert.Equal("6\r\n", writer.ToString());
   }
  }

  [Fact]
  public void Reverse()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("reverse 1 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.Equal("3 2 1\r\n", writer.ToString());
   }
  }

  [Fact]
  public void Operations()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("reverse 4 2 3 1", writer);
    app.Exec("add 4 2 3", writer);
    app.Exec("mul 4 2 3", writer);
    Console.Error.Write(writer.ToString());
    Assert.Equal("1 3 2 4\r\n9\r\n24\r\n", writer.ToString());
   }
  }

  [Fact]
  public void ReverseAndAdd()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("reverse (add 1 2 3) (add 4 5)", writer);
    Assert.Equal("9 6\r\n", writer.ToString());
   }
  }

  [Fact]
  public void AddPrintWithParentheses()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("print (add (1) (2) (3))", writer);
    Assert.Equal("6\r\n", writer.ToString());
   }
  }

  [Fact]
  public void Range()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("range 1 9", writer);
    app.Exec("range 1 9 3", writer);
    Assert.Equal("1 2 3 4 5 6 7 8 9\r\n1 4 7\r\n", writer.ToString());
   }
  }

  [Fact]
  public void RangeWithOtherOps()
  {
   var app = new LispInterpreter();
   using (var writer = new StringWriter())
   {
    app.Exec("add (range 1 100)", writer);
    app.Exec("mul (range 1 10)", writer);
    Assert.Equal("5050\r\n3628800\r\n", writer.ToString());
   }
  }
 }
}
