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
        private readonly List<string> errors = new List<string>();
        private readonly List<MemberInfo> excludedMembers = new List<MemberInfo>();
        private readonly List<Type> excludedTypes = new List<Type>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(char)
        };

        private readonly Dictionary<MemberInfo, Func<object, string>> serializationMemberMap =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> serializationTypeMap =
            new Dictionary<Type, Func<object, string>>();

        private int maxDepth = 10;

        public PrintingConfig<TOwner> AddFinalType(Type type)
        {
            finalTypes.Add(type);
            return this;
        }

        public PrintingConfig<TOwner> RemoveFinalType(Type type)
        {
            finalTypes.Remove(type);
            return this;
        }

        public PrintingConfig<TOwner> SetMaxDepth(int newMaxDepth)
        {
            maxDepth = newMaxDepth;
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            var body = expression.Body;
            if (body is MemberExpression memberExpression)
            {
                excludedMembers.Add(memberExpression.Member);
                return this;
            }

            errors.Add(
                $"For method Exclude was incorrect expression: {expression}. This configuration was not applied.");
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Serializing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, serializationTypeMap);
        }

        public IMemberPrintingConfig<TOwner, TPropType> Serializing<TPropType>(
            Expression<Func<TOwner, TPropType>> expression)
        {
            var body = expression.Body;
            if (body is MemberExpression memberExpression)
                return new MemberPrintingConfig<TOwner, TPropType>(serializationMemberMap, memberExpression.Member,
                    this);

            errors.Add(
                $"For method Serializing was incorrect expression: {expression}. This configuration was not applied");
            return new FakePrintingConfig<TOwner, TPropType>(this);
        }

        public string PrintToString(TOwner objectToPrint)
        {
            if (errors.Any())
                return string.Join(Environment.NewLine, errors) +
                       Environment.NewLine +
                       PrintToString(objectToPrint, 0, typeof(TOwner));
            return PrintToString(objectToPrint, 0, typeof(TOwner));
        }

        private static string GetTypeName(Type type)
        {
            var typeName = type.Name;
            if (!type.IsGenericType)
                return typeName;
            var deleteIndex = typeName.IndexOf('`');
            return typeName.Substring(0, deleteIndex);
        }

        private string PrintToString(object objectToPrint, int nestingLevel, MemberInfo member)
        {
            if (objectToPrint == null)
                return "null" + Environment.NewLine;

            if (serializationMemberMap.TryGetValue(member, out var methodForMember))
                return methodForMember(objectToPrint) + Environment.NewLine;

            if (serializationTypeMap.TryGetValue(objectToPrint.GetType(), out var method))
                return method(objectToPrint) + Environment.NewLine;

            if (finalTypes.Contains(objectToPrint.GetType()))
                return objectToPrint + Environment.NewLine;

            if (nestingLevel >= maxDepth)
                return $"Maximum nesting level ({nestingLevel}) reached" + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            if (objectToPrint is IEnumerable enumerable) return PrintCollection(nestingLevel, enumerable, indentation);

            var result = new StringBuilder();
            var type = objectToPrint.GetType();
            result.AppendLine(GetTypeName(type));

            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            
            foreach (var propertyInfo in type.GetProperties(bindingFlags))
            {
                var propertyType = propertyInfo.PropertyType;
                if (excludedTypes.Contains(propertyType) || excludedMembers.Contains(propertyInfo))
                    continue;

                var value = propertyInfo.GetValue(objectToPrint);
                var nestedPrint = PrintToString(value, nestingLevel + 1, propertyInfo);
                result.Append(indentation + propertyInfo.Name + " = " + nestedPrint);
            }

            foreach (var fieldInfo in type.GetFields(bindingFlags))
            {
                var propertyType = fieldInfo.FieldType;
                if (excludedTypes.Contains(propertyType) || excludedMembers.Contains(fieldInfo))
                    continue;

                var value = fieldInfo.GetValue(objectToPrint);
                var nestedPrint = PrintToString(value, nestingLevel + 1, fieldInfo);
                result.Append(indentation + fieldInfo.Name + " = " + nestedPrint);
            }

            return result.ToString();
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