using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<object> printedObjects = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        internal readonly Dictionary<MemberInfo, Delegate> MemberSerializationRule = new Dictionary<MemberInfo, Delegate>();
        internal readonly Dictionary<Type, Delegate> TypeSerializationRule = new Dictionary<Type, Delegate>();
        private readonly Type[] finalTypes = {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

        private readonly char leveler = '\t';

        public PrintingConfig()
        {

        }

        private PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes.AddRange(parent.excludedTypes);
            excludedMembers.AddRange(parent.excludedMembers);
            MemberSerializationRule.AddRange(parent.MemberSerializationRule);
            TypeSerializationRule.AddRange(parent.TypeSerializationRule);
        }
            
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var configClone = new PrintingConfig<TOwner>(this);
            return new PropertyPrintingConfig<TOwner, TPropType>(configClone);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var configClone = new PrintingConfig<TOwner>(this);
            var memberInfo = GetMemberInfo(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(configClone, memberInfo);
        }

        private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
            (memberSelector.Body as MemberExpression)?.Member;

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var configClone = new PrintingConfig<TOwner>(this);
            var memberInfo = GetMemberInfo(memberSelector);
            configClone.excludedMembers.Add(memberInfo);
            return configClone;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var configClone = new PrintingConfig<TOwner>(this);
            configClone.excludedTypes.Add(typeof(TPropType));
            return configClone;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null || printedObjects.Contains(obj))
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string(leveler, nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable enumerable)
            {
                sb.AppendLine("[");
                foreach (var el in enumerable)
                {
                    sb.Append(identation + PrintToString(el, nestingLevel + 1));
                }
                printedObjects.Add(enumerable);
                sb.AppendLine("]");
            }
            else
            {
                foreach (var propertyInfo in type.GetProperties().Where(AreNotExcludedProperty))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(GetValue(obj, propertyInfo), nestingLevel + 1));
                    printedObjects.Add(obj);
                }
            }
            return sb.ToString();
        }

        private bool AreNotExcludedProperty(PropertyInfo propertyInfo) =>
            !excludedTypes.Contains(propertyInfo.PropertyType) &&
            !excludedMembers.Contains(propertyInfo);

        private object GetValue(object obj, PropertyInfo propertyInfo)
        {
            var val = propertyInfo.GetValue(obj);
            return MemberSerializationRule.ContainsKey(propertyInfo) ?
                MemberSerializationRule[propertyInfo].DynamicInvoke(val) :
                TypeSerializationRule.ContainsKey(propertyInfo.PropertyType) ?
                TypeSerializationRule[propertyInfo.PropertyType].DynamicInvoke(val) :
                val;
        }
    }
}