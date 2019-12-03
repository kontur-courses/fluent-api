using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting
{
    public class PrintingConfigBase
    {
        protected const string NullRepresentation = "null";
        protected const char Indentation = '\t';

        protected static bool IsFinalType(Type type) => type.IsPrimitive || FinalTypes.Contains(type);

        private static IEnumerable<Type> FinalTypes => new[]
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };
    }
}