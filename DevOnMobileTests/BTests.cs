using System;
using Xunit;

namespace DevOnMobile.Tests
{
 public class BinaryTreeTests
 {
  [Fact]
  public void testBinaryTree()
  {
   var tree = new BinaryTree<int>();
   tree.Add(5);
   tree.Add(8);
   tree.Add(2);

   Console.WriteLine("Binary tree contents in order:");
   foreach (var item in tree)
   {
    Console.WriteLine(item);
   }

   // TODO: tree should put numbers into order
   int[] expectedItemOrder = { 2, 8, 5 };
   var enu = tree.GetEnumerator();
   foreach (var expItem in expectedItemOrder)
   {
    Assert.True(enu.MoveNext());
    Assert.Equal(expItem, enu.Current);
   }
  }
 }
}
