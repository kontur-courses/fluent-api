using System;
using System.Collections;
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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typePrintingFormat = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> propertyPrintingFormat = new Dictionary<string, Delegate>();
        private int nestingLevel = int.MaxValue;

        public PrintingConfig<TOwner> NestingLevel(int level)
        {
            nestingLevel = level;
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression) memberSelector.Body;
            excludedProperties.Add(memberExpression.Member.Name);
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression) memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberExpression.Member.Name);
        }
        
        public string PrintToString(TOwner obj)
        {
            if (obj.GetType().GetInterface(nameof(ICollection)) == null)
                return PrintToString(obj, 0);
            var result = new StringBuilder();
            foreach (var elem in (ICollection)obj)
            {
                result.Append(PrintToString(elem, 0));
            }

            return result.ToString();
        }


        private string GetValue(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var unformattedValue = propertyInfo.GetValue(obj);
            var value = GetFormatted(propertyInfo, unformattedValue);

            if (nestingLevel == this.nestingLevel)
                return value ?? $"{unformattedValue.GetType().Name}{Environment.NewLine}Nesting limit reached";
            
            return value ?? PrintToString(unformattedValue, nestingLevel + 1);
        }

        private string GetFormatted(PropertyInfo propertyInfo, object unformattedValue)
        {
            string value = null;
            var type = propertyInfo.PropertyType;
            var name = propertyInfo.Name;
            if (typePrintingFormat.ContainsKey(propertyInfo.PropertyType))
                value = InvokeFormatting(typePrintingFormat[type], unformattedValue);

            if (propertyPrintingFormat.ContainsKey(propertyInfo.Name))
                value = InvokeFormatting(propertyPrintingFormat[name], value ?? unformattedValue);
            
            if (typeCultures.ContainsKey(propertyInfo.PropertyType))
                value = FormatTypeCulture(type, value ?? unformattedValue);
            
            return value;
        }
        
        private string FormatTypeCulture(Type propertyType, object value) =>
            string.Format(typeCultures[propertyType], "{0}", value) + Environment.NewLine; 
    

        private string InvokeFormatting(Delegate func, object value) =>
            func.DynamicInvoke(value) + Environment.NewLine;
        

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {   
                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if (excludedProperties.Contains(propertyInfo.Name)) continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          GetValue(obj, propertyInfo, nestingLevel));
            }
            return sb.ToString();
        }

        internal void AddTypePrintingSettings(Type type, Delegate settings)
        {
            typePrintingFormat[type] = settings;
        }       
        
        internal void AddPropertyPrintingSettings(string name, Delegate settings)
        {
            propertyPrintingFormat[name] = settings;
        }

        internal void AddTypeCulture(Type type, CultureInfo cultureInfo)
        {
            typeCultures[type] = cultureInfo;
        }
    }
}