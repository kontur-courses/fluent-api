using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly Dictionary<PropertyInfo, Func<object, string>> propertiesSerialization =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> typesSerialization =
            new Dictionary<Type, Func<object, string>>();

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypesSerialization => typesSerialization;

        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.PropsSerialization =>
            propertiesSerialization;

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
            excludedProperties.Add(selectedProp);
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

        public string PrintToString(ICollection<TOwner> objCollection)
        {
            var sb = new StringBuilder();
            sb.AppendLine(objCollection.GetType().Name);

            foreach (var obj in objCollection)
                sb.AppendLine(PrintToString(obj, 1));

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

                string valueToPrint;
                if (propertyValue == obj)
                    valueToPrint = "self";
                else
                    valueToPrint = propertiesSerialization.ContainsKey(propertyInfo)
                        ? propertiesSerialization[propertyInfo](propertyValue)
                        : PrintToString(propertyValue, nestingLevel + 1);

                sb.AppendLine($"{identation}{propertyInfo.Name} = {valueToPrint}");
            }

            return sb.ToString();
        }
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> TypesSerialization { get; }
        Dictionary<PropertyInfo, Func<object, string>> PropsSerialization { get; }
    }
}