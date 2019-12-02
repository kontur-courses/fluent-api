using System;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Formatting
{
    public abstract class FormatConfiguration
    {
        public readonly Type[] FinalTypes;

        protected FormatConfiguration()
        {
            FinalTypes = CustomFinalTypes == null
                ? DefaultFinalTypes
                : DefaultFinalTypes.Concat(CustomFinalTypes).ToArray();
        }

        public abstract int MaximumRecursionDepth { get; protected set;  }
        public abstract string GetPropertyPrintingStart(int nestingLevel, PropertyInfo property);
        public abstract string GetIndent(int nestingLevel);
        public abstract string GetEnumerableIndent(int nestingLevel, int index);
        public abstract string GetDictionaryElementVisualisation(int nestingLevel, string key, string value);
        public abstract string GetBorder(int nestingLevel, bool isStart);
        public abstract string GetGenericVisualisation(params Type[] types);

        
        protected abstract Type[] CustomFinalTypes { get; }

        public static FormatConfiguration Default =>
            new DefaultFormatting();
        
        private static readonly Type[] DefaultFinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
    }
}