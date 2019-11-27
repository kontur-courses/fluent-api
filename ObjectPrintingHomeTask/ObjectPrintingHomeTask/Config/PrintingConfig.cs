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

        public string PrintToString(TOwner obj) => PrintToString(obj, 0, new Dictionary<object, int>());

        private string PrintToString(object obj, int nestingLevel, Dictionary<object, int> serializedItemsIndexes, PropertyInfo property = default)
        {
            //TODO apply configurations
            if (obj == null)
                return "null";

            if (serializedItemsIndexes.TryGetValue(obj, out var index))
                return $"{obj.GetType().Name} {index}";

            if (finalTypes.Contains(obj.GetType()))
            {
                var toPrint = GetSerializedObject(obj, property);
                return (double.TryParse(toPrint.ToString(), out var value) ? value.ToString(culture) : toPrint.ToString());
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine($"{type.Name} {serializedItemsIndexes.Count}");
            serializedItemsIndexes.Add(obj, serializedItemsIndexes.Count);
            foreach (var propertyInfo in type.GetProperties()
                .Where(p => !(excludedTypes.Contains(p.PropertyType) || excludedProperties.Contains(p) ||
                              p.GetMethod == null)))
            {
                var propertyString = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, serializedItemsIndexes, propertyInfo);
                sb.Append($"{identation}{propertyInfo.Name} = {propertyString.Trim()}{Environment.NewLine}");
            }

            return sb.ToString();
        }

        private object GetSerializedObject(object obj, PropertyInfo property)
        {
            var objectToPrint = obj;
            if (obj.GetType().GetInterfaces().Contains(typeof(IConvertible)))
                objectToPrint = ((IConvertible) obj).ToString(culture);

            objectToPrint = property != null && typesRules.TryGetValue(property.PropertyType, out var typeRule)
                ? typeRule.DynamicInvoke(obj).ToString()
                : objectToPrint;

            return property != null && propertiesRules.TryGetValue(property, out var propertyRule)
                ? propertyRule.DynamicInvoke(obj)
                : objectToPrint;
        }

        void IPrintingConfig.AddChangedType(Type type, Delegate del) => typesRules[type] = del;
        

        void IPrintingConfig.AddChangedProperty(PropertyInfo propertyInfo, Delegate del) 
            => propertiesRules[propertyInfo] = del;


        void IPrintingConfig.ChangeCulture(CultureInfo culture) => this.culture = culture;
    }
}
