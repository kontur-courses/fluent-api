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
        private readonly HashSet<Type> excludeMembersByType;
        private readonly HashSet<string> excludeMembersByName;
        private readonly Dictionary<Type, Delegate> alternativeSerializationByType;
        private readonly Dictionary<string, Delegate> alternativeSerializationByName;
        private readonly Dictionary<string, Delegate> trimmingFunctions;
        private readonly Dictionary<Type, CultureInfo> cultureInfoForNumbers;
        private readonly HashSet<Type> finalTypes;

        public PrintingConfig()
        {
            excludeMembersByType = new HashSet<Type>();
            alternativeSerializationByType = new Dictionary<Type, Delegate>();
            alternativeSerializationByName = new Dictionary<string, Delegate>();
            trimmingFunctions = new Dictionary<string, Delegate>();
            excludeMembersByName = new HashSet<string>();
            cultureInfoForNumbers = new Dictionary<Type, CultureInfo>();
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),typeof(long),typeof(decimal),typeof(char),
                typeof(bool),typeof(DateTime), typeof(TimeSpan), typeof(byte), typeof(sbyte),typeof(short),typeof(ushort),
                typeof(uint),typeof(ulong)
            };
        }
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            var excludedType = typeof(TPropType);
            excludeMembersByType.Add(excludedType);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> excludedExpression)
        {
            var excludedName = ((MemberExpression)excludedExpression.Body).Member.Name;
            excludeMembersByName.Add(excludedName);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }


        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>(Expression<Func<TOwner, TPropType>> alternativeExpression)
        {
            var memberName = ((MemberExpression)alternativeExpression.Body).Member.Name;
            return new TypePrintingConfig<TOwner, TPropType>(this, memberName);
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> viewedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return (cultureInfoForNumbers.ContainsKey(type) ?
                    ((IFormattable)obj).ToString("", cultureInfoForNumbers[type]) : obj) + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var members = FilteringMembers(type);
            var sb = new StringBuilder();
            if (obj is IEnumerable collection)
            {
                sb.AppendLine(obj.GetType().Name + "\r\n" + indentation + "\t" + '{');
                foreach (var item in collection)
                    sb.Append(indentation + "\t" + PrintToString(item, nestingLevel + 2, new HashSet<object>()));
                sb.AppendLine(indentation + "\t" + '}');
                return sb.ToString();
            }
            var result = PrintMembers(obj, nestingLevel, indentation, members, viewedObjects);

            return result;
        }

        private MemberInfo[] FilteringMembers(Type type)
        {
            return type.GetMembers()
                .Where(member => (member.MemberType & MemberTypes.Property) != 0 ||
                                                               (member.MemberType & MemberTypes.Field) != 0)
                .Where(member => !(excludeMembersByType
                                       .Contains(member is PropertyInfo info ? info.PropertyType : ((FieldInfo)member).FieldType)
                                                  || excludeMembersByName.Contains(member.Name))).ToArray();
        }

        private string PrintMembers(object obj, int nestingLevel, string indentation, MemberInfo[] members, HashSet<object> viewedObjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            foreach (var memberInfo in members)
            {
                var isPropertyInfo = memberInfo is PropertyInfo;
                var memberType = isPropertyInfo ? ((PropertyInfo)memberInfo).PropertyType : ((FieldInfo)memberInfo).FieldType;
                var value = isPropertyInfo ? ((PropertyInfo)memberInfo).GetValue(obj) : ((FieldInfo)memberInfo).GetValue(obj);
                if (viewedObjects.Contains(value))
                    continue;
                viewedObjects.Add(value);
                if (value is string str && trimmingFunctions.ContainsKey(memberInfo.Name))
                    value = trimmingFunctions[memberInfo.Name].DynamicInvoke(str);
                var result = alternativeSerializationByType.ContainsKey(memberType)
                    ? alternativeSerializationByType[memberType].DynamicInvoke(value)
                    : alternativeSerializationByName.ContainsKey(memberInfo.Name)
                        ? alternativeSerializationByName[memberInfo.Name].DynamicInvoke(value)
                        : null;
                if (result != null)
                {
                    sb.Append(indentation + result + "\r\n");
                    continue;
                }
                sb.Append(indentation + memberInfo.Name + " = " +
                              PrintToString(value, nestingLevel + 1, viewedObjects));
            }

            return sb.ToString();
        }

        internal void AddAlternativeSerialization(Type type, Delegate altSerialize)
        {
            if (alternativeSerializationByType.ContainsKey(type))
                alternativeSerializationByType[type] = altSerialize;
            alternativeSerializationByType.Add(type, altSerialize);
        }

        internal void AddAlternativeSerialization(string name, Delegate altSerialize)
        {
            if (alternativeSerializationByName.ContainsKey(name))
                alternativeSerializationByName[name] = altSerialize;
            alternativeSerializationByName.Add(name, altSerialize);
        }

        internal void AddTrimmingFunctions(string name, Delegate trimmingFunction)
        {
            if (trimmingFunctions.ContainsKey(name))
                trimmingFunctions[name] = trimmingFunction;
            trimmingFunctions.Add(name, trimmingFunction);
        }

        internal void AddCultureInfoForNumbers(Type type, CultureInfo altSerialize)
        {
            if (cultureInfoForNumbers.ContainsKey(type))
                cultureInfoForNumbers[type] = altSerialize;
            cultureInfoForNumbers.Add(type, altSerialize);
        }
    }
}