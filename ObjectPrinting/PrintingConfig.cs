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
        private static readonly HashSet<Type> _finalTypes = new()
        {
            typeof(int), typeof(uint), typeof(double), typeof(float), typeof(decimal),
            typeof(long), typeof(ulong), typeof(short), typeof(ushort),
            typeof(string), typeof(bool),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<MemberInfo> excludedProperties = new();
        private readonly HashSet<Type> excludedTypes = new();
        protected readonly Dictionary<MemberInfo, int> propertiesMaxLength = new();
        protected readonly Dictionary<MemberInfo, Func<object, string>> propertiesSerialization = new();
        protected readonly Dictionary<Type, CultureInfo> typesCulture = new();
        protected readonly Dictionary<Type, Func<object, string>> typesSerialization = new();
        private Func<dynamic> recursivePropertiesSerialization;

        public PrintingConfig()
        {
        }

        protected PrintingConfig(PrintingConfig<TOwner> config)
        {
            typesCulture = config.typesCulture;
            propertiesMaxLength = config.propertiesMaxLength;
            propertiesSerialization = config.propertiesSerialization;
            typesSerialization = config.typesSerialization;
        }

        public string PrintToString(TOwner obj)
        {
            return SerializeObject(obj, ImmutableList<object>.Empty);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> member)
        {
            var memberInfo = GetMember(member);

            excludedProperties.Add(memberInfo);

            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> printMember)
        {
            var memberInfo = GetMember(printMember);

            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> WithCyclicLinkException()
        {
            recursivePropertiesSerialization = () => throw new ArgumentException("recursive property");

            return this;
        }

        public PrintingConfig<TOwner> WithCyclicLinkMessage(Func<string> printProperty)
        {
            recursivePropertiesSerialization = printProperty;

            return this;
        }

        public PrintingConfig<TOwner> WithCyclicLinkIgnored()
        {
            recursivePropertiesSerialization = () => string.Empty;

            return this;
        }

        private string SerializeObject(object obj, IImmutableList<object> previousObjects)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            if (previousObjects.Any(prev => prev == obj))
            {
                return recursivePropertiesSerialization != null 
                    ? (string)recursivePropertiesSerialization() : "Recursive property";
            }

            if (obj is IEnumerable && obj is not string)
                return SerializeEnumerable(obj, previousObjects);

            if (_finalTypes.Contains(type))
                return SerializeFinalType(obj);

            var indentation = new string('\t', previousObjects.Count + 1);
            var objectSerialization = new StringBuilder().AppendLine(type.Name);
            previousObjects = previousObjects.Add(obj);
            var members = Array.Empty<MemberInfo>().Concat(type.GetProperties()).Concat(type.GetFields());
            foreach (var memberInfo in members)
            {
                if (excludedProperties.Contains(memberInfo)
                    || excludedTypes.Contains(memberInfo.GetMemberType()))
                    continue;

                objectSerialization.Append(indentation);
                objectSerialization.Append(memberInfo.Name);
                objectSerialization.Append(" = ");
                objectSerialization.Append(SerializeMember(memberInfo, obj, previousObjects));
                objectSerialization.Append(Environment.NewLine);
            }

            return objectSerialization.ToString();
        }

        private string SerializeFinalType(object obj)
        {
            var type = obj.GetType();
            if (typesSerialization.TryGetValue(type, out var serialization))
                return serialization.Invoke(obj);

            var result = obj.ToString();
            if (typesCulture.ContainsKey(obj.GetType()))
                result = ((IFormattable)obj).ToString(null, typesCulture[type]);

            return result;
        }

        private string SerializeMember(MemberInfo memberInfo, object obj, IImmutableList<object> previousObjects)
        {
            var value = memberInfo.GetMemberValue(obj);
            var result = propertiesSerialization.TryGetValue(memberInfo, out var serialization)
                ? serialization.Invoke(value)
                : SerializeObject(value, previousObjects).TrimEnd();

            var propertyMaxLength = propertiesMaxLength.TryGetValue(memberInfo, out var length)
                ? length
                : result.Length;

            result = result[..propertyMaxLength];

            return result;
        }

        private string SerializeEnumerable(object obj, IImmutableList<object> previousObjects)
        {
            var enumerable = (IEnumerable)obj;
            var serializedEnumerable = new StringBuilder();
            var indentation = previousObjects.Count == 0
                ? string.Empty
                : Environment.NewLine + new string('\t', previousObjects.Count + 1);

            foreach (var item in enumerable)
            {
                serializedEnumerable.Append(indentation);
                serializedEnumerable.Append(SerializeObject(item, previousObjects));
            }

            return serializedEnumerable.ToString();
        }

        private static MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            var memberBody = (MemberExpression)expression.Body;
            var memberInfo = memberBody.Member;

            return memberInfo;
        }
    }
}