using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks.Sources;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly Dictionary<Type, Delegate> customTypeSerializers;
        private readonly Dictionary<PropertyInfo, Delegate> customPropertySerializers;
        private readonly Dictionary<Type, CultureInfo> culturesForTypes;
        private readonly Dictionary<PropertyInfo, int> trimmedProperties;

        private HashSet<object> printedObjects;

        internal Dictionary<Type, Delegate> CustomTypeSerializers => customTypeSerializers;
        internal Dictionary<PropertyInfo, Delegate> CustomPropertySerializers => customPropertySerializers;
        internal Dictionary<Type, CultureInfo> CulturesForTypes => culturesForTypes;
        internal Dictionary<PropertyInfo, int> TrimmedProperties => trimmedProperties;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            customTypeSerializers = new Dictionary<Type, Delegate>();
            customPropertySerializers = new Dictionary<PropertyInfo, Delegate>();
            culturesForTypes = new Dictionary<Type, CultureInfo>();
            trimmedProperties = new Dictionary<PropertyInfo, int>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> SetPrintingFor<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> SetPrintingFor<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)expression.Member);
        }

        public PrintingConfig<TOwner> ExcludeProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            excludedProperties.Add((PropertyInfo)expression.Member);
            return this;
        }

        public PrintingConfig<TOwner> ExcludePropertyType<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        private static bool IsFinal(Type type)
        {
            return type.IsValueType || type == typeof(string);
        }

        public string PrintToString(TOwner obj)
        {
            printedObjects = new HashSet<object>();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (printedObjects.Contains(obj))
                return "Cycle reference detected";
            printedObjects.Add(obj);
            
            if (obj == null)
                return "null" + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);

            if (CulturesForTypes.TryGetValue(obj.GetType(), out var culture))
                return identation + ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;

            if (IsFinal(obj.GetType()))
                return obj + Environment.NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;

                string printingResult;

                if (customTypeSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
                {
                    printingResult = (string)typeSerializer.DynamicInvoke(propertyInfo.GetValue(obj));
                }
                else if (customPropertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
                {
                    printingResult = (string)propertySerializer.DynamicInvoke(propertyInfo.GetValue(obj));
                }
                else
                    printingResult = PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1);

                if (trimmedProperties.TryGetValue(propertyInfo, out var length))
                    printingResult = printingResult.Length > length
                        ? printingResult.Substring(0, length) + Environment.NewLine
                        : printingResult;

                sb.Append(identation + propertyInfo.Name + " = " + printingResult);
            }

            return sb.ToString();
        }
    }
}