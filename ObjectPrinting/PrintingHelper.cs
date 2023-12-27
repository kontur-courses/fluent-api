using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class PrintingHelper
    {
        public const int MaxCyclicDepth = 2;
        
        public static readonly string NewLine = Environment.NewLine;
        public static readonly string NullString = $"null{NewLine}";

        public static readonly HashSet<Type> FinalTypes = new HashSet<Type>(new[]
        {
            typeof(byte), typeof(short), typeof(int), typeof(long), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(char), typeof(bool),
        });
    }
}