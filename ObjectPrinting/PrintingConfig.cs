using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> notSerialisedTypeOfProperty = new HashSet<Type>();
        private readonly HashSet<string> notSerialisedProperty = new HashSet<string>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<Type, Func<object, string>> serialised =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<object, string>> serialisedProperty =
            new Dictionary<string, Func<object, string>>();

        private CultureInfo cultureType = CultureInfo.CurrentCulture;

        private readonly Dictionary<string, Func<string, string>> trimmer =
            new Dictionary<string, Func<string, string>>();

        private Func<string, string> trim = s => s;

        private HashSet<object> alreadyBe = new HashSet<object>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo =
                ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            notSerialisedProperty.Add(propInfo.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            notSerialisedTypeOfProperty.Add(typeof(TPropType));
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

            var type = obj.GetType();
            if (type == typeof(string))
            {
                if (serialised.ContainsKey(type))
                    return trim(serialised[type](obj)) + Environment.NewLine;
                return trim((string) obj) + Environment.NewLine;
            }

            if (serialised.ContainsKey(type))
            {
                return serialised[type](obj) + Environment.NewLine;
            }

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (IsNotSerializableProperty(propertyInfo.Name, propertyInfo.PropertyType))
                    continue;
                if (trimmer.ContainsKey(propertyInfo.Name) || serialisedProperty.ContainsKey(propertyInfo.Name))
                {
                    var serialised = SerialiseProperty(propertyInfo.Name, propertyInfo.GetValue(obj));
                    sb.Append($"{indentation}{propertyInfo.Name} = {serialised}{Environment.NewLine}");
                    continue;
                }

                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        private bool IsNotSerializableProperty(string propertyName, Type propertyType) =>
            notSerialisedTypeOfProperty.Contains(propertyType) ||
            notSerialisedProperty.Contains(propertyName);

        private string SerialiseProperty(string propertyName, object propertyValue)
        {
            if (trimmer.ContainsKey(propertyName) && serialisedProperty.ContainsKey(propertyName))
                return trimmer[propertyName](serialisedProperty[propertyName](propertyValue));
            return trimmer.ContainsKey(propertyName) 
                ? trimmer[propertyName]((string) propertyValue) 
                : serialisedProperty[propertyName](propertyValue);
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.Serialised => serialised;
        Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.SerialisedProperty => serialisedProperty;

        CultureInfo IPrintingConfig<TOwner>.CultureType
        {
            get => cultureType;
            set => cultureType = value;
        }

        Dictionary<string, Func<string, string>> IPrintingConfig<TOwner>.Trimmer => trimmer;

        Func<string, string> IPrintingConfig<TOwner>.Trim
        {
            get => trim;
            set => trim = value;
        }
    }
}