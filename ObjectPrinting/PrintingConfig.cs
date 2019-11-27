using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        private readonly IDictionary<Type, CultureInfo> cultureLookup;
        private readonly ISet<MemberInfo> excludedProperties;
        private readonly ISet<Type> excludedTypes;
        private readonly IDictionary<MemberInfo, Delegate> propertyPrinters;
        private readonly IDictionary<Type, Delegate> typePrinters;

        internal PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<MemberInfo>();
            typePrinters = new Dictionary<Type, Delegate>();
            propertyPrinters = new Dictionary<MemberInfo, Delegate>();
            cultureLookup = new Dictionary<Type, CultureInfo>();
        }

        IDictionary<Type, Delegate> IPrintingConfig.TypePrinters => typePrinters;
        IDictionary<MemberInfo, Delegate> IPrintingConfig.PropertyPrinters => propertyPrinters;
        IDictionary<Type, CultureInfo> IPrintingConfig.CultureLookup => cultureLookup;

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            excludedProperties.Add(memberInfo ?? throw new ArgumentException("Property expected"));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, new HashSet<object>());
        }

        private string PrintToString(object obj, ISet<object> printed, int nestingLevel = 0)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (printed.Contains(obj))
                return type.Name + " ↰" + Environment.NewLine;

            if (typePrinters.TryGetValue(type, out var print))
                return print.DynamicInvoke(obj) + Environment.NewLine;

            if (!FinalTypes.Contains(type))
                return PrintObject(obj, nestingLevel, printed);

            if (cultureLookup.TryGetValue(type, out var cultureInfo))
                return Convert.ToString(obj, cultureInfo) + Environment.NewLine;

            return obj + Environment.NewLine;
        }

        private string PrintObject(object obj, int nestingLevel, ISet<object> printed)
        {
            if (obj.GetElements().Any(IsPropertyIncluded))
                printed.Add(obj);

            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            PrintElements(obj, printed, nestingLevel, sb);
            return sb.ToString();
        }

        private void PrintElements(object obj, ISet<object> printed, int nestingLevel, StringBuilder sb)
        {
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var elementInfo in obj.GetElements().Where(IsPropertyIncluded))
            {
                sb.Append(indentation).Append(elementInfo.MemberInfo.Name).Append(" = ");
                var value = elementInfo.GetValue(obj);
                if (propertyPrinters.TryGetValue(elementInfo.MemberInfo, out var printingFunction))
                    sb.Append(printingFunction.DynamicInvoke(value));
                else
                    sb.Append(PrintToString(value, printed, nestingLevel + 1));
            }
        }

        private bool IsPropertyIncluded(ElementInfo element)
        {
            return !excludedTypes.Contains(element.MemberInfo.DeclaringType) &&
                   !excludedProperties.Contains(element.MemberInfo);
        }
    }

    internal interface IPrintingConfig
    {
        IDictionary<Type, Delegate> TypePrinters { get; }
        IDictionary<MemberInfo, Delegate> PropertyPrinters { get; }
        IDictionary<Type, CultureInfo> CultureLookup { get; }
    }
}