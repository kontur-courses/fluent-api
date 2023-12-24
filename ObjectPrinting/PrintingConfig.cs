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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperites = new HashSet<PropertyInfo>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<int> serializedPropertiesMetadataToken = new HashSet<int>();

        internal readonly Dictionary<Type, Func<object, string>> typeSerializers =
            new Dictionary<Type, Func<object, string>>();

        internal readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers =
            new Dictionary<PropertyInfo, Func<object, string>>();

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            excludedProperites.Add(propInfo);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string SerializeIEnumerable(IEnumerable enumerable)
        {
            var sb = new StringBuilder();
            var openingBracket = enumerable is IDictionary ? "{ " : "[ ";
            var closingBracket = enumerable is IDictionary ? "}" : "]";
            sb.Append(openingBracket);
            foreach (var element in enumerable)
            {
                sb.Append(element);
                sb.Append(" ");
            }

            sb.Append(closingBracket);
            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
            {
                if (typeSerializers.TryGetValue(objType, out var serializer))
                    return serializer(obj) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            if (obj is IEnumerable enumerable)
                return SerializeIEnumerable(enumerable) + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objType.Name);
            foreach (var propertyInfo in objType.GetProperties())
            {
                if (Excluded(propertyInfo) || serializedPropertiesMetadataToken.Contains(propertyInfo.MetadataToken))
                    continue;

                serializedPropertiesMetadataToken.Add(propertyInfo.MetadataToken);
                if (propertySerializers.TryGetValue(propertyInfo, out var serializer))
                {
                    sb.Append(identation + propertyInfo.Name + " = " + serializer(propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool Excluded(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperites.Contains(propertyInfo);
        }
    }
}