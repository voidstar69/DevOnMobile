using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class Interpreter
 {
  public void Execute(string program, Stream output)
  {
   var lines = program.Split(program, '\n');
   foreach(var line in lines)
   {
    output.WriteLine(line);
   }
  }
 }
}