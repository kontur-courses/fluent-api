using System;
using System.Collections;
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
        
        private readonly HashSet<PrintedObject> _printedObjects = []; 
        private readonly Type[] finalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        ];
        
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

        internal PrintingConfig<TOwner> CopyWithChanges(
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
                CopyWithChanges(),
                null!
            );
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(
                CopyWithChanges(),
                memberSelector
            );
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var excludedProperties = new HashSet<string>(_excludedProperties)
            {
                GetPropertyNameFromSelector(memberSelector)
            };

            return CopyWithChanges(
                excludedProperties: excludedProperties
            );
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var excludedTypes = new HashSet<Type>(_excludedTypes)
            {
                typeof(TPropType)
            };

            return CopyWithChanges(
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

        public string ToString(TOwner obj, int nestingLevel = 0)
        {
            return ToString((object)obj!, nestingLevel);
        }

        private string ToString(object obj, int nestingLevel)
        {
            if (obj == null!)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (_typeSerializers.TryGetValue(type, out var serializer))
            {
                return serializer(obj) + Environment.NewLine;
            }
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var collectionToString = CollectionToString(obj, identation, nestingLevel);
            if (collectionToString is not null)
                sb.Append(collectionToString);

            foreach (var propertyInfo in GetOrderedProperties(obj)) 
            {
                if (IsExcluded(propertyInfo)) {
                    continue;
                }
                var propertyForPrint = PropertyToString(obj, nestingLevel, propertyInfo);
                if (propertyForPrint is not null)
                    sb.Append(identation + propertyInfo.Name + " = " + propertyForPrint);
            }
            return sb.ToString();
        }

        private IOrderedEnumerable<PropertyInfo> GetOrderedProperties(object obj)
        {
            return obj.GetType()
                .GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0)
                .OrderBy(x =>
                {
                    var value = x.GetValue(obj);
                    if (value is null)
                        return int.MaxValue;
                    return value.GetType().GetProperties().Length;
                });
        }

        private string? CollectionToString(object obj, string identation, int nestingLevel)
        {
            if (IsCollection(obj.GetType()))
            {
                var sb = new StringBuilder();
                sb.AppendLine(identation + "{");
                foreach (var item in (IEnumerable)obj)
                {
                    var itemToPrint = ToString(item, nestingLevel + 1);
                    if (itemToPrint is not null)
                        sb.Append(identation + itemToPrint);
                }
                sb.AppendLine(identation + "}");
                return sb.ToString();
            }

            return null;
        }

        private string? PropertyToString(object obj, int nestingLevel, PropertyInfo propertyInfo)
        {
            if (
                _propertySerializers.TryGetValue(propertyInfo.Name, out var propertySerializer) 
                && obj.GetType() == typeof(TOwner)
            ) {
                return propertySerializer((TOwner)obj) + Environment.NewLine;
            }
            
            var printedObject = new PrintedObject(propertyInfo.Name, propertyInfo.GetValue(obj)!, obj);
            if (!_printedObjects.Contains(printedObject))
            {
                _printedObjects.Add(printedObject);
                return ToString(
                    propertyInfo.GetValue(obj)!,
                    nestingLevel + 1);
            }

            return null;
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return _excludedProperties.Contains(propertyInfo.Name)
                   || _excludedTypes.Contains(propertyInfo.PropertyType);
        }
        
        private bool IsCollection(Type type)
        {
            if (type == typeof(string))
                return false;
    
            if (typeof(IEnumerable).IsAssignableFrom(type))
                return true;
    
            if (type.IsGenericType && 
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return true;
    
            return false;
        }
    }
}