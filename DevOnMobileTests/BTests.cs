using System;
using Xunit;

namespace DevOnMobile.Tests
{
 public class BinaryTreeTests
 {
  [Fact]
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
     Assert.True(enu.MoveNext());
     Assert.Equal(expItem, enu.Current);
    }
   }
  }
 }
}
