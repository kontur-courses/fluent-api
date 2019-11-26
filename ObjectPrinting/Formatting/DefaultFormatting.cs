using System;
using System.Reflection;

namespace ObjectPrinting.Formatting
{
    public class DefaultFormatting : FormatConfiguration
    {
        public override int MaximumRecursionDepth { get; } = 20;

        public override string GetPropertyPrintingStart(int level, PropertyInfo property) =>
            GetIndent(level) + property.Name + " = ";

        public override string GetIndent(int level) => 
            '\n' + new string('\t', level);

        public override string GetEnumerableIndent(int nestingLevel, int index) =>
            GetIndent(nestingLevel) + $"#{index} = ";

        public override string GetBorder(int nestingLevel, bool isStart) =>
            GetIndent(nestingLevel) + (isStart ? '{' : '}');

        protected override Type[] CustomFinalTypes { get; } = null;
    }
}