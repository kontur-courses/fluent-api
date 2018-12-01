using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    internal static class TypeExtensions
    {
        private static readonly HashSet<Type> FinalTypes;

        static TypeExtensions()
        {
            FinalTypes = new HashSet<Type>
            {
                typeof(string), typeof(int), typeof(double), typeof(long),
                typeof(float), typeof(decimal), typeof(DateTime), typeof(TimeSpan),
                typeof(Guid)
            };
        }

        public static bool IsFinal(this Type type) => FinalTypes.Contains(type);
    }
}
