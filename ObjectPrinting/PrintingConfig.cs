using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<object, object> serializes = new Dictionary<object, object>();
        private readonly List<object> excludedObjects = new List<object>();

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            if (!excludedObjects.Contains(typeof(TProperty)))
                excludedObjects.Add(typeof(TProperty));
            
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            var propertyName = memberExpression.Member.Name;
            
            if (!excludedObjects.Contains(propertyName))
                excludedObjects.Add(propertyName);
            
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

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (nestingLevel == 5)
                return "TOO DEEP" + Environment.NewLine;
            
            var type = obj.GetType();
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(type.Name);

            foreach (var memberInfo in type.GetProperties().Cast<MemberInfo>().Concat(type.GetFields()))
            {
                var memberName = memberInfo.Name;
                var memberType = memberInfo.GetMemberType();
                var memberValue = memberInfo.GetMemberValue(obj);

                if (excludedObjects.Contains(memberName) || excludedObjects.Contains(memberType))
                    continue;

                if (serializes.ContainsKey(memberName))
                {
                    if (serializes[memberName] is Delegate func)
                        stringBuilder.AppendLine(identation + InvokeDelegate(func, memberValue));
                    continue;
                }

                if (serializes.ContainsKey(memberType))
                {
                    if (serializes[memberType] is CultureInfo cultureInfo)
                    {
                        var nextString = PrintToString(Convert.ToString(memberValue, cultureInfo), nestingLevel + 1);
                        stringBuilder.Append(identation + memberName + " = " + nextString);
                    }
                    
                    if (serializes[memberType] is Delegate func)
                        stringBuilder.AppendLine(identation + InvokeDelegate(func, memberValue));
                    
                    continue;
                }
                
                stringBuilder.Append(identation + memberName + " = " + PrintToString(memberValue, nestingLevel + 1));
            }
            
            return stringBuilder.ToString();
        }

        private object InvokeDelegate(Delegate del, object value)
        {
            return del.GetInvocationList().Aggregate(value, (current, func) => func.DynamicInvoke(current));
        }
    }
}