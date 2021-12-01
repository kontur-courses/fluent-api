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
        private readonly PropertyInfo _propInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propInfo)
        {
            _printingConfig = printingConfig;
            _propInfo = propInfo;
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
            ChooseDictToUpdate(printWrapper);
            return _printingConfig;
        }

        private void ChooseDictToUpdate(Func<object, string> printWrapper)
        {
            if (_propInfo == null)
                _printingConfig.typeAlterSerializations[typeof(TPropType)] = printWrapper;
            else
                _printingConfig.nameAlterSerializations[_propInfo.Name] = printWrapper;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var typeToCheck = typeof(TPropType);
            var toString = GetToStringWithCulture(typeToCheck);
            var serializeWithCulture = GetSerializationWithCulture(culture, typeToCheck, toString);
            _printingConfig.culturedSerializations[typeToCheck] = serializeWithCulture;
            return _printingConfig;
        }

        private Func<object, string> GetSerializationWithCulture(
            CultureInfo culture, Type typeToCheck, MethodInfo toString)
        {
            return value =>
            {
                var serializedValue = toString.Invoke(value, new object[] { culture });
                if (_printingConfig.typeAlterSerializations.ContainsKey(typeToCheck))
                {
                    var serializeLike = _printingConfig.typeAlterSerializations[typeToCheck];
                    serializedValue = serializeLike.DynamicInvoke(serializedValue);
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
}