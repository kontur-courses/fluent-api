using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

        private readonly Dictionary<Type, Delegate> typeSerializers = new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, Delegate> propertiesSerializers = new Dictionary<string, Delegate>();

        private readonly Dictionary<string, int> trimedProperties = new Dictionary<string, int>();

        internal void AddTypeSerializer<TPropType>(Func<TPropType, string> print) => typeSerializers[typeof(TPropType)] = print;

        internal void AddPropertySerializer<TPropType>(Func<TPropType, string> print, string propertyName) => propertiesSerializers[propertyName] = print;

        internal void AddCulture<TPropType>(CultureInfo culture) => culturesForTypes[typeof(TPropType)] = culture;

        internal void AddPropertyTrim(string propertyName, int length) => trimedProperties[propertyName] = length;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
            => new PropertyPrintingConfig<TOwner, TPropType>(this);

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

        private bool IsPropertyIncluded(string name, Type type)
        {
            if (excudedTypes.Contains(type))
                return false;
            if (excudedProperties.Contains(name))
                return false;
            return true;
        }

        private bool ShouldPropertyAcceptSerializationSettings(string name, Type type)
        {
            return typeSerializers.ContainsKey(type) ||
                   propertiesSerializers.ContainsKey(name) ||
                   culturesForTypes.ContainsKey(type) ||
                   trimedProperties.ContainsKey(name);
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(ICollection collection, int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            sb.AppendLine(indent + "{");

            foreach (var item in collection)
                sb.Append(indent + PrintToString(item, nestingLevel));

            sb.AppendLine(indent + "}");
            return sb.ToString();
        }

        private string PrintToString(string name, Type type, object value)
        {
            if (typeSerializers.TryGetValue(type, out var typeSerializer))
                return typeSerializer
                    .DynamicInvoke(value)
                    .ToString();

            if (culturesForTypes.TryGetValue(type, out var culture))
                return ((IFormattable)value).ToString("N", culture);

            if (propertiesSerializers.TryGetValue(name, out var propertySerializer))
                return propertySerializer
                    .DynamicInvoke(value)
                    .ToString();

            if (trimedProperties.TryGetValue(name, out var maxlength))
            {
                var str = (value as string);
                return str.Length <= maxlength ? str : str.Substring(0, maxlength);
            }

            throw new ArgumentException();
        }

        private string PrintToString(IEnumerable<MemberInfo> props, object owner, int nestingLevel)
        {
            var sb = new StringBuilder();
            foreach (var memberInfo in props)
            {
                var name = memberInfo.Name;

                if (!memberInfo.TryGetValue(owner, out var value))
                    continue;

                var type = value.GetType();

                if (!IsPropertyIncluded(name, type))
                    continue;

                var propertyValue = ShouldPropertyAcceptSerializationSettings(name, type)
                    ? PrintToString(name, type, value) + Environment.NewLine
                    : PrintToString(value, nestingLevel + 1);

                sb.Append(new string('\t', nestingLevel + 1) + memberInfo.Name + " = " +
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
                return type.Name + Environment.NewLine + PrintToString(collection, nestingLevel + 1);

            return type.Name + Environment.NewLine + PrintToString(type.GetMembers(), obj, nestingLevel);
        }
    }
}