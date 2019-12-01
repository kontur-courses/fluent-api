using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class PrintingConfigBase
    {
        protected const string NullRepresentation = "null";
        protected const char Indentation = '\t';

        protected static IReadOnlyList<Type> FinalTypes => new[] // TODO: type warning
        {
            typeof(string), typeof(int), typeof(double), typeof(float),
            typeof(DateTime), typeof(TimeSpan)
        };
    }
}