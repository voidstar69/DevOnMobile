using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevOnMobile
{
    public interface ITextCodec
    {
        string encode(string data);
        string decode(string data);
    }

    public interface IStreamCodec
    {
        void encode(Stream inputStream, Stream outputStream);
        void decode(Stream inputStream, Stream outputStream);
        int dictionarySize { get; } // only populated after full decoding
    }

    public interface IInputBitStream
    {
        // All methods return null when reading past the end of the stream
        int? ReadBit(); 
        uint? ReadBits(byte numBits);
        byte? ReadByte();
    }

    public interface IOutputBitStream
    {
        void WriteBit(int bit);
        void WriteBits(uint value, byte numBits);
        void WriteByte(byte value);
    }

    // Read bits from a byte stream (with metadata stored in final byte of stream)
    public class InputBitStream : IInputBitStream
    {
        private readonly Stream stream;
        private readonly bool storeLengthAtEndMode;
        private byte bits;
        private byte numBits;

        // Only used when in storeLengthAtEndMode
        private int nextByte = -1;
        private int nextNextByte = -1;

        public InputBitStream(Stream stream, bool storeLengthAtEndMode = true)
        {
            this.stream = stream;
            this.storeLengthAtEndMode = storeLengthAtEndMode;
        }

        private int ReadByteFromUnderlyingStream()
        {
            if (!storeLengthAtEndMode)
            {
                numBits = 8;
                return stream.ReadByte();
            }

            // Not in Trailing Zero Bits mode, so always need to look two bytes
            // ahead to spot final data byte length followed by EndOfStream marker

            if (nextByte == -1)
            {
                nextByte = stream.ReadByte();
            }
            if (nextNextByte == -1)
            {
                nextNextByte = stream.ReadByte();
            }

            int currentByte = nextByte;
            nextByte = nextNextByte;
            nextNextByte = stream.ReadByte();

            if (nextNextByte == -1)
            {
                // At end of stream. Second-to-last byte is the final data byte.
                // Last byte is the number of valid bits in the final data byte.
                numBits = (byte)nextByte;
                nextByte = -1;
            }
            else
            {
                numBits = 8;
            }

            return currentByte;
        }

        public int? ReadBit()
        {
            if (numBits == 0)
            {
                int data = ReadByteFromUnderlyingStream();
                if (data == -1)
                    return null;
                bits = (byte)data;
            }

            checked
            {
                int bit = bits & 0x1;
                bits = (byte)(bits >> 1);
                numBits--;
                return bit;
            }
        }

        public uint? ReadBits(byte numBits)
        {
            uint value = 0;
            uint bitMask = 1;
            for (var bitPos = 1; bitPos <= numBits; bitPos++)
            {
                int? bit = ReadBit();
                if (bit == null)
                    return null;
                value |= (uint)(bit * bitMask);
                bitMask <<= 1;
            }
            return value;
        }

        public byte? ReadByte()
        {
            byte value = 0;
            for (var bitPos = 1; bitPos <= 8; bitPos++)
            {
                value >>= 1;
                int? bit = ReadBit();
                if (bit == null)
                    return null;
                value |= (byte)(bit * 0x80);
            }
            return value;
        }
    }

    // Write bits to a byte stream (with metadata stored in final byte of stream)
    public class OutputBitStream : IOutputBitStream, IDisposable
    {
        private readonly Stream stream;
        private readonly bool storeLengthAtEndMode;
        private byte bits;
        private byte numBits;
        private bool flushed = false;

        public OutputBitStream(Stream stream, bool storeLengthAtEndMode = true)
        {
            this.stream = stream;
            this.storeLengthAtEndMode = storeLengthAtEndMode;
        }

        public void WriteBit(int bit)
        {
            if (bit != 0 && bit != 1)
                throw new ArgumentOutOfRangeException("bit", bit, "bit must be 0 or 1");

            checked
            {
                bits = (byte)(bits + (bit << numBits));
                numBits++;
            }

            if (numBits == 8)
            {
                stream.WriteByte(bits);
                bits = 0;
                numBits = 0;
            }
        }

        public void WriteBits(uint value, byte numBits)
        {
            for(var bitPos = 1; bitPos <= numBits; bitPos++)
            {
                WriteBit((byte)(value & 1));
                value >>= 1;
            }
        }

        public void WriteByte(byte value)
        {
            for(var bitPos = 1; bitPos <= 8; bitPos++)
            {
                WriteBit(value & 1);
                value >>= 1;
            }
        }

        private void Flush()
        {
            if (flushed)
            {
                throw new ApplicationException("OutputBitStream may only be flushed once!");
            }

            if (storeLengthAtEndMode)
            {
                // write final bits into byte stream, and also count of final bits
                if (numBits > 0)
                {
                    stream.WriteByte(bits);
                    stream.WriteByte(numBits);
                }
                else
                {
                    // no final bits to write, so final byte is full
                    stream.WriteByte(8);
                }
            }
            else
            {
                // write final bits into byte stream
                if (numBits > 0)
                {
                    stream.WriteByte(bits);
                }
            }

            bits = 0;
            numBits = 0;
            flushed = true;
        }

        public void Dispose()
        {
            Flush();
        }
    }

    // Read and write bits in a stream, backed by a text string storing '0' and '1' characters
    public class BitsAsTextStream : IInputBitStream, IOutputBitStream
 {
  private string data;

  public BitsAsTextStream()
  {
   data = string.Empty;
  }

  public BitsAsTextStream(string data)
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

  public uint? ReadBits(byte numBits)
  {
    throw new NotImplementedException();
  }

  public void WriteBits(uint value, byte numBits)
  {
    throw new NotImplementedException();
  }

  public byte? ReadByte()
  {
   byte value = 0;
   for (int bitPos = 1; bitPos <= 8; bitPos++)
   {
    value >>= 1;
    int? bit = ReadBit();
    if (bit == null)
        return null;
    value |= (byte)(bit * 0x80);
   }
   return value;
  }

  public void WriteByte(byte value)
  {
   for(int bitPos = 1; bitPos <= 8; bitPos++)
   {
    WriteBit(value & 1);
    value >>= 1;
   }
  }

  public string GetData()
  {
   return data;
  }

  //public int Length => data.Length;
 }

 public class HuffmanCodec : ITextCodec, IStreamCodec
 {
     private static TextWriter log;
        
     static HuffmanCodec()
     {
#if DEBUG
      //log = Console.Out;
      log = TextWriter.Null;
#else
      log = TextWriter.Null;
#endif
     }

     // This represents a tree node (leaf node or internal node).
     // Every node has either two children or no children.
     // TODO: use [Serializable] ?
     private class Node
     {
         public byte byteValue; // TODO: only leaf nodes need this
         public int frequency; // number of occurrences of this byte value. TODO: we may be able to get rid of this

         public Node parent;

         // TODO: leaf nodes do not need children pointers
         public Node leftChild;
         public Node rightChild;

         public bool IsLeaf()
         {
             return leftChild == null && rightChild == null;
         }

         // Serialise the tree (rooted at this node) into the bit stream.
         // This only serialises data required by the decoder.
         public void Serialise(IOutputBitStream stream)
         {
             if (IsLeaf())
             {
                 log.WriteLine("Leaf node {0} (0x{0:X})", byteValue);
                 stream.WriteBit(1);
                 stream.WriteByte(byteValue);
             }
             else
             {
                 log.WriteLine("Internal node");
                 stream.WriteBit(0);
                 leftChild.Serialise(stream);
                 rightChild.Serialise(stream);
             }
         }

         // Deserialise the tree (rooted at this node) from the bit stream.
         // This only deserialises data required by the decoder.
         public void Deserialise(IInputBitStream stream)
         {
             int? bit = stream.ReadBit();
             if (bit == null)
                 throw new ArgumentNullException("stream.ReadBit", "Unexpected end-of-stream");

             bool isLeaf = (bit == 1);
             if (isLeaf)
             {
                 byte? byteOrNull = stream.ReadByte();
                 if (byteOrNull == null)
                     throw new ArgumentNullException("stream.ReadByte", "Unexpected end-of-stream");
                 byteValue = byteOrNull.Value;
                 log.WriteLine("Leaf node {0} (0x{0:X})", byteValue);
             }
             else
             {
                 log.WriteLine("Internal node");
                 leftChild = new Node();
                 leftChild.Deserialise(stream);
                 rightChild = new Node();
                 rightChild.Deserialise(stream);
             }
         }
     }

     public int dictionarySize { get; private set; }

     // encode binary data -> binary data
     public void encode(Stream inputStream, Stream outputStream)
     {
         using (var outputBitStream = new OutputBitStream(outputStream))
         {
             EncodeInternal(inputStream, outputBitStream);
         }
     }

     // encode text to bits, stored as a sequence of characters 0 or 1
     public string encode(string text)
     {
         if (string.IsNullOrEmpty(text))
             return text;

         var encoding = new ASCIIEncoding();
         byte[] rawBytes = encoding.GetBytes(text);
         using (var inputStream = new MemoryStream(rawBytes))
         {
             var outputStream = new BitsAsTextStream();
             EncodeInternal(inputStream, outputStream);
             return outputStream.GetData();
         }
     }

     private void EncodeInternal(Stream inputStream, IOutputBitStream outputBitStream)
     {
         Node treeRoot;
         Dictionary<byte, Node> byteToNodeDict = BuildHuffmanTree(inputStream, out treeRoot);
         dictionarySize = byteToNodeDict.Count;

         // store the Huffman coding tree in the bitstream, so the decoder can reconstruct the tree
         log.WriteLine("Serialising Huffman tree:");
         treeRoot.Serialise(outputBitStream);
        
         // seek to the start of the input data
         long newPos = inputStream.Seek(0, SeekOrigin.Begin);
         if (newPos != 0)
             throw new ArgumentOutOfRangeException("inputStream.Position", newPos,
                 "Seeked a Stream to the start but it returned a different position!");

         // translate each input byte value into a variable number of bits
         log.WriteLine("Huffman encoding each byte to a variable number of bits:");
         int byteOrFlag;
         while (-1 != (byteOrFlag = inputStream.ReadByte()))
         {
             byte num = (byte) byteOrFlag;

             // use a stack to write the bits in reverse order
             var stack = new Stack<int>(8); // TODO: reverse bits more efficiently?
             log.Write("{0}=", num);
             for (var node = byteToNodeDict[num]; node.parent != null; node = node.parent)
             {
                 int bit = (node == node.parent.leftChild ? 0 : 1);
                 stack.Push(bit);
                 log.Write(bit);
             }

             log.Write('/');
             foreach (int bit in stack)
             {
                 outputBitStream.WriteBit(bit);
                 log.Write(bit);
             }

             log.WriteLine();
         }
     }

     static Dictionary<byte, Node> BuildHuffmanTree(Stream inputStream, out Node treeRoot)
     {
         var byteToNodeDict = new Dictionary<byte, Node>(byte.MaxValue);
         var freqtoTreeDict = new SortedList<int, IList<Node>>((int) inputStream.Length); // TODO: guess number of internal 'tree' nodes?
         var frequency = new int[byte.MaxValue + 1];

         // track frequency (number of occurences) of each unique byte value
         int byteOrFlag;
         while (-1 != (byteOrFlag = inputStream.ReadByte()))
         {
             var num = (byte) byteOrFlag;
             frequency[num]++;
         }

         // build a collection of (tree) nodes, one per unique byte value that occurs in the input
         for (int num = byte.MinValue; num <= byte.MaxValue; num++)
         {
             var byteValue = (byte) num;
             int freq = frequency[num];
             if (freq > 0)
             {
                 var node = new Node {byteValue = byteValue, frequency = freq};
                 byteToNodeDict.Add(byteValue, node);
                 if (!freqtoTreeDict.ContainsKey(freq)) freqtoTreeDict[freq] = new List<Node>();
                 freqtoTreeDict[freq].Add(node);
             }
         }

         // loop until there is only one root node, i.e. a single tree
         while (freqtoTreeDict.Count > 1 || freqtoTreeDict.First().Value.Count > 1)
         {
             // find the two nodes/trees with lowest frequency
             var nodeList = freqtoTreeDict.First().Value;
             var lowFreqNode1 = nodeList.First();
             nodeList.RemoveAt(0);
             if (nodeList.Count == 0) freqtoTreeDict.RemoveAt(0);
             nodeList = freqtoTreeDict.First().Value;
             var lowFreqNode2 = nodeList.First();
             nodeList.RemoveAt(0);
             if (nodeList.Count == 0) freqtoTreeDict.RemoveAt(0);

             // combine these two nodes/trees
             // TODO: internal node should not have byteValue!
             var parent = new Node
             {
                 leftChild = lowFreqNode1,
                 rightChild = lowFreqNode2,
                 frequency = lowFreqNode1.frequency + lowFreqNode2.frequency
             };
             lowFreqNode1.parent = parent;
             lowFreqNode2.parent = parent;
             if (!freqtoTreeDict.ContainsKey(parent.frequency))
                 freqtoTreeDict[parent.frequency] = new List<Node>();
             freqtoTreeDict[parent.frequency].Add(parent);
         }

         // Huffman coding tree has now been built. Get tree root node.
         Node root = freqtoTreeDict.Single().Value.Single();

         // Tree must have at least two levels so that walking the tree from leaf to root produces at least one transition / one bit
         if (root.IsLeaf())
         {
             Node leftChild = root;
             var rightChild = new Node {leftChild = null, rightChild = null};
             root = new Node {parent = null, leftChild = leftChild, rightChild = rightChild};
             leftChild.parent = root;
             rightChild.parent = root;
         }

         treeRoot = root;
         return byteToNodeDict;
     }

     public string decode(string text)
     {
         if (string.IsNullOrEmpty(text))
             return text;

         using (MemoryStream outputStream = new MemoryStream())
         {
             var inputBitStream = new BitsAsTextStream(text);
             DecodeInternal(inputBitStream, outputStream);
             return new ASCIIEncoding().GetString(outputStream.ToArray());
         }
     }

     public void decode(Stream inputStream, Stream outputStream)
     {
         if (inputStream.Position != 0)
             throw new ArgumentOutOfRangeException("inputStream", "Expected stream at starting position");

         // reconstruct the Huffman coding tree from the bitstream
         var inputBitStream = new InputBitStream(inputStream);

         DecodeInternal(inputBitStream, outputStream);
     }

    private void DecodeInternal(IInputBitStream inputBitStream, Stream outputStream)
    {
        // reconstruct the Huffman coding tree from the bitstream
        var treeRoot = new Node();
        log.WriteLine("Deserialising Huffman tree:");
        treeRoot.Deserialise(inputBitStream);

        log.WriteLine("Huffman decoding groups of bits into bytes:");
        var node = treeRoot;
        int? bitOrNull;
        while (null != (bitOrNull = inputBitStream.ReadBit()))
        {
            int bit = bitOrNull.Value;
            node = (bit == 0 ? node.leftChild : node.rightChild);
            log.Write(bit);

            if (node.IsLeaf())
            {
                outputStream.WriteByte(node.byteValue);
                log.WriteLine("={0}", node.byteValue);
                node = treeRoot; // reset to root of tree
            }
        }
    }
}

 // run length encoding where run length of bits is stored in next 2 bits
 public class BinaryRunLengthCodec : ITextCodec
 {
  public string encode(string text)
  {
   if(string.IsNullOrEmpty(text))
    return text;

   var input = new BitsAsTextStream(text);
   var output = new BitsAsTextStream();

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

   var input = new BitsAsTextStream(text);
   var output = new BitsAsTextStream();

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
 public class CharacterRunLengthCodec : ITextCodec
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
