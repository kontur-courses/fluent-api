using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes;
        private readonly Dictionary<PropertyInfo, Func<object, string>> specialPrintingFunctionsForProperties;
        private readonly PropertyInfo property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Dictionary<PropertyInfo, 
            Func<object, string>> specialPrintingFunctionsForProperties, PropertyInfo property)
        {
            this.printingConfig = printingConfig;
            this.specialPrintingFunctionsForProperties = specialPrintingFunctionsForProperties;
            this.property = property;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, 
            Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes)
        {
            this.printingConfig = printingConfig;
            this.specialPrintingFunctionsForTypes = specialPrintingFunctionsForTypes;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (property != null)
            {
                specialPrintingFunctionsForProperties[property] = o => print((TPropType) o);
                return printingConfig;
            }
            specialPrintingFunctionsForTypes[typeof(TPropType)] = o => print((TPropType) o);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        Dictionary<Type, Func<object, string>> IPropertyPrintingConfig<TOwner, TPropType>.SpecialPrintingFunctionsForTypes =>
            specialPrintingFunctionsForTypes;

        Dictionary<PropertyInfo, Func<object, string>> IPropertyPrintingConfig<TOwner, TPropType>.SpecialPrintingFunctionsForProperties =>
            specialPrintingFunctionsForProperties;

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property => property;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Dictionary<Type, Func<object, string>> SpecialPrintingFunctionsForTypes { get; }
        Dictionary<PropertyInfo, Func<object, string>> SpecialPrintingFunctionsForProperties { get; }
        PropertyInfo Property { get; }
    }
}