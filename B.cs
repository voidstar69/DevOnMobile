using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class Node<T>
 {
  public T data;
  public Node<T> next;
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
   var node = new Node<T>();
   node.data = item;
   node.next = root;
   root = node;
  }

  public IEnumerator<T> GetEnumerator()
  {
   return new BinaryTreeEnumerator<T>(root);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
   return null;
  }
 }

 public class BinaryTreeEnumerator<T> : IEnumerator<T>
 {
  private Node<T> beforeRoot;

  public BinaryTreeEnumerator(Node<T> root)
  {
   beforeRoot = new Node<T>();
   beforeRoot.next = root;
   _current = beforeRoot;
  }

  private Node<T> _current;
  // Implement the IEnumerator(T).Current publicly, but implement 
  // IEnumerator.Current, which is also required, privately.
  public T Current
  {
   get
   {
    return _current.data;
   }
  }

  private object Current1
  {
   get { return this.Current; }
  }

  object IEnumerator.Current
  {
   get { return Current1; }
  }

  // Implement MoveNext and Reset, which are required by IEnumerator.
  public bool MoveNext()
  {
   _current = _current.next;
   if (_current == null)
    return false;
   return true;
  }

  public void Reset()
  {
   _current = beforeRoot;
  }

  // Implement IDisposable, which is also implemented by IEnumerator(T).
  private bool disposedValue = false;
  public void Dispose()
  {
   Dispose(true);
   GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
   if (!this.disposedValue)
   {
    if (disposing)
    {
     // Dispose of managed resources.
    }
    _current = null;
   }

   this.disposedValue = true;
  }

  ~BinaryTreeEnumerator()
  {
   Dispose(false);
  }
 }
}
