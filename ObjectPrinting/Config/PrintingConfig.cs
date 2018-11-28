using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Config.Property;
using ObjectPrinting.Config.Type;

namespace ObjectPrinting.Config
{
    public class PrintingConfig<TOwner>
    {
        private readonly System.Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<System.Type> typesToExclude;
        private readonly Dictionary<System.Type, Func<object, string>> printingOverridedTypes;
        private readonly Dictionary<System.Type, CultureInfo> cultureOverridedTypes;
        private readonly Dictionary<PropertyInfo, Func<object, string>> printingOverridedProperties;
        private readonly HashSet<PropertyInfo> propertiesToExclude;

        public PrintingConfig()
        {
            typesToExclude = new HashSet<System.Type>();
            printingOverridedTypes = new Dictionary<System.Type, Func<object, string>>();
            cultureOverridedTypes = new Dictionary<System.Type, CultureInfo>();
            printingOverridedProperties = new Dictionary<PropertyInfo, Func<object, string>>();
            propertiesToExclude = new HashSet<PropertyInfo>();
        }

        public void OverrideTypePrinting<TPropType>(Func<TPropType, string> print)
        {
            printingOverridedTypes[typeof(TPropType)] = obj => print((TPropType) obj);
        }

        public void OverrideTypeCulture<TPropType>(CultureInfo culture)
        {
            cultureOverridedTypes[typeof(TPropType)] = culture;
        }

        public void OverridePropertyPrinting(PropertyInfo propertyInfo, Func<object, string> print)
        {
            printingOverridedProperties[propertyInfo] = print;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));

            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetProperty(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            propertiesToExclude.Add(GetProperty(memberSelector));

            return this;
        }


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, null, 0);
        }

        private string PrintToString(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();

            if (propertyInfo != null && printingOverridedProperties.ContainsKey(propertyInfo))
                return printingOverridedProperties[propertyInfo](obj);

            if (nestingLevel != 0 && printingOverridedTypes.ContainsKey(type))
                return printingOverridedTypes[type](obj);

            if (finalTypes.Contains(type))
            {
                if (cultureOverridedTypes.TryGetValue(type, out var cultureInfo))
                    return PrintWithCulture(obj, cultureInfo);

                return obj.ToString();
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.Append(type.Name);

            foreach (var property in type.GetProperties())
            {
                if (typesToExclude.Contains(property.PropertyType) || propertiesToExclude.Contains(property))
                    continue;

                var propertyString = PrintToString(property.GetValue(obj), property, nestingLevel + 1);
                sb.Append(Environment.NewLine + identation + property.Name + " = " + propertyString);
            }

            return sb.ToString();
        }

        private static string PrintWithCulture(object obj, CultureInfo cultureInfo)
        {
            var toStringMethod = obj.GetType().GetMethod("ToString", new[] {typeof(CultureInfo)});
            return toStringMethod?.Invoke(obj, new object[] {cultureInfo}).ToString();
        }

        private static PropertyInfo GetProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
        }
    }
}