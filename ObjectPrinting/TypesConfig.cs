using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class TypesConfig
    {
        public static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid),
            typeof(decimal), typeof(long), typeof(short),
            typeof(uint), typeof(bool), typeof(byte),
            typeof(sbyte), typeof(ushort), typeof(ulong),
            typeof(char), typeof(IntPtr)
        };
    }
}