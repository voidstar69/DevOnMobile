﻿using System;
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
   var code = @"print (reverse 1 2 3)";

   var app = new Interpreter();
   app.LispStyleExecute(code, Console.Out);
//   Assert.AreEqual("3 2 1", app.Output);
  }
 }
}
