using System;
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
        private readonly List<MemberInfo> exludedProperties = new List<MemberInfo>();
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly Dictionary<object, int> complexObjectLinks = new Dictionary<object, int>();
        private int maxRecursion = 2;

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Dictionary<Type, Func<object, string>> typeSerializesInfos =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> membersSerializesInfos =
            new Dictionary<MemberInfo, Func<object, string>>();


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> WithMaxRecursion(int maxRecursion)
        {
            if (maxRecursion < 1)
                throw new ArgumentException();
            this.maxRecursion = maxRecursion;

            return this;
        }

        public PrintingConfig<TOwner> Exclude<TResult>(Expression<Func<TOwner, TResult>> exclude)
        {
            var memberInfo = GetMemberInfo(exclude);

            exludedProperties.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> SetCulture<T>(CultureInfo culture)
            where T : IFormattable
        {
            return SerializeWith<T>(
                p => p.ToString(null, culture));
        }

        public PrintingConfig<TOwner> SerializeWith<T>(Func<T, string> serialize)
        {
            Func<object, string> func = p => serialize((T)p);

            typeSerializesInfos[typeof(T)] = func;

            return this;
        }

        public PrintingConfig<TOwner> SerializeWith<T>(
            Expression<Func<TOwner, T>> property,
            Func<T, string> serialize)
        {
            var memberInfo = GetMemberInfo(property);
            Func<object, string> func = p => serialize((T)p);

            membersSerializesInfos[memberInfo] = func;

            return this;
        }

        private MemberInfo GetMemberInfo<T>(Expression<Func<TOwner, T>> expression)
        {
            var memberExpression = expression.Body is UnaryExpression unaryExpression
                ? (MemberExpression)unaryExpression.Operand
                : (MemberExpression)expression.Body;

            return memberExpression.Member;
        }

        public PrintingConfig<TOwner> Trim(Expression<Func<TOwner, string>> property, int length)
        {
            Func<string, string> func = value => value.Length <= length
                ? value
                : value.Substring(0, length);

            return SerializeWith(property, func);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));

            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (!complexObjectLinks.ContainsKey(obj))
                complexObjectLinks[obj] = 0;
            complexObjectLinks[obj]++;

            if (complexObjectLinks[obj] == maxRecursion)
                return "Maximum recursion has been reached" + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (exludedProperties.Any(m => m.Name == propertyInfo.Name) ||
                    excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (membersSerializesInfos.TryGetValue(propertyInfo, out var serializeMember))
                {
                    var t = serializeMember(propertyInfo.GetValue(obj));

                    sb.Append(identation + propertyInfo.Name + " = " + t + Environment.NewLine);
                    continue;
                }

                if (typeSerializesInfos.TryGetValue(propertyInfo.PropertyType, out var serializeType))
                {
                    var t = serializeType(propertyInfo.GetValue(obj));

                    sb.Append(identation + propertyInfo.Name + " = " + t + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}