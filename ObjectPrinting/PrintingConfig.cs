using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _excludedTypes;
        private readonly HashSet<string> _excludedProperties;
        
        private readonly Dictionary<Type, Delegate> _typeSerializers;
        private readonly Dictionary<string, Delegate> _propertySerializers;

        public Dictionary<Type, Delegate> GetTypeSerializers()
        {
            return _typeSerializers;
        }
        
        public Dictionary<string, Delegate> GetPropertySerializers()
        {
            return _propertySerializers;
        }

        public PrintingConfig() : this(null)
        { }
        
        private PrintingConfig(
            HashSet<Type>? excludedTypes = null,
            HashSet<string>? excludedProperties = null,
            Dictionary<Type, Delegate>? typeSerializers = null,
            Dictionary<string, Delegate>? propertySerializers = null
        )
        {
            _excludedTypes =  excludedTypes ?? new HashSet<Type>();
            _excludedProperties = excludedProperties ?? new HashSet<string>();
            _typeSerializers = typeSerializers ?? new Dictionary<Type, Delegate>();
            _propertySerializers = propertySerializers ?? new Dictionary<string, Delegate>();
        }
        
        public PrintingConfig<TOwner> BuildPrintingConfig(
            HashSet<Type>? excludedTypes = null,
            HashSet<string>? excludedProperties = null,
            Dictionary<Type, Delegate>? typeSerializers = null,
            Dictionary<string, Delegate>? propertySerializers = null
        ) {
            return new PrintingConfig<TOwner>(
                excludedTypes ?? _excludedTypes,
                excludedProperties ?? _excludedProperties,
                typeSerializers ?? _typeSerializers,
                propertySerializers ?? _propertySerializers
            );
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var typeSerializers = new Dictionary<Type, Delegate>(_typeSerializers);
            typeSerializers.Add(typeof(TPropType), null!);
            
            return new PropertyPrintingConfig<TOwner, TPropType>(
                BuildPrintingConfig(typeSerializers: typeSerializers)
            );
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertySerializers = new Dictionary<string, Delegate>(_propertySerializers);
            propertySerializers.Add(GetPropertyName(memberSelector), null!);
            
            return new PropertyPrintingConfig<TOwner, TPropType>(
                BuildPrintingConfig(propertySerializers: propertySerializers)
            );
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var excludedProperties = new HashSet<string>(_excludedProperties);
        
            excludedProperties.Add(GetPropertyName(memberSelector));
            
            return BuildPrintingConfig(
                excludedProperties: excludedProperties
            );
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var excludedTypes = new HashSet<Type>(_excludedTypes);
        
            excludedTypes.Add(typeof(TPropType));
            
            return BuildPrintingConfig(
                excludedTypes: excludedTypes
            );
        }
        
        private string GetPropertyName<TProperty>(
            Expression<Func<TOwner, TProperty>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    return propertyInfo.Name;
                }
            }
        
            throw new ArgumentException(
                "Expression must be a property selector: x => x.PropertyName", 
                nameof(propertySelector));
        }

        public string PrintToString(TOwner obj, int nestingLevel = 0)
        {
            return PrintToString((object)obj!, nestingLevel);
        }

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
                if (
                    _excludedProperties.Contains(propertyInfo.Name)
                    || _excludedTypes.Contains(propertyInfo.PropertyType)
                ) {
                    continue;
                }
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}