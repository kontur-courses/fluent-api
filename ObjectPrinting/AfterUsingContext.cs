using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class AfterUsingContext<TOwner, TPrevPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyPrintingConfig<TOwner, TPrevPropType> propertyPrintingConfig;

        public AfterUsingContext(
            PrintingConfig<TOwner> printingConfig, PropertyPrintingConfig<TOwner, TPrevPropType> propertyPrintingConfig)
        {
            this.printingConfig = printingConfig;
            this.propertyPrintingConfig = propertyPrintingConfig;
        }

        public PrintingConfig<TOwner> TrimmedToLength(int prefixLength)
        {
            var propertyPrintingConfigAsInterface = propertyPrintingConfig as IPropertyPrintingConfig<TOwner, TPrevPropType>;
            var previousPrintingFunc = propertyPrintingConfigAsInterface.PrintingFunction;
            propertyPrintingConfig.Using(prop =>
            {
                var previousResult = previousPrintingFunc(prop);
                return previousResult.Substring(0, Math.Min(prefixLength, previousResult.Length));
            });

            return propertyPrintingConfigAsInterface.PrintingConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return printingConfig.Excluding<TPropType>();
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            return printingConfig.Excluding(propertySelector);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return printingConfig.Printing<TPropType>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            return printingConfig.Printing(propertySelector);
        }

        public string PrintToString(TOwner obj)
        {
            return printingConfig.PrintToString(obj);
        }
    }
}
