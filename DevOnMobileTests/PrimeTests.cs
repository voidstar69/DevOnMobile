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
 public class PrimeTests
 {
  [TestMethod()]
  public void TestIsPrime()
  {
   Console.WriteLine("Testing numbers for Primality:");
   var prime = new Prime();
   for (int i = 1; i < 1000000; i++)
   {
    bool isPrime1 = prime.IsPrimeSlow(i);
    bool isPrime2 = prime.IsPrimeFast(i);
//    Console.Write(i);
//    Console.Write(',');
    Assert.AreEqual(isPrime1, isPrime2, string.Format("Num: {0}, Slow: {1}, Fast: {2}", i, isPrime1, isPrime2));
   }
  }
 }
}
