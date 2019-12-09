using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    internal static class PrintingConfigHelper
    {
        public const string NullRepresentation = "null";
        public const char Indentation = '\t';

        public static bool IsFinalType(Type type) => type.IsPrimitive || finalTypes.Contains(type);

        private static readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };
    }
}