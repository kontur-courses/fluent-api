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
        private readonly Expression<Func<TOwner, TPropType>> member;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector = null)
        {
            this.printingConfig = printingConfig;
            member = memberSelector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (member is null)
                ((IPrintingConfig<TOwner>) printingConfig).Serialised[typeof(TPropType)] = x => print((TPropType) x);
            else
            {
                var serialisedProperty = ((IPrintingConfig<TOwner>) printingConfig).SerialisedProperty;
                var propInfo = ((MemberExpression) member.Body).Member as PropertyInfo;
                serialisedProperty[propInfo.Name] = x => print((TPropType) x);
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var cultureTypes = ((IPrintingConfig<TOwner>) printingConfig).CultureTypes;
            var typeOfTPropType = this.GetType().GetGenericArguments()[1];
            cultureTypes[typeOfTPropType] = culture;
            
            return printingConfig;
        }
        
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        Expression<Func<TOwner, TPropType>> IPropertyPrintingConfig<TOwner, TPropType>.Member => member;
    }
}