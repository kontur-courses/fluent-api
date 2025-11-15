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
        
        private readonly Dictionary<Type, Func<object, string>> _typeSerializers;
        private readonly Dictionary<string, Func<TOwner, string>> _propertySerializers;
        private readonly HashSet<(string name, object current, object parent)> _printedObjects = []; 

        public Dictionary<Type, Func<object, string>> GetTypeSerializers => _typeSerializers;
        public Dictionary<string, Func<TOwner, string>> GetPropertySerializers => _propertySerializers;


        public PrintingConfig() : this(null)
        { }
        
        private PrintingConfig(
            HashSet<Type>? excludedTypes = null,
            HashSet<string>? excludedProperties = null,
            Dictionary<Type, Func<object, string>>? typeSerializers = null,
            Dictionary<string, Func<TOwner, string>>? propertySerializers = null
        )
        {
            _excludedTypes =  excludedTypes ?? new HashSet<Type>();
            _excludedProperties = excludedProperties ?? new HashSet<string>();
            _typeSerializers = typeSerializers ?? new Dictionary<Type, Func<object, string>>();
            _propertySerializers = propertySerializers ?? new Dictionary<string, Func<TOwner, string>>();
        }
        
        public PrintingConfig<TOwner> BuildPrintingConfig(
            HashSet<Type>? excludedTypes = null,
            HashSet<string>? excludedProperties = null,
            Dictionary<Type, Func<object, string>>? typeSerializers = null,
            Dictionary<string, Func<TOwner, string>>? propertySerializers = null
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
            return new PropertyPrintingConfig<TOwner, TPropType>(
                BuildPrintingConfig(),
                null!
            );
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(
                BuildPrintingConfig(),
                memberSelector
            );
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var excludedProperties = new HashSet<string>(_excludedProperties)
            {
                GetPropertyNameFromSelector(memberSelector)
            };

            return BuildPrintingConfig(
                excludedProperties: excludedProperties
            );
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var excludedTypes = new HashSet<Type>(_excludedTypes)
            {
                typeof(TPropType)
            };

            return BuildPrintingConfig(
                excludedTypes: excludedTypes
            );
        }
        
        public string GetPropertyNameFromSelector<TProperty>(
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
            if (obj == null!)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (_typeSerializers.TryGetValue(type, out var serializer))
            {
                return serializer(obj) + Environment.NewLine;
            }
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (IsExcluded(propertyInfo)) {
                    continue;
                }

                string toPrint;
                if (_propertySerializers.TryGetValue(propertyInfo.Name, out var propertySerializer) && type == typeof(TOwner))
                {
                    toPrint = propertySerializer((TOwner)obj) + Environment.NewLine;
                }
                else
                {
                    toPrint = "Printed earlier"  + Environment.NewLine;
                    if (!_printedObjects.Contains((propertyInfo.Name, propertyInfo.GetValue(obj)!, obj)))
                    {
                        _printedObjects.Add((propertyInfo.Name, propertyInfo.GetValue(obj)!, obj));
                        toPrint = PrintToString(propertyInfo.GetValue(obj)!,
                            nestingLevel + 1);
                    }
                }

                sb.Append(identation + propertyInfo.Name + " = " + toPrint);
            }
            return sb.ToString();
        }
        
        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return _excludedProperties.Contains(propertyInfo.Name)
                   || _excludedTypes.Contains(propertyInfo.PropertyType);
        }
    }
}