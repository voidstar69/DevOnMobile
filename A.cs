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
 }

 public class BinaryCodec : Codec
 {
  public string encode(string data)
  {
   var input = new BinaryStream(data);
   var output = new BinaryStream();
   
   return output.GetData();
  }

  public string decode(string data)
  {
   return data;
  }
 }


 // run length encoding where run length is stored in next character
 public class RunLengthCodec : Codec
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
