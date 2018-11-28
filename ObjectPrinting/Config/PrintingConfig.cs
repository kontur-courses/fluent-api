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

        private readonly Dictionary<FieldInfo, Func<object, string>> printingOverridedFields;
        private readonly HashSet<FieldInfo> fieldsToExclude;

        public PrintingConfig()
        {
            typesToExclude = new HashSet<System.Type>();
            printingOverridedTypes = new Dictionary<System.Type, Func<object, string>>();
            cultureOverridedTypes = new Dictionary<System.Type, CultureInfo>();

            printingOverridedProperties = new Dictionary<PropertyInfo, Func<object, string>>();
            propertiesToExclude = new HashSet<PropertyInfo>();

            printingOverridedFields = new Dictionary<FieldInfo, Func<object, string>>();
            fieldsToExclude = new HashSet<FieldInfo>();
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
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                if (cultureOverridedTypes.TryGetValue(type, out var cultureInfo))
                    return PrintWithCulture(obj, cultureInfo);

                return obj.ToString();
            }
            
            var sb = new StringBuilder();
            sb.Append(type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (typesToExclude.Contains(property.PropertyType) || propertiesToExclude.Contains(property))
                    continue;

                var propertyString = PropertyToString(property, obj, nestingLevel);
                sb.Append(FormatMember(property.Name, propertyString, nestingLevel));
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (typesToExclude.Contains(field.FieldType) || fieldsToExclude.Contains(field))
                    continue;

                var fieldString = FieldToString(field, obj, nestingLevel);
                sb.Append(FormatMember(field.Name, fieldString, nestingLevel));
            }

            return sb.ToString();
        }

        private string PropertyToString(PropertyInfo property, object container, int nestingLevel)
        {
            var type = property.PropertyType;
            var value = property.GetValue(container);

            if (printingOverridedProperties.ContainsKey(property))
                return printingOverridedProperties[property](value);

            if (printingOverridedTypes.ContainsKey(type))
                return printingOverridedTypes[type](value);

            return PrintToString(value, nestingLevel + 1);
        }

        private string FieldToString(FieldInfo field, object container, int nestingLevel)
        {
            var type = field.FieldType;
            var value = field.GetValue(container);

            if (printingOverridedFields.ContainsKey(field))
                return printingOverridedFields[field](value);

            if (printingOverridedTypes.ContainsKey(type))
                return printingOverridedTypes[type](value);

            return PrintToString(value, nestingLevel + 1);
        }

        #region static helpers

        private static string FormatMember(string memberName, string memeberValue, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            return Environment.NewLine + identation + memberName + " = " + memeberValue;
        }

        private static string PrintWithCulture(object obj, CultureInfo cultureInfo)
        {
            var toStringMethod = obj.GetType().GetMethod("ToString", new[] { typeof(CultureInfo) });
            return toStringMethod?.Invoke(obj, new object[] { cultureInfo }).ToString();
        }

        private static PropertyInfo GetProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
        }

        #endregion
    }
}