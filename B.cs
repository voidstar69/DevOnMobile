using System;
using System.Collections;
using System.Collections.Generic;

namespace DevOnMobile
{
 public class Node<T>
 {
  public T Data { get; set; }
  public Node<T> Next { get; set; }
 }

 public class BinaryTree<T> : IEnumerable<T>
 {
  private Node<T> root;

  public BinaryTree()
  {
   root = null;
  }

  public void Add(T item)
  {
      var node = new Node<T>
      {
          Data = item,
          Next = root
      };
      root = node;
  }
  
  public IEnumerator<T> GetEnumerator()
  {
   return EnumeratorImpl();
  }
  
  IEnumerator IEnumerable.GetEnumerator()
  {
   return EnumeratorImpl();
  }
  
  private IEnumerator<T> EnumeratorImpl()
  {
   var curr = root;
   while (curr != null)
   {
    yield return curr.Data;
    curr = curr.Next;
   }
  }
 }
}