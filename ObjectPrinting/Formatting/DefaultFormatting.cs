using System;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Formatting
{
    public class DefaultFormatting : FormatConfiguration
    {
        public DefaultFormatting(int maximumRecursionDepth = 20)
        {
            MaximumRecursionDepth = maximumRecursionDepth;
        }
        
        public override string GetGenericVisualisation(params Type[] types)
        {
            if(types == null || types.Length == 0)
                throw new ArgumentException($"{nameof(types)} can't be null or empty");
            
            if (types.Length == 1)
                return $"<{types[0].Name}>";

            var resultStr = new StringBuilder("<");
            for (var index = 0; index < types.Length; index++)
            {
                var t = types[index].Name;
                resultStr.Append(t + (index + 1 != types.Length ? ", " : ""));
            }

            resultStr.Append(">");
            return resultStr.ToString();
        }

        public override int MaximumRecursionDepth { get; protected set; }

        public override string GetPropertyPrintingStart(int level, PropertyInfo property) =>
            GetIndent(level) + property.Name + " = ";

        public override string GetIndent(int level) => 
            '\n' + new string('\t', level);

        public override string GetEnumerableIndent(int nestingLevel, int index) =>
            GetIndent(nestingLevel) + $"#{index} = ";

        public override string GetDictionaryElementVisualisation(int nestingLevel, string key, string value) =>
            GetIndent(nestingLevel) + $"[{key}] = " + value;

        public override string GetBorder(int nestingLevel, bool isStart) =>
            GetIndent(nestingLevel) + (isStart ? '{' : '}');

        protected override Type[] CustomFinalTypes { get; } = null;
    }
}