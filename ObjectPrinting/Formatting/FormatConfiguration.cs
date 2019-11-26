using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Formatting
{
    public abstract class FormatConfiguration
    {
        public readonly Type[] FinalTypes;
        
        public FormatConfiguration()
        {
            FinalTypes = CustomFinalTypes == null
                ? DefaultFinalTypes
                : DefaultFinalTypes.Concat(CustomFinalTypes).ToArray();
        }
        
        public abstract int MaximumRecursionDepth { get; }
        public abstract string GetPropertyPrintingStart(int nestingLevel, PropertyInfo property);
        public abstract string GetIndent(int nestingLevel);
        public abstract string GetEnumerableIndent(int nestingLevel, int index);
        public abstract string GetBorder(int nestingLevel, bool isStart);

        protected abstract Type[] CustomFinalTypes { get; }

        public static FormatConfiguration DefaultFormatting() =>
            new DefaultFormatting();
        
        private static readonly Type[] DefaultFinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
    }
}