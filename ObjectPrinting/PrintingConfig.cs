using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        internal readonly Dictionary<Type, CultureInfo> CulturesForProperties = new Dictionary<Type, CultureInfo>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        protected internal readonly Dictionary<string, Func<object, string>> PropertyConverters =
            new Dictionary<string, Func<object, string>>();

        protected internal readonly Dictionary<Type, Func<object, string>> TypeConverters =
            new Dictionary<Type, Func<object, string>>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckExpressionType(memberSelector.Body.NodeType);
            return new PropertyPrintingConfig<TOwner, TPropType>(GetSelectedPropertyName(memberSelector), this);
        }

        private string GetSelectedPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckExpressionType(memberSelector.Body.NodeType);
            var choice = memberSelector.Body.ToString();
            return choice.Substring(choice.LastIndexOf('.') + 1);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckExpressionType(memberSelector.Body.NodeType);
            excludedProperties.Add(GetSelectedPropertyName(memberSelector));
            return this;
        }

        private void CheckExpressionType(ExpressionType type)
        {
            if (type != ExpressionType.MemberAccess)
                throw new ArgumentException("Function should chose property of object");
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var result = PrintToString(obj, 0);
            return result.EndsWith(Environment.NewLine)
                ? result.Substring(0, result.Length - Environment.NewLine.Length)
                : result;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null) return "null" + Environment.NewLine;

            if (nestingLevel > 256)
                throw new InternalBufferOverflowException("nesting level should be less than 256");

            var result = SerializeSimpleObject(obj);
            if (result != null) return result;
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
                sb.Append(SerializeProperty(obj, propertyInfo, nestingLevel));
            return sb.ToString();
        }

        private string SerializeSimpleObject(object obj)
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                dynamic buff = obj;
                if (TypeConverters.ContainsKey(obj.GetType()))
                    return TypeConverters[obj.GetType()].Invoke(obj) + Environment.NewLine;
                if (CulturesForProperties.TryGetValue(obj.GetType(), out var cultureInfo))
                    return buff.ToString(cultureInfo) + Environment.NewLine;
                return buff.ToString() + Environment.NewLine;
            }

            if (TypeConverters.ContainsKey(obj.GetType()))
                return TypeConverters[obj.GetType()].Invoke(obj) + Environment.NewLine;
            return null;
        }

        private string SerializeProperty(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            if (obj.GetType() == typeof(TOwner))
                if (excludedProperties.Contains(propertyInfo.Name))
                {
                    return string.Empty;
                }
                else if (PropertyConverters.ContainsKey(propertyInfo.Name))
                {
                    var value = PropertyConverters[propertyInfo.Name].Invoke(propertyInfo.GetValue(obj));
                    return indentation + propertyInfo.Name + " = " + value + Environment.NewLine;
                }

            if (excludedTypes.Contains(propertyInfo.PropertyType)) return string.Empty;
            if (TypeConverters.ContainsKey(propertyInfo.PropertyType))
                return indentation + propertyInfo.Name + " = " +
                       TypeConverters[propertyInfo.PropertyType].Invoke(propertyInfo.GetValue(obj)) +
                       Environment.NewLine;

            return indentation + propertyInfo.Name + " = " +
                   PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }
    }
}