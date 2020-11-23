using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private ImmutableHashSet<PropertyInfo> excludedProperties = ImmutableHashSet<PropertyInfo>.Empty;
        private ImmutableHashSet<Type> excludedTypes = ImmutableHashSet<Type>.Empty;

        private ImmutableDictionary<PropertyInfo, Func<object, string>> propertiesSerialization =
            ImmutableDictionary<PropertyInfo, Func<object, string>>.Empty;

        private ImmutableDictionary<Type, Func<object, string>> typesSerialization =
            ImmutableDictionary<Type, Func<object, string>>.Empty;

        ImmutableDictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypesSerialization
        {
            get => typesSerialization;
            set => typesSerialization = value;
        }

        ImmutableDictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.PropsSerialization
        {
            get => propertiesSerialization;
            set => propertiesSerialization = value;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return PropertyPrintingConfig<TOwner, TPropType>.For<TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedProp = (PropertyInfo) ((MemberExpression) memberSelector.Body).Member;
            return PropertyPrintingConfig<TOwner, TPropType>.For(this, selectedProp);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedProp = (PropertyInfo) ((MemberExpression) memberSelector.Body).Member;
            excludedProperties = excludedProperties.Add(selectedProp);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes = excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string PrintToString(ICollection<TOwner> objCollection)
        {
            var sb = new StringBuilder();
            sb.AppendLine(objCollection.GetType().Name);

            foreach (var obj in objCollection)
                sb.AppendLine(PrintToString(obj, 1));

            return sb.ToString();
        }

        public string PrintToString<TKey>(IDictionary<TKey, TOwner> objDictionary)
        {
            var sb = new StringBuilder();
            sb.AppendLine(objDictionary.GetType().Name);

            foreach (var (key, value) in objDictionary)
                sb.AppendLine($"{key} : {PrintToString(value, 1)}");

            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()) || typesSerialization.ContainsKey(obj.GetType()))
            {
                var valueToPrint = typesSerialization.ContainsKey(obj.GetType())
                    ? typesSerialization[obj.GetType()](obj)
                    : obj;

                return valueToPrint.ToString();
            }

            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(identation + type.Name);
            identation += '\t';
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);

                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;

                var valueToPrint = GetValueToPrint(obj, nestingLevel, propertyValue, propertyInfo);

                sb.Append($"{identation}{propertyInfo.Name} = {valueToPrint}");
            }

            return sb.ToString();
        }

        private string GetValueToPrint(object obj, int nestingLevel, object propertyValue, PropertyInfo propertyInfo)
        {
            string valueToPrint;
            if (propertyValue == obj)
                valueToPrint = "self";
            else
                valueToPrint = propertiesSerialization.ContainsKey(propertyInfo)
                    ? propertiesSerialization[propertyInfo](propertyValue)
                    : PrintToString(propertyValue, nestingLevel + 1);

            if (!valueToPrint.EndsWith("\r\n")) valueToPrint += Environment.NewLine;

            return valueToPrint;
        }
    }

    public interface IPrintingConfig<TOwner>
    {
        ImmutableDictionary<Type, Func<object, string>> TypesSerialization { get; set; }
        ImmutableDictionary<PropertyInfo, Func<object, string>> PropsSerialization { get; set; }
    }
}