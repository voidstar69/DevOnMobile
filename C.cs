using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class Interpreter
 {
  public void Execute(string program, TextWriter output)
  {
   var lines = program.Split('\n');
   foreach(var line in lines)
   {
    output.Write(">> ");
    output.WriteLine(line);

    var tokens = line.Split(' ', 2);
    var cmd = tokens[0];
    var data = tokens[1];
    switch(data)
    {
     "print":
      output.Write(data);
      break;
    }
   }
  }
 }
}