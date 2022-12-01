using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;
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

        private readonly char leveler = '\t';
        
        public PrintingConfig(PrintingConfig<TOwner> parent = null)
        {
            if (parent == null)
                return;

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

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var configClone = new PrintingConfig<TOwner>(this);
            configClone.excludedTypes.Add(typeof(TPropType));
            return configClone;
        }

        public string PrintToString(object obj, int nestingLevel = 0)
        {
            if (printedObjects.Contains(obj))
                return "recursive reference" + Environment.NewLine;

            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj.GetType().IsFinal())
                return obj + Environment.NewLine;

            return PrintDetailedInformation(obj, nestingLevel);
        }

        private string PrintDetailedInformation(object obj, int nestingLevel)
        {
            var identation = new string(leveler, nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable enumerable)
            {
                sb.AppendLine("[");
                foreach (var el in enumerable)
                {
                    var value = PrintToString(el, nestingLevel + 1);
                    sb.Append($"{identation}{value}");
                }
                printedObjects.Add(enumerable);
                sb.AppendLine("]");
            }
            else
            {
                foreach (var memberInfo in type.GetMembers().Where(m =>
                             (m is PropertyInfo || m is FieldInfo) && AreNotExcludedProperty(m)))
                {
                    var value = PrintToString(GetValue(obj, memberInfo), nestingLevel + 1);
                    sb.Append($"{identation}{memberInfo.Name} = {value}");
                    printedObjects.Add(obj);
                }
            }
            return sb.ToString();
        }

        private bool AreNotExcludedProperty(MemberInfo memberInfo) =>
            !excludedTypes.Contains(memberInfo.GetUnderlyingType()) &&
            !excludedMembers.Contains(memberInfo);

        private object GetValue(object obj, MemberInfo memberInfo)
        {
            var val = memberInfo.GetValue(obj);
            var memberType = memberInfo.GetUnderlyingType();
            return MemberSerializationRule.ContainsKey(memberInfo) 
                ? MemberSerializationRule[memberInfo].DynamicInvoke(val) 
                : TypeSerializationRule.ContainsKey(memberType) 
                    ? TypeSerializationRule[memberType].DynamicInvoke(val) 
                    : val;
        }
    }
}