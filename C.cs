﻿using System;
using System.Collections;
using System.IO;

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
   var dataList = Eval(text, output);

   if (dataList == null)
    return;

   //Console.Write('=');
   foreach (var item in dataList)
   {
    output.Write(item);
    output.Write(' ');
   }
   //output.WriteLine();
  }

  private ArrayList Eval(string expr, TextWriter output)
  {
   Console.Write("Eval " + expr);

   var trimChars = new char[]{'(',')'};
   var splitChars = new char[]{' '};
   var tokens = expr.Trim(trimChars).Split(splitChars, 2);
   var cmd = tokens[0];

   ArrayList dataList;
   if (tokens.Length == 1)
   {
    dataList = new ArrayList();
   }
   else
   {
    dataList = Eval(tokens[1], output);
   }

   double num;
   if(double.TryParse(cmd, out num))
   {
    dataList.Insert(0, num);
    return dataList;
   }

   ArrayList result;
   switch(cmd)
   {
    case "print":
     foreach(var item in dataList)
     {
      output.Write(item);
      output.Write(' ');
     }
     //     dataList.ForEach(x => output.Write(x));
     //output.WriteLine(dataList.ToString());
     break;

    case "reverse":
     result = Reverse(dataList);
     break;

    case "add":
     result = new ArrayList{ Add(dataList) };
     break;
   }

   Console.Write('=');
   foreach (var item in dataList)
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
 }
}