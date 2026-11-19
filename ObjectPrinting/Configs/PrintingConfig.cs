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
        public PrintingConfig<TOwner> Excluding<TPropertyType>()
        {
            var propertyType = typeof(TPropertyType);

            foreach (var member in GetPrintableMembers(typeof(TOwner)))
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
            throw  new NotImplementedException();
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var member in GetPrintableMembers(type))
            {
                if (membersToExclude.Contains(member))
                    continue;
                
                var value = member is PropertyInfo property
                    ? property.GetValue(obj)
                    : ((FieldInfo)member).GetValue(obj);
                
                sb.Append(indentation + member.Name + " = " +
                          PrintToString(value, nestingLevel + 1));
            }
            
            return sb.ToString();
        }
        
        private static IEnumerable<MemberInfo> GetPrintableMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member => member is FieldInfo
                                 || member is PropertyInfo property
                                 && property.GetMethod?.IsPublic == true
                                 && property.GetIndexParameters().Length == 0);
        }
    }
}