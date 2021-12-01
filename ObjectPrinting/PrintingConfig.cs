using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private int maxNesting = 10;
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedProperties = new();
        private readonly HashSet<object> printedObjects = new();

        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

        private readonly Dictionary<Type, Delegate> customTypesDeserializing =
            new();

        private readonly Dictionary<MemberInfo, Delegate> customPropertyDeserializing =
            new();

        private readonly Dictionary<Type, CultureInfo> cultureInfos = new();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberInfo(memberSelector));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            foreach (var memberInfo in GetMemberInfo(memberSelector))
                excludedProperties.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> SetMaxNestingLevel(int level)
        {
            maxNesting = level;

            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(Func<TPropType, string> function)
        {
            customTypesDeserializing[typeof(TPropType)] = function;

            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(IEnumerable<MemberInfo> memberInfos,
            Func<TPropType, string> function)
        {
            foreach (var memberInfo in memberInfos)
                customPropertyDeserializing[memberInfo] = function;

            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(CultureInfo culture)
        {
            cultureInfos[typeof(TPropType)] = culture;

            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > maxNesting)
                return "Reached max level nesting\n";

            if (printedObjects.Contains(obj))
                return "This object was printed already\n";

            if (obj == null)
                return "null\n";

            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();

            var indent = new string('\t', nestingLevel + 1);

            return obj switch
            {
                IDictionary dictionary => ConvertDictionaryToString(dictionary, indent, nestingLevel),
                IEnumerable enumerable => ConvertEnumerableToString(enumerable, indent, nestingLevel),
                _ => ConvertObjectToString(obj, indent, nestingLevel)
            };
        }

        private string ConvertObjectToString(object obj, string indent, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetPropertiesAndFields()
                .Where(prop => !IsExcluded(prop)))
            {
                sb.AppendLine(
                    $"{indent}{propertyInfo.Name} = {ConvertToString(propertyInfo, obj, nestingLevel).Trim()}");

                printedObjects.Add(obj);
            }

            return sb.ToString();
        }

        private string ConvertToString(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var memberType = memberInfo.GetMemberType();
            var memberValue = memberInfo.GetMemberValue(obj);

            if (customTypesDeserializing.ContainsKey(memberType))
                return customTypesDeserializing[memberType]
                    .DynamicInvoke(memberValue)?.ToString();

            if (cultureInfos.ContainsKey(memberType)) return ToStringWithCulture(memberValue);

            if (customPropertyDeserializing.ContainsKey(memberInfo))
                return customPropertyDeserializing[memberInfo].DynamicInvoke(memberValue)?.ToString();

            return PrintToString(memberValue, nestingLevel + 1);
        }

        private string ConvertEnumerableToString(IEnumerable enumerable, string indent, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine("IEnumerable");

            foreach (var element in enumerable)
                sb.AppendLine($"{indent}{PrintToString(element, nestingLevel + 1).Trim()}");

            return sb.ToString();
        }

        private string ConvertDictionaryToString(IDictionary dictionary, string indent, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine("IDictionary");

            foreach (var element in dictionary.Keys)
                sb.AppendLine(
                    $"{indent + PrintToString(element, nestingLevel + 1)} : {PrintToString(dictionary[element], nestingLevel + 1).Trim()}");

            return sb.ToString();
        }

        private string ToStringWithCulture<T>(T obj)
        {
            return Convert.ToString(obj, cultureInfos[obj.GetType()]);
        }

        private static IEnumerable<MemberInfo> GetMemberInfo<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfoList = new List<MemberInfo>();
            if (memberSelector.Body is MemberExpression memberInfo)
                memberInfoList.Add(memberInfo.Member);

            return memberInfoList;
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return excludedTypes.Contains(memberInfo.GetMemberType()) || excludedProperties.Contains(memberInfo);
        }
    }
}