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
 public class PrimeTests
 {
  [TestMethod()]
  public void TestIsPrime()
  {
   Console.WriteLine("Testing numbers for Primality");
   var prime = new Prime();
   for (int i = 1000000; i > 0; i--)
   {
    bool isPrime1 = prime.IsPrimeSlow(i);
    bool isPrime2 = prime.IsPrimeFast(i);
//    Console.Write(i);
//    Console.Write(',');
    Assert.AreEqual(isPrime1, isPrime2);
//    Assert.AreEqual(isPrime1, isPrime2, string.Format("Num: {0}, Slow: {1}, Fast: {2}\nDebug slow: {3}\nDebug fast: {4}", i, isPrime1, isPrime2, prime.DebugSlow, prime.DebugFast));
   }
  }

  [TestMethod()]
  public void IsPrimeFastPerformance()
  {
   var prime = new Prime();
   for (int i = 1; i < 10000000; i++)
   {
    prime.IsPrimeFast(i);
   }
  }

 [TestMethod()]
  public void IsPrimeSlowPerformance()
  {
   var prime = new Prime();
   for (int i = 1; i < 10000000; i++)
   {
    prime.IsPrimeSlow(i);
   }
  }
 }
}
