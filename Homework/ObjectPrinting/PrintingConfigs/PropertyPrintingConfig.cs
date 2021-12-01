using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> _memberSelector;
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            _printingConfig = printingConfig;
            _memberSelector = memberSelector;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Func<object, string> printWrapper = value =>
            {
                return print((TPropType)value);
            };
            _printingConfig.alternativeSerializations[typeof(TPropType)] = printWrapper;
            return _printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var typeToCheck = typeof(TPropType);
            var toString = GetToStringWithCulture(typeToCheck);
            Func<object, string> serializeWithCulture = 
                BuildSerializationWithCulture(culture, typeToCheck, toString);
            _printingConfig.culturedSerializations[typeToCheck] = serializeWithCulture;
            return _printingConfig;
        }

        private Func<object, string> BuildSerializationWithCulture(
            CultureInfo culture, Type typeToCheck, MethodInfo toString)
        {
            return value =>
            {
                var serializedValue = toString.Invoke(value, new object[] { culture });
                if (_printingConfig.alternativeSerializations.ContainsKey(typeToCheck))
                {
                    var serializeLike = _printingConfig.alternativeSerializations[typeToCheck];
                    serializedValue = serializeLike(serializedValue);
                }
                return (string)serializedValue;
            };
        }

        private static MethodInfo GetToStringWithCulture(Type typeToCheck)
        {
            return typeToCheck.GetMethod("ToString", new Type[] { typeof(CultureInfo) })
                ?? throw new Exception("\nGiven type has no customizing culture!");
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}