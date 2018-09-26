using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevOnMobile.Tests
{
 [TestClass]
 public class BinaryTreeTests
 {
  [TestMethod]
  public void TestBinaryTree()
  {
   var tree = new BinaryTree<int> {5, 8, 2};
   
   Console.WriteLine("Binary tree contents in order:");
   foreach (int item in tree)
   {
    Console.WriteLine(item);
   }

   // TODO: tree should put numbers into order
   int[] expectedItemOrder = { 2, 8, 5 };
   using(var enu = tree.GetEnumerator())
   {
    foreach (int expItem in expectedItemOrder)
    {
     Assert.IsTrue(enu.MoveNext());
     Assert.AreEqual(expItem, enu.Current);
    }
   }
  }
  
  [TestMethod]
  public void TestBinaryTreeNonGeneric()
  {
   var tree = new BinaryTree<int> {5, 8, 2};
   var enumerator = ((IEnumerable)tree).GetEnumerator();
   
   // TODO: tree should put numbers into order
   int[] expectedItemOrder = { 2, 8, 5 };
   foreach (int expItem in expectedItemOrder)
   {
    Assert.IsTrue(enumerator.MoveNext());
    Assert.AreEqual(expItem, enumerator.Current);
   }
  }
 }
}