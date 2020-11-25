using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        protected internal readonly Dictionary<Type, CultureInfo> CulturesForProperties =
            new Dictionary<Type, CultureInfo>();

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

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckExpressionType(memberSelector.Body.NodeType);
            excludedProperties.Add(GetSelectedPropertyName(memberSelector));
            return this;
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

            if (nestingLevel > 8)
                return "Cycling reference error" + Environment.NewLine;

            var result = SerializeSimpleObject(obj) ?? SerializeCollection(obj as ICollection, nestingLevel);
            if (result != null) return result;
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
                sb.Append(SerializeProperty(obj, propertyInfo, nestingLevel));
            return sb.ToString();
        }

        private string SerializeCollection(ICollection collection, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            if (collection == null) return null;
            var sb = new StringBuilder(collection.GetType().Name + Environment.NewLine);
            foreach (var item in collection)
                sb.Append(indentation + PrintToString(item, nestingLevel + 1));
            return sb.ToString();
        }

        private string SerializeSimpleObject(object obj)
        {
            switch (obj)
            {
                case {} when TypeConverters.ContainsKey(obj.GetType()):
                    return TypeConverters[obj.GetType()].Invoke(obj) + Environment.NewLine;
                case IFormattable formattable
                    when CulturesForProperties.TryGetValue(formattable.GetType(), out var cultureInfo):
                    return formattable.ToString(null, cultureInfo) + Environment.NewLine;
                case IFormattable formattable:
                    return formattable.ToString(null, CultureInfo.InvariantCulture) + Environment.NewLine;
                case string s:
                    return s.ToString(CulturesForProperties.ContainsKey(typeof(string))
                        ? CulturesForProperties[typeof(string)]
                        : CultureInfo.InvariantCulture) + Environment.NewLine;
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

        private static string GetSelectedPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckExpressionType(memberSelector.Body.NodeType);
            var propertySelection = memberSelector.Body.ToString();
            return propertySelection.Substring(propertySelection.LastIndexOf('.') + 1);
        }

        private static void CheckExpressionType(ExpressionType type)
        {
            if (type != ExpressionType.MemberAccess)
                throw new ArgumentException("Function should chose property of object");
            if (!Enum.IsDefined(typeof(ExpressionType), type))
                throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(ExpressionType));
        }
    }
}