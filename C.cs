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
  public void CStyleExecute(string program, TextWriter output)
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

//     case "if":
//      if(Eval(tokens[1], output))
//       Eval(tokens[2], output);
//      break;
    }
   }
  }

  public void LispStyleExecute(string program, TextWriter output)
  {
   var text = program.Replace('\n',' ');
   Eval(text, output);
  }

  private string Eval(string expr, TextWriter output)
  {
   var tokens = expr.Trim('(', ')').Split(' ', 1);
   var cmd = tokens[0];
   var data = Eval(tokens[1], output);
   switch(cmd)
   {
    case "print":
      output.WriteLine(data.Trim('\''));
      break;

    // TODO
    case "add":
     return data;
   }

   return string.Empty;
  }
 }
}