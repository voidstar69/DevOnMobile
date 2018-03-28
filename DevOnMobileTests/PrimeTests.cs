using System;
using Xunit;

namespace DevOnMobile.Tests
{
 public class PrimeTests
 {
  [Fact]
  public void TestIsPrime()
  {
   Console.WriteLine("Testing numbers for Primality");
   var prime = new Prime();
   for (var i = 1000000; i > 0; i--)
   {
    bool isPrime1 = Prime.IsPrimeSlow(i);
    bool isPrime2 = prime.IsPrimeFast(i);
//    Console.Write(i);
//    Console.Write(',');
    Assert.Equal(isPrime1, isPrime2);
//    Assert.AreEqual(isPrime1, isPrime2, string.Format("Num: {0}, Slow: {1}, Fast: {2}\nDebug slow: {3}\nDebug fast: {4}", i, isPrime1, isPrime2, prime.DebugSlow, prime.DebugFast));
   }
  }

  [Fact]
  public void IsPrimeFastPerformance()
  {
   var prime = new Prime();
   for (var i = 1; i < 10000000; i++)
   {
    prime.IsPrimeFast(i);
   }
  }

 [Fact]
  public void IsPrimeSlowPerformance()
  {
   var prime = new Prime();
   for (var i = 1; i < 10000000; i++)
   {
    Prime.IsPrimeSlow(i);
   }
  }
 }
}
