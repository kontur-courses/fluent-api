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
    public class PrintingConfig<TOwner> : BasePrintingConfig<TOwner>
    {
        public PrintingConfig() { }
        
        public PrintingConfig(BasePrintingConfig<TOwner> printingConfig) : base(printingConfig) { }

        public PrintingConfig(BasePrintingConfig<TOwner> printingConfig, 
            Dictionary<MemberInfo, object> serializedProperties) : base(printingConfig)
            => SerializedProperties = serializedProperties;

        public PrintingConfig(BasePrintingConfig<TOwner> printingConfig, 
            Dictionary<Type, object> serializedTypes) : base(printingConfig)
            => SerializedTypes = serializedTypes;

        public PrintingConfig(BasePrintingConfig<TOwner> printingConfig, List<Type> excludedTypes) : base(printingConfig)
            => ExcludedTypes = excludedTypes;

        public PrintingConfig(BasePrintingConfig<TOwner> printingConfig, List<string> excludedProperties) : base(printingConfig)
            => ExcludedProperties = excludedProperties;

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            var copyExcludedTypes = new List<Type>(ExcludedTypes);
            
            if (!copyExcludedTypes.Contains(typeof(TProperty)))
                copyExcludedTypes.Add(typeof(TProperty));

            return new PrintingConfig<TOwner>(this, copyExcludedTypes);
        }
        
        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var copyExcludedProperties = new List<string>(ExcludedProperties);
            var memberExpression = (MemberExpression)expr.Body;
            var propertyName = memberExpression.Member.Name;
            
            if (!copyExcludedProperties.Contains(propertyName))
                copyExcludedProperties.Add(propertyName);
            
            return new PrintingConfig<TOwner>(this, copyExcludedProperties);
        }
        
        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            var copySerializedTypes = new Dictionary<Type, object>(SerializedTypes);
            
            return new PropertyPrintingConfig<TOwner, TProperty>(this, null, copySerializedTypes, SerializedProperties);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var copySerializedProperties = new Dictionary<MemberInfo, object>(SerializedProperties);
            var memberExpression = (MemberExpression)expr.Body;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberExpression.Member, SerializedTypes, copySerializedProperties);
        }

        public string PrintToString(TOwner obj, int maxNestingLevel = -1)
        {
            if (maxNestingLevel > 0)
                MaxNestingLevel = maxNestingLevel;
            return Print(obj, null,0);
        }

        private string Print(object obj, Member prevMember, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (FinalTypes.Contains(type))
                return Convert.ToString(obj) + Environment.NewLine;
            
            var indentation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(type.Name);

            if (obj is ICollection collection)
                return PrintCollection(collection, nestingLevel, stringBuilder, indentation);

            foreach (var memberInfo in type.GetProperties().Cast<MemberInfo>().Concat(type.GetFields()))
            {
                var member = new Member()
                {
                    Name = memberInfo.Name, Type = (Type)memberInfo.GetMemberType(),
                    Value = memberInfo.GetMemberValue(obj)
                };

                if (prevMember != null && prevMember.Equals(member))
                {
                    stringBuilder.Append(indentation + member.Name + " = " + member.Type.Name + "..." +
                                         Environment.NewLine);
                    continue;
                }

                if (ExcludedProperties.Contains(member.Name) || ExcludedTypes.Contains(member.Type))
                    continue;

                var nextString = CheckAndConvertPropertyToString(memberInfo, member);

                if (!string.IsNullOrEmpty(nextString))
                {
                    stringBuilder.AppendLine(indentation + nextString);
                    continue;
                }

                if (nestingLevel != MaxNestingLevel)
                {
                    stringBuilder.Append(indentation + member.Name + " = " +
                                         Print(member.Value, member, nestingLevel + 1));
                    continue;
                }
                break;
            }

            return stringBuilder.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel, StringBuilder stringBuilder, string indentation)
        {
            foreach (var item in collection)
            {
                var itemType = item.GetType();
                var name = itemType.Name;
                stringBuilder.Append(indentation + name + " = " + Print(item, null,nestingLevel + 1));
            }
            return stringBuilder.ToString();
        }

        private string CheckAndConvertPropertyToString(MemberInfo memberInfo, Member member)
        {
            if (SerializedProperties.ContainsKey(memberInfo))
            {
                if (SerializedProperties[memberInfo] is Delegate func)
                    return (string)InvokeDelegate(func, member.Value);
            }
            
            if (SerializedTypes.ContainsKey(member.Type))
            {
                if (SerializedTypes[member.Type] is CultureInfo cultureInfo)
                {
                    return member.Name + " = " + Convert.ToString(member.Value, cultureInfo);
                }
                    
                if (SerializedTypes[member.Type] is Delegate func)
                    return (string)InvokeDelegate(func, member.Value);
            }

            return null;
        }

        private object InvokeDelegate(Delegate del, object value)
        {
            return value != null ? del.GetInvocationList().Aggregate(value, (current, func) => func.DynamicInvoke(current)) : null;
        }
    }
}