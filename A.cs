﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOnMobile
{
 public class A
 {
  public A()
  {
  }

  public string encode(string data)
  {
   return new string(data.Reverse().ToArray());
  }

  public string decode(string data)
  {
   return new string(data.Reverse().ToArray());
  }

/*
  public C add(B b1, B b2)
  {
   return new C();
  }
*/
 }
}
