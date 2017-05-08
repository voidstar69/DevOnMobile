using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class Prime
 {
  private List<int> primes = new List<int>(new int[]{2,3,5,7,11});

  public Prime()
  {
  }

  public bool IsPrimeSlow(int num)
  {
   if (num < 2)
    return false;

   int limit = (int)Math.Sqrt(num);
   for (int i = 2; i <= limit; i++)
   {
    if (num % i == 0)
    {
     DebugSlow = "Factor: " + i;
     return false;
    }
   }
   return true;
  }

  public bool IsPrimeFast(int num)
  {
   if (num < 2)
    return false;

   int limit = (int)Math.Sqrt(num);
   foreach(int factor in primes)
   {
     if (factor > limit)
      break;
     if (num % factor == 0)
     {
      DebugFast = "Factor: " + i;
      return false;
     }

//     if (num == factor)
//      return true;
//     if (num < factor)
//      return false;
   }

   primes.Add(num);
   return true;
  }

  public string DebugSlow
  {
   get; set;
  }

  public string DebugFast
  {
   get; set;
  }
 }
}
