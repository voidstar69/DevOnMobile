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
 public class BinaryTreeTests
 {
  [TestMethod]
  public void testBinaryTree()
  {
   var t = new BinaryTree();
   t.Add(5);
   t.Add(8);
   t.Add(2);
   
   Console.WriteLine("Binary tree contents in order:");

   foreach(int item in t)
   {
    Console.WriteLine(item);
   }
  }
 }
}
