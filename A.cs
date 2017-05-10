using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 interface Codec
 {
  string encode(string data);
  string decode(string data);
 }

 // run length encoding where run length is stored in next character
 public class RunLengthCodec : Codec
 {
  public string encode(string data)
  {
   string output = "";
   int runLen = 1;
   char prevCh = '\0';

   foreach(char ch in data)
   {
    if (ch == prevCh)
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
   return new string(data.Reverse().ToArray());
  }
 }
}
