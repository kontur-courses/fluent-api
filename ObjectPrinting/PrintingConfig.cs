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
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> finalTypes = new HashSet<Type>()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> excudedTypes = new HashSet<Type>();
        private readonly HashSet<string> excudedProperties = new HashSet<string>();
        private readonly HashSet<object> printedFields = new HashSet<object>();

        private readonly Dictionary<Type, CultureInfo> culturesForTypes = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<Type, Delegate> serializedTypes = new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, Delegate> serializedProperties = new Dictionary<string, Delegate>();

        private readonly Dictionary<string, int> trimmedProperties = new Dictionary<string, int>();

        internal void AddTypeSerialize<TPropType>(Func<TPropType, string> print) => serializedTypes[typeof(TPropType)] = print;

        internal void AddPropertySerialize<TPropType>(Func<TPropType, string> print, string propertyName) => serializedProperties[propertyName] = print;

        internal void AddCulture<TPropType>(CultureInfo culture) => culturesForTypes[typeof(TPropType)] = culture;

        internal void AddPropertyTrimm(string propertyName, int lenght) => trimmedProperties[propertyName] = lenght;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() => new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
            => new PropertyPrintingConfig<TOwner, TPropType>(this, (memberSelector.Body as MemberExpression).Member.Name);

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excudedProperties.Add((memberSelector.Body as MemberExpression).Member.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excudedTypes.Add(typeof(TPropType));
            return this;
        }

        private bool IsPropertyShouldExclude(PropertyInfo propertyInfo)
        {
            if (excudedTypes.Contains(propertyInfo.PropertyType))
                return true;
            if (excudedProperties.Contains(propertyInfo.Name))
                return true;
            return false;
        }

        private bool IsPropertyShouldModified(PropertyInfo propertyInfo)
        {
            return serializedTypes.ContainsKey(propertyInfo.PropertyType) ||
                   serializedProperties.ContainsKey(propertyInfo.Name) ||
                   culturesForTypes.ContainsKey(propertyInfo.PropertyType) ||
                   trimmedProperties.ContainsKey(propertyInfo.Name);
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(ICollection collection)
        {
            var sb = new StringBuilder();
            sb.Append(" {");

            foreach (var item in collection)
                sb.Append(" " + item);

            sb.AppendLine(" }");
            return sb.ToString();
        }

        private string PrintToString(PropertyInfo propertyInfo, object owner)
        {
            var propertyValue = propertyInfo.GetValue(owner);

            if (serializedTypes.ContainsKey(propertyInfo.PropertyType))
                return serializedTypes[propertyInfo.PropertyType]
                    .DynamicInvoke(propertyValue)
                    .ToString();

            if (culturesForTypes.ContainsKey(propertyInfo.PropertyType))
                return ((IFormattable)propertyInfo.GetValue(owner)).ToString("c", culturesForTypes[propertyInfo.PropertyType]);

            if (serializedProperties.ContainsKey(propertyInfo.Name))
                return serializedProperties[propertyInfo.Name]
                    .DynamicInvoke(propertyValue)
                    .ToString();

            if (trimmedProperties.ContainsKey(propertyInfo.Name))
            {
                var str = (propertyValue as string);
                var maxLenght = trimmedProperties[propertyInfo.Name];
                return str.Length <= maxLenght ? str : str.Substring(0, maxLenght);
            }

            throw new ArgumentException();
        }

        private string PrintToString(PropertyInfo[] props, object owner, int nestingLevel)
        {
            var sb = new StringBuilder();
            foreach (var propertyInfo in props)
            {
                if (printedFields.Contains(propertyInfo))
                    continue;
                printedFields.Add(propertyInfo);

                if (IsPropertyShouldExclude(propertyInfo))
                    continue;

                var propertyValue = IsPropertyShouldModified(propertyInfo)
                    ? PrintToString(propertyInfo, owner) + Environment.NewLine
                    : PrintToString(propertyInfo.GetValue(owner), nestingLevel + 1);

                sb.Append(new string('\t', nestingLevel + 1) + propertyInfo.Name + " = " +
                          propertyValue);
            }
            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var type = obj.GetType();

            if (printedFields.Contains(obj))
                return type.Name + "(...)" + Environment.NewLine;
            printedFields.Add(obj);

            if (obj is ICollection collection)
                return type.Name + PrintToString(collection);

            return type.Name + Environment.NewLine + PrintToString(type.GetProperties(), obj, nestingLevel);
        }
    }
}