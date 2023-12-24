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
        private static readonly HashSet<Type> FinalTypes = new HashSet<Type>()
        {
            typeof(int), 
            typeof(double), 
            typeof(float), 
            typeof(string),
            typeof(DateTime), 
            typeof(TimeSpan),
        };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedProperty = new HashSet<MemberInfo>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null";
            
            if (FinalTypes.Contains(obj.GetType()))
                return obj.ToString();

            var identation = new string(" ");
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperty.Contains(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Exclude<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> func)
        {
            var memberInfo = func.Body as MemberExpression;
            if (memberInfo == null)
                throw new ArgumentException();
            excludedProperty.Add(memberInfo.Member);
            return this;
        }
    }
}