using System;
//using System.Collections;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class BinaryTree : IEnumerable<int>
 {
  public void Add(int item)
  {
  }

  public IEnumerator<int> GetEnumerator()
  {
   return new Enumerator<int>();
  }

  private IEnumerator GetEnumerator()
  {
   return null;
  }
 }
}
