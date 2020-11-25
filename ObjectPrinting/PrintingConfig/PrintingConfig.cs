using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting.PrintingConfig
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<string> excludedProperties;
        private readonly Dictionary<Type, Func<object, string>> typesPrintingMethods;
        private readonly Dictionary<string, Func<object, string>> propertiesPrintingMethods;
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };


        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypesPrintingMethods 
            => typesPrintingMethods;
        Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.PropertiesPrintingMethods 
            => propertiesPrintingMethods;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<string>();
            typesPrintingMethods = new Dictionary<Type, Func<object, string>>();
            propertiesPrintingMethods = new Dictionary<string, Func<object, string>>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, PropertyInfo propertyInfo = null)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            var sb = new StringBuilder();
            var objectPrinting = GetPrintedObject(obj, type, propertyInfo);
            sb.Append(objectPrinting + Environment.NewLine);
            
            if (finalTypes.Contains(type))
                return objectPrinting + Environment.NewLine;
            
            var identation = new string('\t', nestingLevel + 1);
            foreach (var propInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propInfo.PropertyType)
                    || excludedProperties.Contains(propInfo.Name)) continue;
                var printedObject = PrintToString(
                    propInfo.GetValue(obj), nestingLevel + 1, propInfo);
                sb.Append($"{identation}{printedObject}");
            }
            
            return sb.ToString();
        }

        private string GetPrintedObject(object obj, Type type, PropertyInfo propertyInfo)
        {
            var objectPrinting = type.Name;
            var propertyName = propertyInfo?.Name;
            if (typesPrintingMethods.TryGetValue(type, out var printMethod))
                return printMethod(obj);
            if (propertyInfo != null && 
                propertiesPrintingMethods.TryGetValue(propertyName, out printMethod))
                return printMethod(obj);
            if (finalTypes.Contains(type))
                return $"{propertyName} = {obj}";
            return objectPrinting;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            excludedProperties.Add(GetPropertyName(memberSelector));
            return this;
        }
        
        public PrintingConfig<TOwner> ExcludingPropertyWithType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> ExcludingPropertyWithTypes(params Type[] types)
        {
            excludedTypes.UnionWith(types);
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> PrintProperty<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyName(memberSelector));
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> PrintProperty<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        private string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo =
                ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            return propertyInfo?.Name;
        }
    }
}