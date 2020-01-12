using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
 [TestClass]
 public class PrimeTests
 {
  [TestMethod, Timeout(10000)]
  public void TestIsPrime()
  {
   Console.WriteLine("TestIsPrime: starting");
   var prime = new Prime();
   for (var i = 10000; i > 0; i--)
   {
    bool isPrime1 = Prime.IsPrimeSlow(i);
    bool isPrime2 = prime.IsPrimeFast(i);
    Assert.AreEqual(isPrime1, isPrime2,
        string.Format("Num: {0}, Slow: {1}, Fast: {2}\nDebug slow: {3}\nDebug fast: {4}", i, isPrime1, isPrime2,
            prime.DebugSlow, prime.DebugFast));
   }
   Console.WriteLine("TestIsPrime: finished");
  }

  [TestMethod, Timeout(10000)]
  public void IsPrimeFastPerformance()
  {
   Console.WriteLine("IsPrimeFastPerformance: starting");
   var prime = new Prime();
   for (var i = 1; i < 10000; i++)
   {
    prime.IsPrimeFast(i);
   }
   Console.WriteLine("IsPrimeFastPerformance: finished");
  }

  [TestMethod, Timeout(10000)]
  public void IsPrimeSlowPerformance()
  {
   Console.WriteLine("IsPrimeSlowPerformance: starting");
   var prime = new Prime();
   for (var i = 1; i < 10000; i++)
   {
    Prime.IsPrimeSlow(i);
   }
   Console.WriteLine("IsPrimeSlowPerformance: finished");
  }
 }
}
