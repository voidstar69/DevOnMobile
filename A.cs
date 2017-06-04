using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public interface Codec
 {
  string encode(string data);
  string decode(string data);
 }

 public class BinaryStream
 {
  private string data;

  public BinaryStream()
  {
   data = string.Empty;
  }

  public BinaryStream(string data)
  {
   this.data = data;
  }

  public int? ReadBit()
  {
   if(data.Length == 0)
    return null;

   char ch = data[0];
   data = data.Substring(1);
   var bit = int.Parse(ch.ToString());

   if (bit != 0 && bit != 1)
    throw new ArgumentOutOfRangeException("bit", bit, "bit must be 0 or 1");
   return bit;
  }

  public void WriteBit(int bit)
  {
   if (bit != 0 && bit != 1)
    throw new ArgumentOutOfRangeException("bit", bit, "bit must be 0 or 1");
   data += bit.ToString();
  }

  public string GetData()
  {
   return data;
  }

  public int Length
  {
   get {
    return data.Length;
   }
  }
 }

 public class HuffmanCodec : Codec
 {
  private class Node // TODO: this represents leaf nodes. Adapt it to also represent internal nodes.
  {
   public byte byteValue;
   public int frequency; // number of occurences of this byte value. TODO: we may be able to get rid of this
//   public byte newByteValue; // TODO: only for testing

   public Node parent;

   // TODO: left nodes do not need children pointers
   public Node leftChild;
   public Node rightChild;
  }

  // TODO: horrible hack!
  private Node lastTree;

  public string encode(string text)
  {
   if (string.IsNullOrEmpty(text))
    return text;

   Console.WriteLine("Huffman encoding:");
   var encoding = new ASCIIEncoding();
   byte[] rawBytes = encoding.GetBytes(text);
   var freqtoTreeDict = new SortedList<int, IList<Node>>(rawBytes.Length); // TODO: guess number of internal 'tree' nodes?
   var byteToNodeDict = new Dictionary<byte, Node>(byte.MaxValue);
   int[] frequency = new int[byte.MaxValue + 1];

   using (MemoryStream inputStream = new MemoryStream(rawBytes))
   {
    // track frequency (number of occurences) of each unique byte value
    int byteOrFlag;
    while(-1 != (byteOrFlag = inputStream.ReadByte()))
    {
     byte num = (byte)byteOrFlag;
     frequency[num]++;
    }

    // build a collection of (tree) nodes, one per unique byte value that occurs in the input
    for (int num = byte.MinValue; num <= byte.MaxValue; num++)
    {
     var byteValue = (byte)num;
     var freq = frequency[num];
     if (freq > 0)
     {
      var node = new Node { byteValue = byteValue, frequency = freq };
      byteToNodeDict.Add(byteValue, node);

      if (!freqtoTreeDict.ContainsKey(freq))
       freqtoTreeDict[freq] = new List<Node>();
      freqtoTreeDict[freq].Add(node);
     }
    }

    // loop until there is only one root node, i.e. a single tree
    while(freqtoTreeDict.Count > 1 || freqtoTreeDict.First().Value.Count > 1)
    {
     // find the two nodes/trees with lowest frequency
     var nodeList = freqtoTreeDict.First().Value;
     var lowFreqNode1 = nodeList.First();
     nodeList.RemoveAt(0);
     if (nodeList.Count == 0)
      freqtoTreeDict.RemoveAt(0);

     nodeList = freqtoTreeDict.First().Value;
     var lowFreqNode2 = nodeList.First();
     nodeList.RemoveAt(0);
     if (nodeList.Count == 0)
      freqtoTreeDict.RemoveAt(0);

     // combine these two nodes/trees
     // TODO: internal node should not have byteValue or newByteValue!
     var parent = new Node { leftChild = lowFreqNode1, rightChild = lowFreqNode2,
      frequency = lowFreqNode1.frequency + lowFreqNode2.frequency };
     lowFreqNode1.parent = parent;
     lowFreqNode2.parent = parent;

     if (!freqtoTreeDict.ContainsKey(parent.frequency))
      freqtoTreeDict[parent.frequency] = new List<Node>();
     freqtoTreeDict[parent.frequency].Add(parent);
    }
    
    // seek to the start of the input data
    long newPos = inputStream.Seek(0, SeekOrigin.Begin);
    if (newPos != 0)
     throw new ArgumentOutOfRangeException("Seek position", newPos, "Seeked a MemoryStream to the start but it returned a different position!");

    // translate each input byte value into a variable number of bits
    var outputStream = new BinaryStream();
    while (-1 != (byteOrFlag = inputStream.ReadByte()))
    {
     byte num = (byte)byteOrFlag;
     //     outputStream.WriteByte(byteToNodeDict[num].newByteValue);

     // use a stack to write the bits in reverse order
     var stack = new Stack<int>(8); // TODO: reverse bits more efficiently?

     Console.Write("{0}=", num);
     for (var node = byteToNodeDict[num]; node.parent != null; node = node.parent)
     {
      int bit = (node == node.parent.leftChild ? 0 : 1);
      stack.Push(bit);
      Console.Write(bit);
     }
     Console.Write('/');

     foreach (int bit in stack)
     {
      outputStream.WriteBit(bit);
      Console.Write(bit);
     }
     Console.WriteLine();
    }

    // TODO: need to transmit the Huffman coding tree to the decoder!
    // TODO: horrible hack!
    lastTree = freqtoTreeDict.First().Value.Single();

    return outputStream.GetData();
    //return encoding.GetString(outputStream.GetBuffer());
   }
  }

  public string decode(string text)
  {
   if (string.IsNullOrEmpty(text))
    return text;

   Console.WriteLine("Huffman decoding:");
   var input = new BinaryStream(text);
   using (MemoryStream outputStream = new MemoryStream())
   {
    var node = lastTree;
    int? bitOrNull;
    while (null != (bitOrNull = input.ReadBit()))
    {
     int bit = bitOrNull.Value;
     node = (bit == 0 ? node.leftChild : node.rightChild);
     Console.Write(bit);

     if (node.leftChild == null && node.rightChild == null)
     {
      outputStream.WriteByte(node.byteValue);
      Console.WriteLine("={0}", node.byteValue);
      node = lastTree; // reset to root of tree
     }
    }

    return new ASCIIEncoding().GetString(outputStream.ToArray());
   }
  }
 }

 public class BinaryRunLengthCodec : Codec
 {
  public string encode(string text)
  {
   if(string.IsNullOrEmpty(text))
    return text;

   var input = new BinaryStream(text);
   var output = new BinaryStream();

   int runLen = 1;
   int prevBit = input.ReadBit().Value;
   bool outOfBits = false;

   while (!outOfBits)
   {
    int? bitOrNull = input.ReadBit();
    int bit = bitOrNull.HasValue ? bitOrNull.Value : -1;
    outOfBits = !bitOrNull.HasValue;
    
    if (bit == prevBit && runLen < 5)
    {
     runLen++;
    }
    else
    {
     output.WriteBit(prevBit);
     if (runLen > 1)
     {
      output.WriteBit(prevBit);
      runLen -= 2;
      output.WriteBit(runLen >> 1);
      output.WriteBit(runLen & 1);
     }
     runLen = 1;
    }
    prevBit = bit;
   }
   
   return output.GetData();
  }

  public string decode(string text)
  {
   if (string.IsNullOrEmpty(text))
    return text;

   var input = new BinaryStream(text);
   var output = new BinaryStream();

   int bit = input.ReadBit().Value;
   bool outOfBits = false;

   while (!outOfBits)
   {
    int prevBit = bit;

    int? bitOrNull = input.ReadBit();
    bit = bitOrNull ?? -1;
    outOfBits = !bitOrNull.HasValue;

    if (bit != prevBit)
    {
     output.WriteBit(prevBit);
    }
    else
    {
     int runLen = (input.ReadBit().Value << 1) + input.ReadBit().Value + 2;

     for (int j = 0; j < runLen; j++)
     {
      output.WriteBit(bit);
     }

     bit = input.ReadBit() ?? -1;
     if (bit == -1)
      outOfBits = true;
    }
   }

   return output.GetData();
  }
 }

 // run length encoding where run length is stored in next character
 public class CharacterRunLengthCodec : Codec
 {
  public string encode(string data)
  {
   if(string.IsNullOrEmpty(data))
    return data;

   string output = "";
   int runLen = 1;
   char prevCh = data[0];

   for(int i = 1; i <= data.Length; i++)
   {
    char ch = (i == data.Length ? '\0' : data[i]);

    if (ch == prevCh && runLen < 9)
    {
     runLen++;
    }
    else
    {
     output += prevCh;
     if(runLen > 1)
     {
      output += runLen;
     }
     runLen = 1;
    }
    prevCh = ch;
   }

   return output;
  }

  public string decode(string data)
  {
   if(string.IsNullOrEmpty(data))
    return data;

   string output = "";
   char ch = data[0];

   for(int i = 1; i < data.Length; i++)
   {
    char prevCh = ch;
    ch = data[i];

    if (!char.IsDigit(ch))
    {
     output += prevCh;
    }
    else
    {
     int runLen = ch - '0';
     for(int j = 0; j < runLen; j++)
     {
      output += prevCh;
     }
     
     i++;
     ch = (i == data.Length ? '\0' : data[i]);
    }
   }

   if(ch != '\0')
    output += ch;

   return output;
  }
 }
}
