using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.PrintingConfiguration
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> excludedTypes;
        private HashSet<MemberInfo> excludedProperties;
        private Dictionary<Type, Delegate> customTypesSerialization;
        private Dictionary<MemberInfo, Delegate> customPropertiesSerialization;
        private Dictionary<MemberInfo, int> stringMaxLengths;
        private Dictionary<Type, CultureInfo> differentCultures;
        private MemberInfo currentProperty;
        private HashSet<object> history;

        private Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(char), typeof(long)
        };

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<MemberInfo>();
            customPropertiesSerialization = new Dictionary<MemberInfo, Delegate>();
            customTypesSerialization = new Dictionary<Type, Delegate>();
            stringMaxLengths = new Dictionary<MemberInfo, int>();
            differentCultures = new Dictionary<Type, CultureInfo>();
            history = new HashSet<object>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string PrintEnumerable(IEnumerable objects)
        {
            return PrintEnumerable(objects, 0);
        }

        public string PrintDictionary(IDictionary objects)
        {
            return PrintDictionary(objects, 0);
        }

        public TypePrintingConfig<TOwner, TType> For<TType>()
        {
            return new TypePrintingConfig<TOwner, TType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberInfo> For<TMemberInfo>(
            Expression<Func<TOwner, TMemberInfo>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            return new MemberPrintingConfig<TOwner, TMemberInfo>(this, member);
        }

        public PrintingConfig<TOwner> Exclude<TMemberInfo>(Expression<Func<TOwner, TMemberInfo>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            excludedProperties.Add(member);
            return this;
        }


        protected internal void ExcludeType(Type type)
        {
            excludedTypes.Add(type);
        }

        protected internal void AddSerializationTypeRule(Type type, Delegate func)
        {
            customTypesSerialization[type] = func;
        }

        protected internal void SetMaxLength(MemberInfo memberInfo, int length)
        {
            stringMaxLengths[memberInfo] = length;
        }

        protected internal void AddSerializationRule<TType>(MemberInfo memberInfo, Func<TType, string> func)
        {
            customPropertiesSerialization[memberInfo] = func;
        }

        protected internal void AddCultureUsing(Type type, CultureInfo invariantCulture)
        {
            differentCultures[type] = invariantCulture;
        }

        private string PrintDictionary(IDictionary objects, int nestingLevel)
        {
            var convertedStrings = new List<string>();
            foreach (DictionaryEntry pair in objects)
            {
                var convertedKey = PrintToString(pair.Key, nestingLevel + 1);
                var convertedValue = PrintToString(pair.Value, nestingLevel + 1);
                convertedStrings.Add(new string('\t', nestingLevel + 1) + $"{convertedKey[..^2]} : {convertedValue}");
            }

            var str = string.Join("", convertedStrings);
            return objects.GetType().Name + '[' + "\n" + str + new string('\t', nestingLevel) + ']' + '\n';
        }

        private string PrintEnumerable(IEnumerable objects, int nestingLevel)
        {
            var convertedStrings = new List<string>();
            foreach (var obj in objects)
            {
                if (excludedTypes.Contains(obj.GetType()))
                {
                    return $"This is an ignored types collection with type {objects.GetType().Name} \n";
                }

                var convertedObject = PrintToString(obj, nestingLevel + 1);
                convertedStrings.Add(new string('\t', nestingLevel + 1) + convertedObject);
            }

            var str = string.Join("", convertedStrings);
            return objects.GetType().Name + '[' + "\n" + str + new string('\t', nestingLevel) + ']' + '\n';
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            var type = obj.GetType();
            if (finalTypes.Contains(type) && !excludedTypes.Contains(type))
            {
                return PrintObjectWithFinalType(obj);
            }

            switch (obj)
            {
                case IDictionary dictionary:
                    return PrintDictionary(dictionary, nestingLevel);
                case IEnumerable enumerable:
                    return PrintEnumerable(enumerable, nestingLevel);
            }

            if (history.Contains(obj))
            {
                return "New cyclic reference detected  \n";
            }

            history.Add(obj);
            var starter = new string('\t', nestingLevel + 1);
            var serializedStringBuilder = new StringBuilder();
            serializedStringBuilder.AppendLine(type.Name);
            var memberInfos = type.GetFields().Cast<MemberInfo>().Concat(type.GetProperties()).ToArray();
            foreach (var memberInfo in memberInfos)
            {
                currentProperty = memberInfo;
                var value = memberInfo.GetValue(obj);
                if (!excludedTypes.Contains(memberInfo.GetMemberType()) && !excludedProperties.Contains(memberInfo))
                {
                    serializedStringBuilder.Append(starter + memberInfo.Name + " = " +
                                                   PrintToString(memberInfo.GetValue(obj), nestingLevel + 1));
                }
            }

            history.Remove(obj);
            currentProperty = null;
            return serializedStringBuilder.ToString();
        }

        private string PrintObjectWithFinalType(object obj)
        {
            var type = obj.GetType();
            if (currentProperty != null && customPropertiesSerialization.ContainsKey(currentProperty))
            {
                var serialization = customPropertiesSerialization[currentProperty];
                return serialization.DynamicInvoke(obj) + Environment.NewLine;
            }

            if (currentProperty != null && stringMaxLengths.ContainsKey(currentProperty))
            {
                var str = obj.ToString();
                var maxLength = stringMaxLengths[currentProperty];
                var result = str != null && str.Length > maxLength
                    ? str[..maxLength]
                    : str;
                return result + Environment.NewLine;
            }

            if (differentCultures.ContainsKey(type) && obj is IFormattable formatObj)
            {
                return formatObj.ToString(null, differentCultures[type]) + Environment.NewLine;
            }

            if (customTypesSerialization.ContainsKey(type))
            {
                var serialization = customTypesSerialization[type];
                return serialization.DynamicInvoke(obj) + Environment.NewLine;
            }

            return obj + Environment.NewLine;
        }
    }
}