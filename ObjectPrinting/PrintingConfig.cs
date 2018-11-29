using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private static string GetTypeName(Type type)
        {
            var typeName = type.Name;
            if (!type.IsGenericType)
                return typeName;
            var deleteIndex = typeName.IndexOf('`');
            return typeName.Substring(0, deleteIndex);
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

            var indentation = new string('\t', nestingLevel + 1);
            if (obj is IEnumerable enumerable)
            {
                return PrintCollection(nestingLevel, enumerable, indentation);
            }

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(GetTypeName(type));
            
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                if (excludedTypes.Contains(propertyType) || excludedMembers.Contains(propertyInfo))
                    continue;

                var value = propertyInfo.GetValue(obj);
                var nestedPrint = PrintToString(value, nestingLevel + 1, propertyInfo);
                sb.Append(indentation + propertyInfo.Name + " = " + nestedPrint);
            }

            foreach (var fieldInfo in type.GetFields())
            {
                var propertyType = fieldInfo.FieldType;
                if (excludedTypes.Contains(propertyType) || excludedMembers.Contains(fieldInfo))
                    continue;

                var value = fieldInfo.GetValue(obj);
                var nestedPrint = PrintToString(value, nestingLevel + 1, fieldInfo);
                sb.Append(indentation + fieldInfo.Name + " = " + nestedPrint);
            }

            return sb.ToString();
        }

        private string PrintCollection(int nestingLevel, IEnumerable enumerable, string indentation)
        {
            var str = new StringBuilder();
            str.Append(Environment.NewLine);
            foreach (var x1 in enumerable)
                str.Append(indentation + PrintToString(x1, nestingLevel + 1, typeof(TOwner)));

            return str.ToString();
        }
    }
}