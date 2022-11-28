using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IMemberConfigurator<TOwner>
    {
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedMembers.Contains(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        public IBasicConfigurator<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public IBasicConfigurator<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
        {
            var memberExpression = (MemberExpression) expression.Body;
            var member = memberExpression.Member;
            excludedMembers.Add(member);
            return this;
        }

        public IMemberConfigurator<TOwner> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression)
        {
            var memberExpression = (MemberExpression) expression.Body;
            var member = memberExpression.Member;
            excludedMembers.Add(member);
            return this;
        }

        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
    }
}