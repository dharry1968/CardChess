﻿using System;
using System.Reflection;

namespace App.Model
{
    public class Inject : Attribute
    {
        public object[] Args;
        public PropertyInfo PropertyType;
        public Type ValueType;
        public Inject(params object[] args)
        {
        }
    }
}