using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes =
        [
            typeof(int), typeof(double), typeof(float),
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];

        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<MemberInfo> excludedProperties = [];

        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> propertySerializers = new();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            excludedProperties.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        internal void SetSerializer<TPropType>(Func<TPropType, string> serializer, MemberInfo? memberInfo = null)
        {
            if (memberInfo != null)
                propertySerializers[memberInfo] = value => serializer((TPropType)value);
            else
                typeSerializers[typeof(TPropType)] = value => serializer((TPropType)value);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, []);
        }

        private string PrintToString(object? obj, int nestingLevel, HashSet<object> processedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (FinalTypes.Contains(type))
                return obj + Environment.NewLine;

            if (!type.IsValueType && !processedObjects.Add(obj))
                return "[Circular reference]" + Environment.NewLine;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(type.Name);
                foreach (var propertyInfo in type.GetProperties())
                {
                    if(excludedTypes.Contains(propertyInfo.PropertyType) ||
                       excludedProperties.Contains(propertyInfo))
                        continue;

                    if(propertySerializers.TryGetValue(propertyInfo, out var serializer) ||
                       typeSerializers.TryGetValue(propertyInfo.PropertyType, out serializer))
                    {
                        sb.AppendLine(propertyInfo.FormatLine(
                            serializer(propertyInfo.GetValue(obj)),
                            nestingLevel));
                        continue;
                    }

                    sb.Append(propertyInfo.FormatLine(
                        PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, processedObjects),
                        nestingLevel));
                }
                return sb.ToString();
            }
            finally
            {
                if (!type.IsValueType)
                    processedObjects.Remove(obj);
            }
        }
    }
}