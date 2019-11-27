using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        public PrintingConfig()
        {
            typeExclusions = new HashSet<Type>();
            propertyExclusions = new HashSet<string>();
            typePrintingFunctions = new Dictionary<Type, Delegate>();
            typeCultures = new Dictionary<Type, CultureInfo>();
            propertyPrintingFunctions = new Dictionary<string, Delegate>();
            propertyStringsLength = new Dictionary<string, int>();
        }
        
        private readonly HashSet<Type> typeExclusions;
        private readonly HashSet<string> propertyExclusions;
        private readonly Dictionary<Type, Delegate> typePrintingFunctions;
        private readonly Dictionary<Type, CultureInfo> typeCultures;
        private readonly Dictionary<string, Delegate> propertyPrintingFunctions;
        private readonly Dictionary<string, int> propertyStringsLength;
        
        Dictionary<Type, Delegate> IPrintingConfig.TypePrintingFunctions => typePrintingFunctions;
        Dictionary<Type, CultureInfo> IPrintingConfig.TypeCultures => typeCultures;
        Dictionary<string, Delegate> IPrintingConfig.PropertyPrintingFunctions => propertyPrintingFunctions;
        Dictionary<string, int> IPrintingConfig.PropertyStringsLength => propertyStringsLength;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                propertyExclusions.Add(memberExpression.Member.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typeExclusions.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (typePrintingFunctions.TryGetValue(obj.GetType(), out var printingFunc))
                return (string) printingFunc.DynamicInvoke(obj);
            
            if (finalTypes.Contains(obj.GetType()))
                if (typeCultures.TryGetValue(obj.GetType(), out  var cultureInfo))
                    return string.Format(cultureInfo, obj.ToString());
                else
                    return obj.ToString();

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (typeExclusions.Contains(propertyInfo.PropertyType) || propertyExclusions.Contains(propertyInfo.Name)) continue;
                string valueString;
                if (propertyPrintingFunctions.TryGetValue(propertyInfo.Name, out printingFunc))
                    valueString = (string) printingFunc.DynamicInvoke(propertyInfo.GetValue(obj));
                else
                    valueString = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (propertyStringsLength.TryGetValue(propertyInfo.Name, out var length))
                    valueString = valueString.Substring(0, length);
                sb.AppendLine(indentation + propertyInfo.Name + " = " + valueString);
            }
            return sb.ToString();
        }
    }

    interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<string, Delegate> PropertyPrintingFunctions { get; }
        Dictionary<string, int> PropertyStringsLength { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}