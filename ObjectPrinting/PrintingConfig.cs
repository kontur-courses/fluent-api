using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly int depth;

        private readonly Dictionary<Type, CultureInfo> formattedTypes = new();
        private readonly Dictionary<MemberInfo, CultureInfo> formattedProperties = new();

        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedProperties = new();

        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> propertySerializers = new();

        private readonly Dictionary<Type, List<Func<string, string>>> additionalSerializationTypes = new();
        private readonly Dictionary<MemberInfo, List<Func<string, string>>> additionalSerializationProperties = new();


        public PrintingConfig(int depth = 10)
        {
            this.depth = depth;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = (MemberExpression)memberSelector.Body;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, member.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = (MemberExpression)memberSelector.Body;
            excludedProperties.Add(member.Member);
            return this;
        }


        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, ImmutableHashSet<object>.Empty);
        }

        internal void SetCulture(Type type, CultureInfo culture)
        {
            formattedTypes[type] = culture;
        }

        internal void SetCulture(MemberInfo member, CultureInfo culture)
        {
            formattedProperties[member] = culture;
        }

        internal void SetSerializer(Type type, Func<object, string> serializer)
        {
            typeSerializers[type] = serializer;
        }

        internal void SetSerializer(MemberInfo member, Func<object, string> serializer)
        {
            propertySerializers[member] = serializer;
        }

        internal void AddSerializer(MemberInfo member, Func<string, string> serializer)
        {
            if (!additionalSerializationProperties.ContainsKey(member))
                additionalSerializationProperties[member] = new List<Func<string, string>>();

            additionalSerializationProperties[member].Add(serializer);
        }

        internal void AddSerializer(Type type, Func<string, string> serializer)
        {
            if (!additionalSerializationTypes.ContainsKey(type))
                additionalSerializationTypes[type] = new List<Func<string, string>>();

            additionalSerializationTypes[type].Add(serializer);
        }


        private string PrintToString(object obj, int nestingLevel, ImmutableHashSet<object> parents,
            MemberInfo member = null)
        {
            if (nestingLevel > depth)
                throw new OverflowException("object nesting depth exceeded");

            if (obj == null)
                return "null";
            var type = obj.GetType();

            if (obj is IDictionary dict)
                return GetStringDict(dict, nestingLevel);

            if (obj is IEnumerable enumerable and not string)
                return GetStringEnumerable(enumerable);

            if (parents.Contains(obj))
                return obj.ToString();

            if ((member is not null && propertySerializers.ContainsKey(member)) || typeSerializers.ContainsKey(type))
                return GetSerializeObj(obj, member, type);

            if (obj is IFormattable formattedObj)
                return GetFormattedString(formattedObj, member);

            if (obj is string str)
                return GetString(str, member, type);

            var indent = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;

                var newObj = propertyInfo.GetValue(obj);
                sb
                    .Append(indent)
                    .Append(propertyInfo.Name)
                    .Append(" = ")
                    .Append(PrintToString(newObj,
                        nestingLevel + 1,
                        parents.Add(obj),
                        propertyInfo))
                    .AppendLine();
            }

            return sb.ToString();
        }

        private string GetStringDict(IDictionary dict, int nestingLevel)
        {
            var sb = new StringBuilder().AppendLine("{");
            var indent = new string('\t', nestingLevel + 1);
            foreach (var key in dict.Keys)
                sb.Append($"{indent}{key}: ")
                    .Append(PrintToString(dict[key],
                        0,
                        ImmutableHashSet<object>.Empty).Trim())
                    .Append(',')
                    .AppendLine()
                    .AppendLine();

            sb.Append(new string('\t', nestingLevel)).Append('}');
            return sb.ToString();
        }

        private string GetStringEnumerable(IEnumerable enumerable)
        {
            var sb = new StringBuilder().Append('[');

            var elems = new List<string>();
            foreach (var e in enumerable)
                elems.Add($"{PrintToString(e, 0, ImmutableHashSet<object>.Empty)}");

            return sb.Append(string.Join(", ", elems)).Append(']').ToString();
        }

        private string GetFormattedString(IFormattable formattedObj, MemberInfo member)
        {
            var culture = CultureInfo.CurrentCulture;
            if (formattedTypes.ContainsKey(formattedObj.GetType()))
                culture = formattedTypes[formattedObj.GetType()];
            if (member != null && formattedProperties.ContainsKey(member))
                culture = formattedProperties[member];
            return formattedObj.ToString(null, culture);
        }

        private string GetString(string str, MemberInfo member, Type type)
        {
            if (additionalSerializationTypes.ContainsKey(type))
                str = additionalSerializationTypes[type]
                    .Aggregate(str, (current, serializer) => serializer(current));

            if (member != null && additionalSerializationProperties.ContainsKey(member))
                str = additionalSerializationProperties[member]
                    .Aggregate(str, (current, serializer) => serializer(current));

            return str;
        }

        private string GetSerializeObj(object obj, MemberInfo member, Type type)
        {
            var serializedStr = member != null && propertySerializers.ContainsKey(member)
                ? propertySerializers[member](obj)
                : typeSerializers[type](obj);

            if (additionalSerializationTypes.ContainsKey(type))
                serializedStr = additionalSerializationTypes[type]
                    .Aggregate(serializedStr, (current, serializer) => serializer(current));

            if (member != null && additionalSerializationProperties.ContainsKey(member))
                serializedStr = additionalSerializationProperties[member]
                    .Aggregate(serializedStr, (current, serializer) => serializer(current));
            return serializedStr;
        }
    }
}