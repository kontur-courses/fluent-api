using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : ISerializationConfig<TOwner>
    {
        private readonly HashSet<Type> typesIgnored = new HashSet<Type>();
        private readonly HashSet<string> propertiesIgnored = new HashSet<string>();
        private readonly Dictionary<Type, Func<object, string>> typesSerialization =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<TOwner, string>> propertiesSerialization =
            new Dictionary<string, Func<TOwner, string>>();

        private readonly Dictionary<Type, CultureInfo> typesCulture =
            new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<string, int> propertiesNameTrim =
            new Dictionary<string, int>();

        private static readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private bool isIgnored(PropertyInfo property)
        {
            return typesIgnored.Contains(property.PropertyType) ||
                   propertiesIgnored.Contains(property.Name);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExp = memberSelector.Body as MemberExpression;
            var propertyName = "";
            try
            {
                propertyName = memberExp.Member.Name;
            }
            catch (NullReferenceException e)
            {
                throw new NullReferenceException("Can't get the member name", e);
            }
            if (propertyName != "")
                propertiesIgnored.Add(propertyName);

            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesIgnored.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        void ISerializationConfig<TOwner>.SetTypeSerialization<TPropType>
            (Func<TPropType, string> serializer)
        {
            var type = typeof(TPropType);
            if (!typesSerialization.ContainsKey(type))
                typesSerialization.Add(type, obj => serializer((TPropType)obj));
            else
                typesSerialization[type] = obj => serializer((TPropType)obj);
        }

        void ISerializationConfig<TOwner>.SetPropertySerialization
            (MemberInfo memberInfo, Func<TOwner, string> serializer)
        {
            var propertyName = memberInfo.Name;
            if (!propertiesSerialization.ContainsKey(propertyName))
                propertiesSerialization.Add(propertyName, serializer);
            else
                propertiesSerialization[propertyName] = serializer;
        }

        void ISerializationConfig<TOwner>.SetCulture<TPropType>(CultureInfo culture)
        {
            var type = typeof(TPropType);
            if (!typesCulture.ContainsKey(type))
                typesCulture.Add(type, culture);
            else
                typesCulture[type] = culture;
        }

        void ISerializationConfig<TOwner>.SetTrim(MemberInfo memberInfo, int length)
        {
            var propertyName = memberInfo.Name;
            if (!propertiesNameTrim.ContainsKey(propertyName))
                propertiesNameTrim.Add(propertyName, length);
            else
                propertiesNameTrim[propertyName] = length;
        }
    }
}