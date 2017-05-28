using System;
using System.Collections.Generic;
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
   return int.Parse(ch.ToString());
  }

  public void WriteBit(int bit)
  {
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

//     bit = (i == data.Length ? '\0' : data[i]);
   }

//   if (bit != '\0')
//    output += bit;

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
