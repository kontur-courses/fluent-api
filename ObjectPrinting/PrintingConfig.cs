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
        private readonly IEnumerable<Type> baseTypes = new List<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<string, int> cutMemberInfo = new Dictionary<string, int>();

        private readonly List<string> excludingMemberInfo = new List<string>();
        private readonly List<Type> excludingTypes = new List<Type>();

        private readonly Dictionary<Type, CultureInfo> specialTypeSerializationCulture =
            new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<Type, Delegate> specialTypeSerializationFunction = new Dictionary<Type, Delegate>();

        public MemberInfoPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new MemberInfoPrintingConfig<TOwner, TPropType>(this);
        }

        public MemberInfoPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberName = GetMemberName(memberSelector);
            return new MemberInfoPrintingConfig<TOwner, TPropType>(this, memberName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberName = GetMemberName(memberSelector);
            excludingMemberInfo.Add(memberName);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new Stack<object>());
        }


        internal void AddTypeSerialization(Type type, Delegate function)
        {
            specialTypeSerializationFunction[type] = function;
        }

        internal void AddTypeSerializationCulture(Type type, CultureInfo culture)
        {
            specialTypeSerializationCulture[type] = culture;
        }

        internal void AddCutMemberInfo(string memberInfoName, int length)
        {
            cutMemberInfo[memberInfoName] = length;
        }

        private string GetMemberName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = memberSelector.Body as MemberExpression;
            var memberName = memberExpression?.Member.Name;
            return memberName;
        }

        private string PrintToString(object obj, int nestingLevel, Stack<object> visited)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (baseTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var objectType = obj.GetType();
            var stringBuilder = new StringBuilder(objectType.Name + Environment.NewLine);

            if (obj is IEnumerable collection) return PrintIEnumerable(collection, nestingLevel, visited);

            visited.Push(obj);
            
            MemberInfo[] propertyInfos = objectType.GetProperties();
            MemberInfo[] fieldInfos = objectType.GetFields();
            var memberInfos = propertyInfos.Concat(fieldInfos);
            
            foreach (var memberInfo in memberInfos)
            {
                var memberInfoName = memberInfo.Name;
                if (TryPrintValueByMemberName(obj, nestingLevel, visited, memberInfo, stringBuilder,
                    out var innerInformation))
                    stringBuilder.Append(indentation + memberInfoName + " = " + innerInformation);

            }

            visited.Pop();
            return stringBuilder.ToString();
        }

        private bool TryPrintValueByMemberName(object obj, int nestingLevel, Stack<object> visited,
            MemberInfo memberInfo,
            StringBuilder stringBuilder, out string innerInformation)
        {
            var (memberInfoType, memberInfoValue) = GetMemberInfoTypeAndValue(memberInfo, obj);
            innerInformation = "";
            var indentation = new string('\t', nestingLevel + 1);

            if (excludingTypes.Contains(memberInfoType)) return false;
            if (excludingMemberInfo.Contains(memberInfo.Name)) return false;
            
            if (visited.Contains(memberInfoValue))
            {
                stringBuilder.Append($"{indentation}{memberInfo.Name} = {memberInfoValue.GetType().Name}{Environment.NewLine}");
                return false;
            }

            innerInformation = PrintToString(FormatValue(memberInfoType, memberInfoValue, memberInfo.Name),
                nestingLevel + 1, visited);
            return true;
        }

        private string PrintIEnumerable(IEnumerable collection, int nestingLvl, Stack<object> visited)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            var values = new List<string>();

            foreach (var item in collection)
            {
                if (visited.Contains(item))
                    values.Add(item.GetType().Name);
                else
                    values.Add(PrintToString(item, nestingLvl, visited)
                        .Trim(Environment.NewLine.ToCharArray()));
            }

            sb.Append(string.Join(", ", values))
                .Append("]")
                .Append(Environment.NewLine);
            return sb.ToString();
        }

        private (Type, object) GetMemberInfoTypeAndValue(MemberInfo memberInfo, object obj)
        {
            Type type;
            object value;

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    value = ((FieldInfo) memberInfo).GetValue(obj);
                    type = ((FieldInfo) memberInfo).FieldType;
                    return (type, value);
                case MemberTypes.Property:
                    value = ((PropertyInfo) memberInfo).GetValue(obj);
                    type = ((PropertyInfo) memberInfo).PropertyType;
                    return (type, value);
                default:
                    throw new Exception("cant match memberInfo");
            }
        }

        private object FormatValue(Type type, object value, string name)
        {
            if (specialTypeSerializationFunction.TryGetValue(type, out var serializeFunction))
                return serializeFunction?.DynamicInvoke(value);

            if (specialTypeSerializationCulture.TryGetValue(type, out var serializeCultureInfo))
                return ((IFormattable) value).ToString("g", serializeCultureInfo);

            if (cutMemberInfo.TryGetValue(name, out var maxLen))
            {
                var len = value.ToString().Length > maxLen ? maxLen : value.ToString().Length;
                return value.ToString().Substring(0, len);
            }

            return value;
        }
    }
}