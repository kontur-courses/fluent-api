using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class FinalTypesLibrary
    {
        public static readonly HashSet<Type> NumberTypes = new HashSet<Type>
            {typeof(int), typeof(double), typeof(float), typeof(long)};

        public static readonly HashSet<Type> FinalTypes = new HashSet<Type>(NumberTypes)
            {typeof(string), typeof(DateTime), typeof(TimeSpan)};
    }
}