using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> membersToExclude = [];
        private readonly Dictionary<Type, Func<object, string>> formatters = [];
        private readonly Dictionary<MemberInfo, Func<object, string>> formattersByMember = [];
        
        public PrintingConfig<TOwner> Excluding<TPropertyType>()
        {
            var propertyType = typeof(TPropertyType);

            foreach (var member in typeof(TOwner).GetPrintableMembers())
            {
                var memberType = member is PropertyInfo property
                    ? property.PropertyType
                    : ((FieldInfo)member).FieldType;

                if (memberType == propertyType)
                    membersToExclude.Add(member);
            }

            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TPropertyType>(Expression<Func<TOwner, TPropertyType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression
                || memberExpression.Expression != memberSelector.Parameters[0]
                || memberExpression.Member is not (PropertyInfo or FieldInfo))
            {
                throw new ArgumentException("Выражение должно указывать на свойство или поле.");
            }
            
            membersToExclude.Add(memberExpression.Member);
            
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropertyType>(this);
        }
        
        public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>(Expression<Func<TOwner, TPropertyType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression
                || memberExpression.Expression != memberSelector.Parameters[0]
                || memberExpression.Member is not (PropertyInfo or FieldInfo))
            {
                throw new ArgumentException("Выражение должно указывать на свойство или поле.");
            }
            
            return new PropertyPrintingConfig<TOwner, TPropertyType>(this, memberExpression.Member);
        }

        internal void RegisterFormatter<T>(Func<T, string> print)
        {
            formatters[typeof(T)] = value => print((T)value);
        }
        
        internal void RegisterFormatterByMember<T>(MemberInfo member, Func<T, string> print)
        {
            formattersByMember[member] = value => print((T)value);
        }
        
        public string PrintToString(TOwner obj)
        {
            var currentPath = new HashSet<object>(ReferenceEqualityComparer.Instance);
            return PrintToString(obj, 0, currentPath);
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> currentPath)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();

            if (formatters.TryGetValue(type, out var formatter))
                return formatter(obj) + Environment.NewLine;
            
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            
            if (!currentPath.Add(obj))
                return "<cycle>" + Environment.NewLine;

            try
            {
                var indentation = new string('\t', nestingLevel + 1);
                var sb = new StringBuilder();

                sb.AppendLine(type.Name);

                foreach (var member in type.GetPrintableMembers())
                {
                    if (membersToExclude.Contains(member))
                        continue;

                    var value = member is PropertyInfo property
                        ? property.GetValue(obj)
                        : ((FieldInfo)member).GetValue(obj);

                    var printedValue =
                        value is not null && formattersByMember.TryGetValue(member, out var memberFormatter)
                            ? memberFormatter(value) + Environment.NewLine
                            : PrintToString(value, nestingLevel + 1, currentPath);

                    sb.Append(indentation)
                        .Append(member.Name)
                        .Append(" = ")
                        .Append(printedValue);
                }

                return sb.ToString();
            }
            finally
            {
                currentPath.Remove(obj);
            }
        }
        
        
    }
}