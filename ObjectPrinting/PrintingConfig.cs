using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> PrintersForTypes { get; }
        Dictionary<Type, CultureInfo> CultureInfoForTypes { get; }
        Dictionary<string, Func<object, string>> PrintersForPropertiesNames { get; }
        int? MaxLength { get; set; }
    }

    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<string> excluded = new List<string>();

        private readonly Dictionary<Type, CultureInfo> cultureInfoForTypes =
            new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Func<object, string>> printersForTypes =
            new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<string, Func<object, string>> printersForPropertiesName =
            new Dictionary<string, Func<object, string>>();
        private int? maxLength;

        Dictionary<Type, Func<object, string>> IPrintingConfig.PrintersForTypes => printersForTypes;
        Dictionary<Type, CultureInfo> IPrintingConfig.CultureInfoForTypes => cultureInfoForTypes;
        Dictionary<string, Func<object, string>> IPrintingConfig.PrintersForPropertiesNames => printersForPropertiesName;
        int? IPrintingConfig.MaxLength { get => maxLength; set => maxLength = value; }

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
            excluded.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private bool HasSpecialPrintingValue(object value, Type type, string name, out string specialPrintingValue)
        {
            specialPrintingValue = null;

            if (excludedTypes.Contains(type))
            {
                specialPrintingValue = string.Empty;
                return true;
            }

            if (excluded.Contains(name))
            {
                specialPrintingValue = string.Empty;
                return true;
            }

            if (printersForTypes.ContainsKey(type))
            {
                specialPrintingValue = $"{name} = {printersForTypes[type].Invoke(value)}\r\n";
                return true;
            }

            if (cultureInfoForTypes.TryGetValue(type, out var culture))
            {
                specialPrintingValue = $"{name} = {((IFormattable)value).ToString(null, culture)}\r\n";
                return true;
            }

            if (printersForPropertiesName.ContainsKey(name))
            {
                specialPrintingValue = $"{name} = {printersForPropertiesName[name].Invoke(value)}\r\n";
                return true;
            }
            if (maxLength != null && value is string s)
            {
                specialPrintingValue = $"{name} = {s.Substring(0, (int)maxLength)}\r\n";
                return true;
            }

            return false;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);

            if (obj == null) return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (nestingLevel > 10) return "REC" + Environment.NewLine;
            if (finalTypes.Contains(type)) return obj + Environment.NewLine;

            sb.AppendLine(type.Name);

            if (obj is IEnumerable enumerable)
            {
                sb.Append(GetICollectionPrintingValue(enumerable, nestingLevel));
                return sb.ToString();
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(
                    HasSpecialPrintingValue(propertyInfo.GetValue(obj), propertyInfo.PropertyType, propertyInfo.Name, out var specialPrintingValue)
                    ? identation + specialPrintingValue
                    : $"{identation}{propertyInfo.Name} = {PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)}");
            }

            foreach (var fieldInfo in type.GetFields())
            {
                if (fieldInfo.Name == "Empty") continue;
                sb.Append(
                    HasSpecialPrintingValue(fieldInfo.GetValue(obj), fieldInfo.FieldType, fieldInfo.Name, out var specialPrintingValue)
                        ? identation + specialPrintingValue
                        : $"{identation}{fieldInfo.Name} = {PrintToString(fieldInfo.GetValue(obj), nestingLevel + 1)}");
            }

            return sb.ToString();
        }

        private string GetICollectionPrintingValue(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);

            var index = 0;

            foreach (var obj in collection)
            {
                sb.Append($"{identation}{index}: {PrintToString(obj, nestingLevel)}");
                index++;
            }

            return sb.ToString();
        }
    }
}