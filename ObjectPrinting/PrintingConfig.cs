using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<object, object> serializes = new Dictionary<object, object>();
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<string> excludedProperties = new List<string>();

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private int MaxNestingLevel { get; set; } = 10;

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            if (!excludedTypes.Contains(typeof(TProperty)))
                excludedTypes.Add(typeof(TProperty));
            
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            var propertyName = memberExpression.Member.Name;
            
            if (!excludedProperties.Contains(propertyName))
                excludedProperties.Add(propertyName);
            
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this, null, serializes);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberExpression.Member, serializes);
        }

        public string PrintToString(TOwner obj, int maxNestingLevel = -1)
        {
            if (maxNestingLevel > 0)
                MaxNestingLevel = maxNestingLevel;
            return Print(obj, 0);
        }

        private string Print(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (finalTypes.Contains(type))
                return Convert.ToString(obj) + Environment.NewLine;
            
            var indentation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(type.Name);

            foreach (var memberInfo in type.GetProperties().Cast<MemberInfo>().Concat(type.GetFields()))
            {
                var member = Tuple.Create(
                    memberInfo.Name, memberInfo.GetMemberType(), memberInfo.GetMemberValue(obj)
                );

                if (memberInfo.DeclaringType == typeof(TOwner))
                {
                    var nextString = CheckAndConvertPropertyToString(member);

                    if (!string.IsNullOrEmpty(nextString))
                    {
                        stringBuilder.AppendLine(indentation + nextString);
                        continue;
                    }
                }

                if (nestingLevel != MaxNestingLevel)
                    stringBuilder.Append(indentation + member.Item1 + " = " + Print(member.Item3, nestingLevel + 1));
            }

            return stringBuilder.ToString();
        }

        private string CheckAndConvertPropertyToString(Tuple<string, object, object> memberInfo)
        {
            if (excludedProperties.Contains(memberInfo.Item1) || excludedTypes.Contains(memberInfo.Item2))
                return null;
            
            if (serializes.ContainsKey(memberInfo.Item1))
            {
                if (serializes[memberInfo.Item1] is Delegate func)
                    return (string)InvokeDelegate(func, memberInfo.Item3);
            }
            
            if (serializes.ContainsKey(memberInfo.Item2))
            {
                if (serializes[memberInfo.Item2] is CultureInfo cultureInfo)
                {
                    return memberInfo.Item1 + " = " + Convert.ToString(memberInfo.Item3, cultureInfo);
                }
                    
                if (serializes[memberInfo.Item2] is Delegate func)
                    return (string)InvokeDelegate(func, memberInfo.Item3);
            }

            return null;
        }

        private object InvokeDelegate(Delegate del, object value)
        {
            return del.GetInvocationList().Aggregate(value, (current, func) => func.DynamicInvoke(current));
        }
    }
}