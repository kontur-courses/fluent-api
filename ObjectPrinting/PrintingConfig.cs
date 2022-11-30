using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludingTypes = new List<Type>();
        private readonly List<string> excludingProperties = new List<string>();
        protected internal readonly Dictionary<Type, CultureInfo> CultureInfosForTypes = new Dictionary<Type, CultureInfo>();
        protected internal readonly Dictionary<Tuple<Type, string>, Func<object, string>> SpecialSerializations =
            new Dictionary<Tuple<Type, string>, Func<object, string>>();


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = (MemberExpression) memberSelector.Body;
            var property = (PropertyInfo) member.Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(property.Name, this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = (MemberExpression) memberSelector.Body;
            var property = (PropertyInfo) member.Member;
            excludingProperties.Add(property.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > 10)
                throw new ArgumentException("Cycling reference");
            if (obj == null)
                return "null" + Environment.NewLine;

            if (CultureInfosForTypes.ContainsKey(obj.GetType()))
                CultureInfo.CurrentCulture = CultureInfosForTypes[obj.GetType()];

            if (SpecialSerializations.ContainsKey(new Tuple<Type, string>(obj.GetType(), null)))
                return SpecialSerializations[new Tuple<Type, string>(obj.GetType(), null)](obj);

            if (PrintFinalType(obj) != null)
                return PrintFinalType(obj);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (CheckExcluding(propertyInfo))
                    continue;
                var pairPropertyTypeAndName = new Tuple<Type, string>(propertyInfo.PropertyType, propertyInfo.Name);

                sb.Append(identation + propertyInfo.Name + " = ");
                sb.Append(SpecialSerializations.ContainsKey(pairPropertyTypeAndName)
                    ? SpecialSerializations[pairPropertyTypeAndName](propertyInfo.GetValue(obj))
                    : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        public bool CheckExcluding(PropertyInfo propertyInfo)
        {
            return excludingProperties.Contains(propertyInfo.Name) ||
                   excludingTypes.Contains(propertyInfo.PropertyType);
        }

        public string PrintFinalType(object obj)
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            return null;
        }
    }
}