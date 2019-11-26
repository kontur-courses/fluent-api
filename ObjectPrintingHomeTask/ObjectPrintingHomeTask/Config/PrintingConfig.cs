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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<object, int> serializedItemsIndexes = new Dictionary<object, int>();
        private readonly Dictionary<Type, Delegate> typesRules = new Dictionary<Type, Delegate>();
        private readonly Dictionary<PropertyInfo, Delegate> propertiesRules = new Dictionary<PropertyInfo, Delegate>();
        private CultureInfo culture = CultureInfo.CurrentCulture;

        readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
            => new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>
            (Expression<Func<TOwner, TPropType>> memberSelector)
            => memberSelector.Body.ToString().Split('.').Count() == 2
                ? new PropertyPrintingConfig<TOwner, TPropType>(this,
                    (PropertyInfo) ((MemberExpression) memberSelector.Body).Member)
                : throw new ArgumentException("You can't serialize nested types");


        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.ToString().Split('.').Count() != 2)
                throw new ArgumentException("You can't exclude from nested type");
            excludedProperties.Add((PropertyInfo) ((MemberExpression) memberSelector.Body).Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel, PropertyInfo property = default)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (serializedItemsIndexes.TryGetValue(obj, out var index))
                return $"{obj.GetType().Name} {index}{Environment.NewLine}";

            if (finalTypes.Contains(obj.GetType()))
            {
                var toPrint = GetSerializedObject(obj, property);
                return (double.TryParse(toPrint.ToString(), out var value) ? value.ToString(culture) : toPrint) +
                       Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine($"{type.Name} {serializedItemsIndexes.Count}");
            lock (serializedItemsIndexes)
                serializedItemsIndexes.Add(obj, serializedItemsIndexes.Count);
            foreach (var propertyInfo in type.GetProperties()
                .Where(p => !(excludedTypes.Contains(p.PropertyType) || excludedProperties.Contains(p) ||
                              p.GetMethod == null)))
            {
                var propertyString = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, propertyInfo);
                sb.Append($"{identation}{propertyInfo.Name} = {propertyString}");
            }

            return sb.ToString();
        }

        private object GetSerializedObject(object obj, PropertyInfo property)
        {
            var objectToPrint = typesRules.TryGetValue(property.PropertyType, out var typeRule)
                ? typeRule.DynamicInvoke(obj)
                : obj;

            return propertiesRules.TryGetValue(property, out var propertyRule)
                ? propertyRule.DynamicInvoke(objectToPrint)
                : objectToPrint;
        }

        void IPrintingConfig.AddChangedType(Type type, Delegate del)
        {
            lock (typesRules)
                typesRules[type] = del;
        }

        void IPrintingConfig.AddChangedProperty(PropertyInfo propertyInfo, Delegate del)
        {
            lock (propertiesRules)
                propertiesRules[propertyInfo] = del;
        }

        void IPrintingConfig.ChangeCulture(CultureInfo culture) => this.culture = culture;
    }
}
