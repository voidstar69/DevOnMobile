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
   var tree = new BinaryTree<int>();
   tree.Add(5);
   tree.Add(8);
   tree.Add(2);

   Console.WriteLine("Binary tree contents in order:");
   foreach (int item in tree)
   {
    Console.WriteLine(item);
   }

   // TODO: tree should put numbers into order
   int[] expectedItemOrder = { 2, 8, 5 };
   var enu = tree.GetEnumerator();
   foreach (var expItem in expectedItemOrder)
   {
    Assert.IsTrue(enu.MoveNext());
    Assert.AreEqual(expItem, enu.Current);
   }
  }
 }
}
