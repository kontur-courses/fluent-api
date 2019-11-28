using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            parent.DigitTypeToCulture[typeof(int)] = cultureInfo;  
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            parent.DigitTypeToCulture[typeof(double)] = cultureInfo;
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, float> config, CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            parent.DigitTypeToCulture[typeof(float)] = cultureInfo;
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config, int amount)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            var propertyInfo = (config as IPropertyPrintingConfig<TOwner>).ConfiguredProperty;

            parent.PropertyToLength[propertyInfo] = amount;

            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetElementFormat<TOwner, TElement>(
            this PropertyPrintingConfig<TOwner, TElement[]> config, 
            Func<TElement, string> serializationFunc)
        {
            return SetElementFormat(config, typeof(TElement[]), serializationFunc);
        }

        public static PrintingConfig<TOwner> SetElementFormat<TOwner, TElement>(
            this PropertyPrintingConfig<TOwner, List<TElement>> config, 
            Func<TElement, string> serializationFunc)
        {
            return SetElementFormat(config, typeof(List<TElement>), serializationFunc);
        }

        public static PrintingConfig<TOwner> SetElementFormat<TOwner, TKey, TValue>(
            this PropertyPrintingConfig<TOwner, Dictionary<TKey, TValue>> config,
            Func<(TKey, TValue), string> serializationFunc)
        {
            return SetElementFormat(config, typeof(Dictionary<TKey, TValue>), serializationFunc);
        }

        private static PrintingConfig<TOwner> SetElementFormat<TOwner, TCollection, TElement>(
            this PropertyPrintingConfig<TOwner, TCollection> config, Type collectionType,
            Func<TElement, string> serializationFunc)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            var propertyInfo = (config as IPropertyPrintingConfig<TOwner>).ConfiguredProperty;

            if (propertyInfo is null)
                parent.CollectionTypeToElementFormatter[collectionType] = serializationFunc;
            else
                parent.CollectionPropertyToElementFormatter[propertyInfo] = serializationFunc;

            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static string Print<TOwner>(
            this TOwner formattedObject)
        {
            return new PrintingConfig<TOwner>(formattedObject).Print();
        }

        public static PrintingConfig<TOwner> ConfigureFormat<TOwner>(
            this TOwner formattedObject)
        {
            return new PrintingConfig<TOwner>(formattedObject);
        }
    }
}
