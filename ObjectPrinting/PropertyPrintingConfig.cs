using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private Expression<Func<TOwner, TPropType>>? _memberSelector;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> memberSelector)
        {
            _memberSelector = memberSelector;
            _printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (_memberSelector == null)
            {
                return UsingForType(print);
            }
            else
            {
                if (typeof(TPropType)  == typeof(string))
                    return UsingForProperty(print as Func<string, string>);
                return UsingForProperty(print);
            }
        }
        
        private PrintingConfig<TOwner> UsingForType(Func<TPropType, string> print)
        {
            var serializers = new Dictionary<Type, Func<object, string>>(_printingConfig.GetTypeSerializers);
            Func<object, string> serializer = obj => print((TPropType)obj);

            if (serializers.ContainsKey(typeof(TPropType)))
            {
                var oldSerializer =  serializers[typeof(TPropType)];
                serializers[typeof(TPropType)] = obj =>  serializer(oldSerializer(obj));
            } 
            else
                serializers[typeof(TPropType)] = serializer;
            return _printingConfig.CopyWithChanges(typeSerializers: serializers);
        }
        
        private PrintingConfig<TOwner> UsingForProperty(Func<TPropType, string> print)
        {
            if (_memberSelector == null)
                return _printingConfig;
            
            var serializers = new Dictionary<string, Func<TOwner, string>>(_printingConfig.GetPropertySerializers);
            var propertyName = _printingConfig.GetPropertyNameFromSelector(_memberSelector);
            var memberSelector = _memberSelector.Compile();
            
            Func<TOwner, string> ownerSerializer = owner =>
            {
                TPropType propertyValue = memberSelector(owner);
                return print(propertyValue);
            };
            serializers[propertyName] = ownerSerializer;
            return _printingConfig.CopyWithChanges(propertySerializers: serializers);
        }

        private PrintingConfig<TOwner> UsingForProperty(Func<string, string> print)
        {
            if (_memberSelector == null)
                return _printingConfig;

            var serializers = new Dictionary<string, Func<TOwner, string>>(_printingConfig.GetPropertySerializers);
            var propertyName = _printingConfig.GetPropertyNameFromSelector(_memberSelector);
            if (!serializers.ContainsKey(propertyName))
            {
                return UsingForProperty(print as Func<TPropType, string>);
            }
            var oldSerializer =  serializers[propertyName];
            Func<TOwner, string> ownerSerializer = owner =>
            {
                return print(oldSerializer(owner));
            };
            serializers[propertyName] = ownerSerializer;
            return _printingConfig.CopyWithChanges(propertySerializers: serializers);
        }
        

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(TPropType).IsNumericType())
            {
                throw new InvalidOperationException($"{typeof(TPropType).Name} is not numeric.");
            }
            var print = (TPropType p) => ((IFormattable)p!).ToString(null, culture);
            
            return Using(print);
        }

        public PrintingConfig<TOwner> ParentConfig => _printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}