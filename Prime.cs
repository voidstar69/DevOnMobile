using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class Prime
 {
  public bool IsPrimeSlow(int num)
  {
   int limit = (int)Math.Sqrt(num);
   for (int i = 2; i <= limit; i++)
   {
    if (num % i == 0)
     return false;
   }
   return true;
  }

  public bool IsPrimeFast(int num)
  {
   // TODO: write an optimised algorithm
   return IsPrimeSlow(num);
  }
 }
}
