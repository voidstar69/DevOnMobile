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

    var tokens = line.Split(' ');
    var cmd = tokens[0];
    var data = tokens[1];
    switch(cmd)
    {
     case "print":
      output.WriteLine(data.Trim('\''));
      break;

     case "if":
      if(Eval(tokens[1]))
       Eval(tokens[2]);
      break;
    }
   }
  }

  private bool Eval(string expr)
  {
   return false;
  }
 }
}