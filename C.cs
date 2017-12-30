using System;
using System.Collections;
using System.IO;

namespace DevOnMobile
{
 public class LispInterpreter
 {
/*
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
*/

  public void Exec(string program, TextWriter output)
  {
   var text = program.Replace('\n',' ');

   ArrayList dataList;
   using(var input=new StringReader(program))
   {
    dataList = Eval(text,input,output,0);
   }

   if (dataList == null)
    return;

   //Console.Write('=');
   bool first = true;
   foreach (var item in dataList)
   {
    if (first)
     first = false;
    else
     output.Write(' ');

    output.Write(item);
   }
   output.WriteLine();
  }

  private ArrayList Eval(string expr,TextReader input,TextWriter output,int indent)
  {
   Console.Write(new string(' ', indent));
   Console.WriteLine("Eval " + expr);

   var dataList = new ArrayList();
   string prefix="";
   int ch;
   while(-1!=(ch=input.Read()) && ch!=')')
   {
    if('(' == ch)
    {
     dataList.Add(prefix);
     prefix="";

     dataList.Add(
Eval(null,input,output,indent+1));
    }
    else if(' '==ch)
    {
     dataList.Add(prefix);
     prefix="";
    }
    else
     prefix+=(char)ch;
   }

//   return dataList;


/*
   // TODO: parentheses handling is broken!
   var trimChars = new char[]{'(',')'};
   var splitChars = new char[]{' '};
   var tokens = expr.Trim(trimChars).Split(splitChars, 2);
   var cmd = tokens[0];

   //ArrayList dataList;
   if (tokens.Length == 1)
   {
    dataList = new ArrayList();
   }
   else
   {
    dataList = Eval(tokens[1], input, output, indent+1);
   }
*/

   Console.Write(new string(' ', indent));
   Console.Write("Eval "+expr+" => ");
   ArrayList result = null;

/*
   double num;
   if(double.TryParse(cmd, out num))
   {
    dataList.Insert(0, num);
    result = dataList;
    //return dataList;
   }
*/

   var cmd=dataList[0] as string;

   //if(result == null)
   switch(cmd)
   {
    case "print":
     foreach(var item in dataList)
     {
      output.Write(item);
      output.Write(' ');
     }
     output.WriteLine();
     //dataList.ForEach(x => output.Write(x));
     //output.WriteLine(dataList.ToString());
     break;

    case "reverse":
     result = Reverse(dataList);
     break;

    case "add":
     result = new ArrayList{ Add(dataList) };
     break;

    case "mul":
     result = new ArrayList{ Reduce(dataList, (x,y)=>x*y) };
     break;
   }

   if(result == null)
    Console.Write("null");
   else
   foreach (var item in result)
   {
    Console.Write(item);
    Console.Write(' ');
   }
   Console.WriteLine();

   return result;
  }

  private ArrayList Reverse(ArrayList list)
  {
   if (list.Count <= 1)
    return list;

   var head = list[0];
   list.RemoveAt(0);
   var result = Reverse(list);
   result.Add(head);
   return result;
  }

  private double Add(ArrayList list)
  {
   if (list.Count == 0)
    return 0.0;
   if (list.Count == 1)
    return (double)list[0];

   var head = list[0];
   list.RemoveAt(0);
   var result = Add(list);
   result += (double)head;
   return result;
  }

  private double Reduce(ArrayList list, Func<double,double,double> Op)
  {
   if (list.Count == 0)
    return 0.0;
   if (list.Count == 1)
    return (double)list[0];

   var head = list[0];
   list.RemoveAt(0);
   var result = Reduce(list,Op);
   return Op(result,(double)head);
  }
 }
}