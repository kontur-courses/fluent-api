using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Configuration;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<MemberInfo> excludedMembers = new List<MemberInfo>();
        private readonly List<Type> excludedTypes = new List<Type>();

        private readonly Dictionary<MemberInfo, Func<object, string>> serializationMemberMap =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> serializationTypeMap =
            new Dictionary<Type, Func<object, string>>();

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> field)
        {
            var expression = field.Body;
            if (expression is MemberExpression memberExpression)
            {
                excludedMembers.Add(memberExpression.Member);
                return this;
            }

            throw new ArgumentException();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Serializing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, serializationTypeMap);
        }

        public MemberPrintingConfig<TOwner, TPropType> Serializing<TPropType>(
            Expression<Func<TOwner, TPropType>> field)
        {
            var expression = field.Body;
            if (expression is MemberExpression memberExpression)
                return new MemberPrintingConfig<TOwner, TPropType>(serializationMemberMap, memberExpression.Member,
                    this);

            throw new ArgumentException();
        }
    

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, typeof(TOwner));
        }


        private string PrintToString(object obj, int nestingLevel, MemberInfo member)
        {
            var maxDepth = 10;
            if (nestingLevel >= maxDepth)
                return "max depth reached";
            
            if (obj == null)
                return "null" + Environment.NewLine;

            if (serializationMemberMap.TryGetValue(member, out var methodForMember))
                return methodForMember(obj) + Environment.NewLine;

            if (serializationTypeMap.TryGetValue(obj.GetType(), out var method))
                return method(obj) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(char)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            
            if (obj is IEnumerable enumerable)
            {
                var str = new StringBuilder();
                foreach (var x1 in enumerable)
                {
                    str.Append(PrintToString(x1, nestingLevel+1, typeof(TOwner)));
                }

                return str.ToString();
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                if (excludedTypes.Contains(propertyType) || excludedMembers.Contains(propertyInfo))
                    continue;

                var value = propertyInfo.GetValue(obj);
                var nestedPrint = PrintToString(value, nestingLevel + 1, propertyInfo);
                sb.Append(indentation + propertyInfo.Name + " = " + nestedPrint);
            }

            return sb.ToString();
        }
    }
}