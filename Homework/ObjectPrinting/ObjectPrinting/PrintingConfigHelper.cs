using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting
{
    internal static class PrintingConfigHelper
    {
        public const string NullRepresentation = "null";
        public const char Indentation = '\t';

        public static bool IsFinalType(Type type) => type.IsPrimitive || finalTypes.Contains(type);

        private static readonly IEnumerable<Type> finalTypes = new[]
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };
    }
}