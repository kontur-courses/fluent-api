using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrintingHomeTask.PropertyConfig;

namespace ObjectPrintingHomeTask.Config
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private List<Type> excludedTypes = new List<Type>();
        private List<(int deep, PropertyInfo propertyInfo)> excludedProperties = new List<(int deep, PropertyInfo propertyInfo)>();
        private Dictionary<Type, List<Delegate>> typesRules = new Dictionary<Type, List<Delegate>>();
        private Dictionary<(int deep, PropertyInfo property), List<Delegate>> properties = new Dictionary<(int deep, PropertyInfo property), List<Delegate>>();
        private CultureInfo culture = CultureInfo.CurrentCulture;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() 
            => new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var deep = memberSelector.Body.ToString().Split('.').Length - 2;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, deep, (PropertyInfo)((MemberExpression)memberSelector.Body).Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var deep = memberSelector.Body.ToString().Split('.').Length - 2;
            excludedProperties.Add((deep, (PropertyInfo)((MemberExpression)memberSelector.Body).Member));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj) 
            => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel, PropertyInfo property = default)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                var toPrint = GetSerializedObject(obj, nestingLevel, property);
                return (double.TryParse(toPrint.ToString(), out var value) ? value.ToString(culture) : toPrint) + Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties()
                .Where(p => !(excludedTypes.Contains(p.PropertyType) || excludedProperties.Contains((nestingLevel, p)))))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, propertyInfo));
            }
            return sb.ToString();
        }

        private object GetSerializedObject(object obj, int nestingLevel, PropertyInfo property)
        {
            var objectToPrint = obj;

            if (property != null && typesRules.ContainsKey(property.PropertyType))
                typesRules[property.PropertyType]
                    .ForEach(function => objectToPrint = function.DynamicInvoke(obj));

            if (properties.ContainsKey((nestingLevel - 1, property)))
                properties[(nestingLevel - 1, property)]
                    .ForEach(function => objectToPrint = function.DynamicInvoke(obj));

            return objectToPrint;
        }

        void IPrintingConfig.AddChangedType(Type type, Delegate del)
        {
            if(!typesRules.ContainsKey(type))
                typesRules.Add(type, new List<Delegate>());
            typesRules[type].Add(del);
        }

        void IPrintingConfig.AddChangedProperty((int deep, PropertyInfo propertyInfo) key, Delegate del)
        {
            if(!properties.ContainsKey(key))
                properties.Add(key, new List<Delegate>());
            properties[key].Add(del);
        }

        void IPrintingConfig.ChangeCulture(CultureInfo culture)
        {
            this.culture = culture;
        }
    }
}
